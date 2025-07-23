using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Codout.Framework.Common.Security;

/// <summary>
/// Algoritmos de hash seguros suportados (.NET 9.0)
/// </summary>
public enum SecureHashAlgorithm
{
    /// <summary>SHA-256 (recomendado para uso geral)</summary>
    Sha256,
    /// <summary>SHA-384 (segurança extra)</summary>
    Sha384,
    /// <summary>SHA-512 (máxima segurança)</summary>
    Sha512,
    /// <summary>SHA3-256 (algoritmo mais moderno)</summary>
    Sha3_256,
    /// <summary>SHA3-384</summary>
    Sha3_384,
    /// <summary>SHA3-512</summary>
    Sha3_512
}

/// <summary>
/// Implementação moderna e segura para hash com salt (.NET 9.0)
/// Remove algoritmos inseguros (MD5, SHA1) e usa apenas algoritmos seguros
/// </summary>
public static class SecureHash
{
    private const int DefaultSaltSize = 32; // 256 bits
    private const int MinSaltSize = 16;     // 128 bits mínimo
    private const int MaxSaltSize = 64;     // 512 bits máximo

    /// <summary>
    /// Gera hash seguro para texto com salt aleatório
    /// </summary>
    /// <param name="plainText">Texto a ser hasheado</param>
    /// <param name="algorithm">Algoritmo de hash (padrão: SHA-256)</param>
    /// <param name="options">Opções de configuração</param>
    /// <returns>Hash em Base64 com salt anexado</returns>
    /// <exception cref="ArgumentException">Quando parâmetros são inválidos</exception>
    public static string ComputeHash(string plainText, 
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256, 
        HashOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText, nameof(plainText));
        
        options ??= HashOptions.Default;
        ValidateOptions(options);

        // Gera salt criptograficamente seguro
        var saltSize = options.SaltSize ?? DefaultSaltSize;
        Span<byte> salt = stackalloc byte[saltSize];
        RandomNumberGenerator.Fill(salt);

