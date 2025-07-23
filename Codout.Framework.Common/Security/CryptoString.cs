using System;
using System.Security.Cryptography;
using System.Text;

namespace Codout.Framework.Common.Security;

/// <summary>
/// Implementação moderna para criptografia de strings usando AES-256-GCM (.NET 9.0)
/// </summary>
public static class CryptoString
{
    private const int SaltSize = 16;           // 128 bits
    private const int NonceSize = 12;          // 96 bits (recomendado para GCM)
    private const int TagSize = 16;            // 128 bits

    /// <summary>
    /// Criptografa uma string usando AES-256-GCM com derivação segura de chave
    /// </summary>
    /// <param name="plaintext">Texto a ser criptografado</param>
    /// <param name="password">Senha para derivação da chave</param>
    /// <param name="options">Opções de criptografia (opcional)</param>
    /// <returns>String criptografada em formato Base64</returns>
    /// <exception cref="ArgumentException">Quando parâmetros são inválidos</exception>
    /// <exception cref="CryptographicException">Quando há erro na criptografia</exception>
    public static string Encrypt(string plaintext, string password, CryptoOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext, nameof(plaintext));
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        options ??= CryptoOptions.Default;
        ValidatePassword(password, options);

        try
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var encryptedData = EncryptBytes(plaintextBytes, password, options);
            return Convert.ToBase64String(encryptedData);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is CryptographicException))
        {
            throw new CryptographicException("Erro durante a criptografia", ex);
        }
    }

    /// <summary>
    /// Descriptografa uma string criptografada com AES-256-GCM
    /// </summary>
    /// <param name="ciphertext">Texto criptografado em Base64</param>
    /// <param name="password">Senha para derivação da chave</param>
    /// <param name="options">Opções de criptografia (opcional)</param>
    /// <returns>Texto descriptografado</returns>
    /// <exception cref="ArgumentException">Quando parâmetros são inválidos</exception>
    /// <exception cref="CryptographicException">Quando há erro na descriptografia</exception>
    public static string Decrypt(string ciphertext, string password, CryptoOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext, nameof(ciphertext));
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        options ??= CryptoOptions.Default;
        ValidatePassword(password, options);

        try
        {
            // Normaliza Base64 (corrige espaços em '+')
            var normalizedCiphertext = ciphertext.Replace(' ', '+');
            var encryptedData = Convert.FromBase64String(normalizedCiphertext);
            var decryptedBytes = DecryptBytes(encryptedData, password, options);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Formato de ciphertext inválido", nameof(ciphertext), ex);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is CryptographicException))
        {
            throw new CryptographicException("Erro durante a descriptografia", ex);
        }
    }

    /// <summary>
    /// Criptografa dados binários usando AES-256-GCM
    /// </summary>
    /// <param name="plaintext">Dados a serem criptografados</param>
    /// <param name="password">Senha para derivação da chave</param>
    /// <param name="options">Opções de criptografia</param>
    /// <returns>Dados criptografados incluindo salt, nonce e tag</returns>
    public static byte[] EncryptBytes(ReadOnlySpan<byte> plaintext, string password, CryptoOptions options)
    {
        // Gera salt e nonce usando a API mais moderna
        Span<byte> salt = stackalloc byte[SaltSize];
        Span<byte> nonce = stackalloc byte[NonceSize];

        RandomNumberGenerator.Fill(salt);
        RandomNumberGenerator.Fill(nonce);

        // Deriva chave usando PBKDF2-SHA256
        using var key = SecureMemory.Allocate<byte>(32); // 256 bits para AES-256
        DeriveKey(password, salt, options, key.Span);

        // Prepara buffer de saída: salt + nonce + tag + ciphertext
        var outputSize = SaltSize + NonceSize + TagSize + plaintext.Length;
        var output = new byte[outputSize];
        var outputSpan = output.AsSpan();

        // Copia salt e nonce para o início do buffer
        salt.CopyTo(outputSpan[..SaltSize]);
        nonce.CopyTo(outputSpan.Slice(SaltSize, NonceSize));

        // Prepara spans para criptografia
        var tagSpan = outputSpan.Slice(SaltSize + NonceSize, TagSize);
        var ciphertextSpan = outputSpan[(SaltSize + NonceSize + TagSize)..];

        // Executa criptografia AES-GCM usando .NET 9.0 APIs
        using var aes = new AesGcm(key.Span, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertextSpan, tagSpan);

        return output;
    }

    /// <summary>
    /// Descriptografa dados binários usando AES-256-GCM
    /// </summary>
    /// <param name="ciphertext">Dados criptografados</param>
    /// <param name="password">Senha para derivação da chave</param>
    /// <param name="options">Opções de criptografia</param>
    /// <returns>Dados descriptografados</returns>
    /// <exception cref="CryptographicException">Quando dados estão corrompidos ou senha incorreta</exception>
    public static byte[] DecryptBytes(ReadOnlySpan<byte> ciphertext, string password, CryptoOptions options)
    {
        var minSize = SaltSize + NonceSize + TagSize;
        if (ciphertext.Length < minSize)
            throw new CryptographicException($"Dados criptografados muito pequenos. Mínimo: {minSize} bytes");

        // Extrai componentes do ciphertext
        var salt = ciphertext[..SaltSize];
        var nonce = ciphertext.Slice(SaltSize, NonceSize);
        var tag = ciphertext.Slice(SaltSize + NonceSize, TagSize);
        var encryptedData = ciphertext[(SaltSize + NonceSize + TagSize)..];

        // Deriva chave usando o mesmo salt
        using var key = SecureMemory.Allocate<byte>(32);
        DeriveKey(password, salt, options, key.Span);

        // Prepara buffer para dados descriptografados
        var plaintext = new byte[encryptedData.Length];

        // Executa descriptografia AES-GCM
        using var aes = new AesGcm(key.Span, TagSize);
        aes.Decrypt(nonce, encryptedData, tag, plaintext);

        return plaintext;
    }

    /// <summary>
    /// Deriva uma chave criptográfica usando PBKDF2-SHA256 (.NET 9.0 optimized)
    /// </summary>
    private static void DeriveKey(string password, ReadOnlySpan<byte> salt, CryptoOptions options, Span<byte> destination)
    {
        // Converte password para bytes de forma segura
        var maxPasswordBytes = Encoding.UTF8.GetMaxByteCount(password.Length);
        using var passwordBuffer = SecureMemory.Allocate<byte>(maxPasswordBytes);
        var actualPasswordBytes = Encoding.UTF8.GetBytes(password, passwordBuffer.Span);

        try
        {
            // Usa a API otimizada do .NET 9.0 que aceita Span diretamente
            Rfc2898DeriveBytes.Pbkdf2(
                passwordBuffer.Span[..actualPasswordBytes],
                salt,
                destination,
                options.KeyDerivationIterations,
                HashAlgorithmName.SHA256);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Erro na derivação de chave", ex);
        }
    }

    /// <summary>
    /// Valida força da senha baseada nas opções
    /// </summary>
    private static void ValidatePassword(string password, CryptoOptions options)
    {
        if (password.Length < options.MinPasswordLength)
            throw new ArgumentException($"Senha deve ter pelo menos {options.MinPasswordLength} caracteres", nameof(password));

        if (options.RequireStrongPassword && !IsStrongPassword(password))
            throw new ArgumentException("Senha não atende aos critérios de segurança", nameof(password));
    }

    /// <summary>
    /// Verifica se a senha atende critérios de segurança usando .NET 9.0 APIs
    /// </summary>
    private static bool IsStrongPassword(ReadOnlySpan<char> password)
    {
        if (password.Length < 12) return false;

        var hasLower = false;
        var hasUpper = false;
        var hasDigit = false;
        var hasSpecial = false;

        foreach (var c in password)
        {
            if (char.IsAsciiLetterLower(c)) hasLower = true;
            else if (char.IsAsciiLetterUpper(c)) hasUpper = true;
            else if (char.IsAsciiDigit(c)) hasDigit = true;
            else if (!char.IsAsciiLetterOrDigit(c)) hasSpecial = true;

            // Early exit quando todos critérios são atendidos
            if (hasLower && hasUpper && hasDigit && hasSpecial)
                return true;
        }

        return hasLower && hasUpper && hasDigit && hasSpecial;
    }
}