        return ComputeHashInternal(plainText, algorithm, salt, options);
    }

    /// <summary>
    /// Gera hash com salt específico (para testes ou casos especiais)
    /// </summary>
    /// <param name="plainText">Texto a ser hasheado</param>
    /// <param name="algorithm">Algoritmo de hash</param>
    /// <param name="salt">Salt específico</param>
    /// <param name="options">Opções de configuração</param>
    /// <returns>Hash em Base64 com salt anexado</returns>
    public static string ComputeHash(string plainText, 
        SecureHashAlgorithm algorithm, 
        ReadOnlySpan<byte> salt, 
        HashOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText, nameof(plainText));
        
        if (salt.Length < MinSaltSize)
            throw new ArgumentException($"Salt deve ter pelo menos {MinSaltSize} bytes", nameof(salt));

        options ??= HashOptions.Default;
        ValidateOptions(options);

        return ComputeHashInternal(plainText, algorithm, salt, options);
    }

    /// <summary>
    /// Verifica se o texto corresponde ao hash fornecido
    /// </summary>
    /// <param name="plainText">Texto a ser verificado</param>
    /// <param name="hashValue">Hash em Base64 para comparação</param>
    /// <param name="algorithm">Algoritmo usado no hash original</param>
    /// <param name="options">Opções de configuração</param>
    /// <returns>True se o texto corresponde ao hash</returns>
    public static bool VerifyHash(string plainText, 
        string hashValue, 
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256,
        HashOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText, nameof(plainText));
        ArgumentException.ThrowIfNullOrWhiteSpace(hashValue, nameof(hashValue));
        
        options ??= HashOptions.Default;

        try
        {
            // Decodifica hash + salt
            var hashWithSalt = Convert.FromBase64String(hashValue);
            var hashSize = GetHashSize(algorithm);
            
            if (hashWithSalt.Length < hashSize + MinSaltSize)
                return false;

            // Extrai salt do final
            var salt = hashWithSalt.AsSpan()[hashSize..];
            
            // Recalcula hash com o mesmo salt
            var expectedHash = ComputeHashInternal(plainText, algorithm, salt, options);
            
            // Comparação resistente a timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(hashValue),
                Convert.FromBase64String(expectedHash));
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Computa hash de arquivo de forma assíncrona
    /// </summary>
    /// <param name="filePath">Caminho do arquivo</param>
    /// <param name="algorithm">Algoritmo de hash</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Hash do arquivo em hexadecimal</returns>
    public static async Task<string> ComputeFileHashAsync(string filePath, 
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath, nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 
            bufferSize: 4096, useAsync: true);
        
        return await ComputeStreamHashAsync(fileStream, algorithm, cancellationToken);
    }

    /// <summary>
    /// Computa hash de stream de forma assíncrona
    /// </summary>
    /// <param name="stream">Stream a ser hasheada</param>
    /// <param name="algorithm">Algoritmo de hash</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Hash em hexadecimal</returns>
    public static async Task<string> ComputeStreamHashAsync(Stream stream, 
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        if (!stream.CanRead)
            throw new ArgumentException("Stream deve ser legível", nameof(stream));

        using var hashAlg = CreateHashAlgorithm(algorithm);
        var hashBytes = await hashAlg.ComputeHashAsync(stream, cancellationToken);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verifica integridade de arquivo comparando com hash esperado
    /// </summary>
    /// <param name="filePath">Caminho do arquivo</param>
    /// <param name="expectedHash">Hash esperado em hexadecimal</param>
    /// <param name="algorithm">Algoritmo de hash</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o arquivo está íntegro</returns>
    public static async Task<bool> VerifyFileIntegrityAsync(string filePath, 
        string expectedHash,
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var actualHash = await ComputeFileHashAsync(filePath, algorithm, cancellationToken);
            
            // Normaliza hashes para comparação
            var expected = expectedHash.Replace("-", "").ToLowerInvariant();
            var actual = actualHash.Replace("-", "").ToLowerInvariant();
            
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(actual));
        }
        catch
        {
            return false;
        }
    }
    /// <param name="algorithm">Algoritmo a ser verificado</param>
    /// <returns>True se o algoritmo está disponível</returns>
    public static bool IsAlgorithmSupported(SecureHashAlgorithm algorithm)
    {
        try
        {
            using var hashAlg = CreateHashAlgorithm(algorithm);
            return true;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    /// <summary>
    /// Retorna lista de algoritmos suportados na plataforma atual
    /// </summary>
    /// <returns>Array com algoritmos disponíveis</returns>
    public static SecureHashAlgorithm[] GetSupportedAlgorithms()
    {
        var allAlgorithms = Enum.GetValues<SecureHashAlgorithm>();
        return allAlgorithms.Where(IsAlgorithmSupported).ToArray();
    }
    /// <param name="filePath">Caminho do arquivo</param>
    /// <param name="expectedHash">Hash esperado em hexadecimal</param>
    /// <param name="algorithm">Algoritmo de hash</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>True se o arquivo está íntegro</returns>

    /// <summary>
    /// Implementação interna para computar hash
    /// </summary>
    private static string ComputeHashInternal(string plainText, 
        SecureHashAlgorithm algorithm, 
        ReadOnlySpan<byte> salt, 
        HashOptions options)
    {
        // Converte texto para bytes
        var maxTextBytes = Encoding.UTF8.GetMaxByteCount(plainText.Length);
        using var textBuffer = SecureMemory.Allocate<byte>(maxTextBytes);
        var actualTextBytes = Encoding.UTF8.GetBytes(plainText, textBuffer.Span);
        var textSpan = textBuffer.Span[..actualTextBytes];

        // Combina texto + salt
        var combinedSize = textSpan.Length + salt.Length;
        using var combinedBuffer = SecureMemory.Allocate<byte>(combinedSize);
        var combinedSpan = combinedBuffer.Span;
        
        textSpan.CopyTo(combinedSpan);
        salt.CopyTo(combinedSpan[textSpan.Length..]);

        // Computa hash
        using var hashAlg = CreateHashAlgorithm(algorithm);
        Span<byte> hashBytes = stackalloc byte[GetHashSize(algorithm)];
        
        if (!hashAlg.TryComputeHash(combinedSpan, hashBytes, out var bytesWritten))
            throw new CryptographicException("Erro ao computar hash");

        // Combina hash + salt para resultado final
        var resultSize = bytesWritten + salt.Length;
        var result = new byte[resultSize];
        var resultSpan = result.AsSpan();
        
        hashBytes[..bytesWritten].CopyTo(resultSpan);
        salt.CopyTo(resultSpan[bytesWritten..]);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Cria instância do algoritmo de hash
    /// </summary>
    private static HashAlgorithm CreateHashAlgorithm(SecureHashAlgorithm algorithm) => algorithm switch
    {
        SecureHashAlgorithm.Sha256 => SHA256.Create(),
        SecureHashAlgorithm.Sha384 => SHA384.Create(),
        SecureHashAlgorithm.Sha512 => SHA512.Create(),
        SecureHashAlgorithm.Sha3_256 => SHA3_256.Create(),
        SecureHashAlgorithm.Sha3_384 => SHA3_384.Create(),
        SecureHashAlgorithm.Sha3_512 => SHA3_512.Create(),
        _ => throw new ArgumentException($"Algoritmo não suportado: {algorithm}")
    };

    /// <summary>
    /// Retorna tamanho do hash em bytes
    /// </summary>
    private static int GetHashSize(SecureHashAlgorithm algorithm) => algorithm switch
    {
        SecureHashAlgorithm.Sha256 or SecureHashAlgorithm.Sha3_256 => 32,
        SecureHashAlgorithm.Sha384 or SecureHashAlgorithm.Sha3_384 => 48,
        SecureHashAlgorithm.Sha512 or SecureHashAlgorithm.Sha3_512 => 64,
        _ => throw new ArgumentException($"Algoritmo não suportado: {algorithm}")
    };

    /// <summary>
    /// Valida opções de configuração
    /// </summary>
    private static void ValidateOptions(HashOptions options)
    {
        if (options.SaltSize.HasValue)
        {
            var saltSize = options.SaltSize.Value;
            if (saltSize < MinSaltSize || saltSize > MaxSaltSize)
                throw new ArgumentException($"Tamanho do salt deve estar entre {MinSaltSize} e {MaxSaltSize} bytes");
        }
    }
}

/// <summary>
/// Opções de configuração para operações de hash
/// </summary>
public sealed record HashOptions
{
    /// <summary>
    /// Tamanho do salt em bytes (padrão: 32 bytes)
    /// </summary>
    public int? SaltSize { get; init; }

    /// <summary>
    /// Configuração padrão
    /// </summary>
    public static HashOptions Default { get; } = new();

    /// <summary>
    /// Configuração de alta segurança
    /// </summary>
    public static HashOptions HighSecurity { get; } = new()
    {
        SaltSize = 64 // 512 bits
    };

    /// <summary>
    /// Configuração compacta (menor overhead)
    /// </summary>
    public static HashOptions Compact { get; } = new()
    {
        SaltSize = 16 // 128 bits
    };
}

/// <summary>
/// Gerenciador de memória segura para operações de hash
/// </summary>
file static class SecureMemory
{
    public static SecureBuffer<T> Allocate<T>(int length) where T : unmanaged
        => new(length);
}

/// <summary>
/// Buffer seguro com limpeza automática
/// </summary>
file sealed class SecureBuffer<T> : IDisposable where T : unmanaged
{
    private readonly T[] _buffer;
    private bool _disposed;

    public SecureBuffer(int length)
    {
        _buffer = GC.AllocateUninitializedArray<T>(length, pinned: true);
    }

    public Span<T> Span => _disposed 
        ? throw new ObjectDisposedException(nameof(SecureBuffer<T>)) 
        : _buffer.AsSpan();

    public void Dispose()
    {
        if (!_disposed)
        {
            // Limpa memória convertendo para bytes
            var byteSpan = MemoryMarshal.AsBytes(_buffer.AsSpan());
            CryptographicOperations.ZeroMemory(byteSpan);
            _disposed = true;
        }
    }
}

/// <summary>
/// Métodos de extensão para facilitar o uso
/// </summary>
public static class SecureHashExtensions
{
    /// <summary>
    /// Gera hash seguro da string
    /// </summary>
    public static string ToSecureHash(this string text, SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256)
        => SecureHash.ComputeHash(text, algorithm);

    /// <summary>
    /// Verifica se a string corresponde ao hash
    /// </summary>
    public static bool VerifyAgainstHash(this string text, string hash, SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256)
        => SecureHash.VerifyHash(text, hash, algorithm);

    /// <summary>
    /// Computa hash de arquivo de forma assíncrona
    /// </summary>
    public static Task<string> ComputeFileHashAsync(this FileInfo fileInfo, 
        SecureHashAlgorithm algorithm = SecureHashAlgorithm.Sha256,
        CancellationToken cancellationToken = default)
        => SecureHash.ComputeFileHashAsync(fileInfo.FullName, algorithm, cancellationToken);
}

// Exemplos de uso para .NET 9.0:
public class SecureHashUsageExamples
{
    public void BasicUsage()
    {
        const string password = "MinhaS3nh@Forte2024!";
        
        // Hash com salt automático
        var hash = password.ToSecureHash();
        Console.WriteLine($"Hash gerado: {hash}");
        
        // Verificação
        var isValid = password.VerifyAgainstHash(hash);
        Console.WriteLine($"Senha válida: {isValid}");
        
        // Hash com algoritmo específico
        var sha512Hash = SecureHash.ComputeHash(password, SecureHashAlgorithm.Sha512);
        var sha3Hash = SecureHash.ComputeHash(password, SecureHashAlgorithm.Sha3_256);
    }

    public void AdvancedUsage()
    {
        const string sensitiveData = "Dados ultra-secretos";
        
        // Hash com configuração de alta segurança
        var secureHash = SecureHash.ComputeHash(sensitiveData, 
            SecureHashAlgorithm.Sha3_512, 
            HashOptions.HighSecurity);
        
        // Verificação com mesmas configurações
        var isValid = SecureHash.VerifyHash(sensitiveData, secureHash, 
            SecureHashAlgorithm.Sha3_512, 
            HashOptions.HighSecurity);
            
        Console.WriteLine($"Verificação de alta segurança: {isValid}");
    }

    public async Task FileIntegrityUsage()
    {
        const string filePath = @"C:\temp\document.pdf";
        
        // Computa hash do arquivo
        var fileHash = await SecureHash.ComputeFileHashAsync(filePath, SecureHashAlgorithm.Sha256);
        Console.WriteLine($"Hash do arquivo: {fileHash}");
        
        // Verifica integridade
        var isIntegral = await SecureHash.VerifyFileIntegrityAsync(filePath, fileHash);
        Console.WriteLine($"Arquivo íntegro: {isIntegral}");
        
        // Usando extension method
        var fileInfo = new FileInfo(filePath);
        var hash2 = await fileInfo.ComputeFileHashAsync(SecureHashAlgorithm.Sha3_256);
    }

    public void CustomSaltUsage()
    {
        const string text = "Dados com salt customizado";
        
        // Salt customizado
        var customSalt = new byte[32];
        RandomNumberGenerator.Fill(customSalt);
        
        var hash = SecureHash.ComputeHash(text, SecureHashAlgorithm.Sha256, customSalt);
        var isValid = SecureHash.VerifyHash(text, hash, SecureHashAlgorithm.Sha256);
        
        Console.WriteLine($"Hash com salt customizado válido: {isValid}");
    }

    public void PlatformCompatibilityUsage()
    {
        const string data = "Teste de compatibilidade";
        
        // Verifica algoritmos disponíveis
        var supportedAlgorithms = SecureHash.GetSupportedAlgorithms();
        Console.WriteLine($"Algoritmos suportados: {string.Join(", ", supportedAlgorithms)}");
        
        // Tenta usar SHA3 com fallback
        SecureHashAlgorithm algorithm;
        if (SecureHash.IsAlgorithmSupported(SecureHashAlgorithm.Sha3_256))
        {
            algorithm = SecureHashAlgorithm.Sha3_256;
            Console.WriteLine("Usando SHA3-256 (mais moderno)");
        }
        else
        {
            algorithm = SecureHashAlgorithm.Sha256;
            Console.WriteLine("Usando SHA-256 (fallback compatível)");
        }
        
        var hash = SecureHash.ComputeHash(data, algorithm);
        Console.WriteLine($"Hash gerado: {hash}");
    }

    public void ErrorHandlingUsage()
    {
        const string data = "Teste com tratamento de erro";
        
        try
        {
            // Tenta usar algoritmo que pode não estar disponível
            var hash = SecureHash.ComputeHash(data, SecureHashAlgorithm.Sha3_512);
            Console.WriteLine($"SHA3-512 hash: {hash}");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"SHA3 não suportado: {ex.Message}");
            
            // Fallback para algoritmo garantidamente disponível
            var fallbackHash = SecureHash.ComputeHash(data, SecureHashAlgorithm.Sha256);
            Console.WriteLine($"Fallback SHA-256 hash: {fallbackHash}");
        }
    }

    public void PerformanceOptimizedUsage()
    {
        // Para cenários que requerem performance máxima
        const string data = "Performance crítica";
        
        // Usa configuração compacta (menor overhead)
        var compactHash = SecureHash.ComputeHash(data, 
            SecureHashAlgorithm.Sha256, 
            HashOptions.Compact);
            
        Console.WriteLine($"Hash compacto: {compactHash}");
    }
}