/// <summary>
/// Opções de configuração para criptografia
/// </summary>
public sealed record CryptoOptions
{
    /// <summary>
    /// Número de iterações para derivação de chave PBKDF2
    /// </summary>
    public int KeyDerivationIterations { get; init; } = 120_000;

    /// <summary>
    /// Comprimento mínimo da senha
    /// </summary>
    public int MinPasswordLength { get; init; } = 8;

    /// <summary>
    /// Exigir senha forte (maiúscula, minúscula, número, especial)
    /// </summary>
    public bool RequireStrongPassword { get; init; } = true;

    /// <summary>
    /// Configuração padrão (segura)
    /// </summary>
    public static CryptoOptions Default { get; } = new();

    /// <summary>
    /// Configuração de alta segurança
    /// </summary>
    public static CryptoOptions HighSecurity { get; } = new()
    {
        KeyDerivationIterations = 250_000,
        MinPasswordLength = 16,
        RequireStrongPassword = true
    };

    /// <summary>
    /// Configuração legacy para compatibilidade
    /// </summary>
    public static CryptoOptions Legacy { get; } = new()
    {
        KeyDerivationIterations = 50_000,
        MinPasswordLength = 6,
        RequireStrongPassword = false
    };
}

/// <summary>
/// Gerenciador de memória segura otimizado para .NET 9.0
/// </summary>
public static class SecureMemory
{
    /// <summary>
    /// Aloca memória pinned e zerada automaticamente (.NET 9.0 optimized)
    /// </summary>
    public static SecureBuffer<T> Allocate<T>(int length) where T : unmanaged
        => new(length);

    /// <summary>
    /// Limpa memória de forma segura usando APIs nativas do .NET 9.0
    /// </summary>
    public static void ZeroMemory(Span<byte> span)
    {
        // CryptographicOperations.ZeroMemory aceita apenas Span<byte>
        CryptographicOperations.ZeroMemory(span);
    }

    /// <summary>
    /// Limpa memória genérica convertendo para bytes
    /// </summary>
    public static void ZeroMemory<T>(Span<T> span) where T : unmanaged
    {
        // Converte span genérico para bytes para usar a API nativa
        var byteSpan = System.Runtime.InteropServices.MemoryMarshal.AsBytes(span);
        CryptographicOperations.ZeroMemory(byteSpan);
    }
}

/// <summary>
/// Buffer seguro com limpeza automática (.NET 9.0 optimized)
/// </summary>
public sealed class SecureBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T[] _buffer;
    private bool _disposed;

    internal SecureBuffer(int length)
    {
        // Usa alocação pinned otimizada do .NET 9.0
        _buffer = GC.AllocateUninitializedArray<T>(length, pinned: true);
    }

    /// <summary>
    /// Acesso seguro ao span de memória
    /// </summary>
    public Span<T> Span => _disposed
        ? throw new ObjectDisposedException(nameof(SecureBuffer<T>))
        : _buffer.AsSpan();

    /// <summary>
    /// Limpa e libera a memória de forma segura
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // Usa o método genérico que faz a conversão para bytes
            SecureMemory.ZeroMemory(_buffer.AsSpan());
            _disposed = true;
        }
    }
}

/// <summary>
/// Métodos de extensão para facilitar o uso
/// </summary>
public static class CryptoStringExtensions
{
    /// <summary>
    /// Criptografa string usando configuração padrão
    /// </summary>
    public static string Encrypt(this string plaintext, string password)
        => CryptoString.Encrypt(plaintext, password);

    /// <summary>
    /// Descriptografa string criptografada
    /// </summary>
    public static string Decrypt(this string ciphertext, string password)
        => CryptoString.Decrypt(ciphertext, password);

    /// <summary>
    /// Criptografa string com opções específicas
    /// </summary>
    public static string Encrypt(this string plaintext, string password, CryptoOptions options)
        => CryptoString.Encrypt(plaintext, password, options);

    /// <summary>
    /// Descriptografa string com opções específicas
    /// </summary>
    public static string Decrypt(this string ciphertext, string password, CryptoOptions options)
        => CryptoString.Decrypt(ciphertext, password, options);
}