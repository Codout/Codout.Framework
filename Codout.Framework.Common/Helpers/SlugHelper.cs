using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Helpers;

/// <summary>
/// Helper para geração de slugs limpos e amigáveis para URLs
/// </summary>
public class SlugHelper
{
    private readonly SlugConfig _config;

    // Regex compilados para melhor performance (cached)
    private static readonly Regex CollapseWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex SingleWhitespaceRegex = new(@"\s", RegexOptions.Compiled);
    private static readonly Regex CollapseHyphensRegex = new(@"-+", RegexOptions.Compiled);

    public SlugHelper() : this(SlugConfig.Default)
    {
    }

    public SlugHelper(SlugConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config), "Config não pode ser null. Use SlugConfig.Default ou construtor vazio.");
    }

    /// <summary>
    /// Gera um slug limpo a partir de uma string
    /// </summary>
    /// <param name="input">String de entrada</param>
    /// <returns>Slug limpo e amigável para URL</returns>
    /// <exception cref="ArgumentException">Quando a entrada é nula ou vazia</exception>
    public string GenerateSlug(string input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input, nameof(input));

        var processed = input;

        // 1. Aplicar case transformation primeiro
        if (_config.ForceLowerCase)
            processed = processed.ToLowerInvariant();

        // 2. Limpar espaços em branco
        processed = CleanWhiteSpace(processed, _config.CollapseWhiteSpace);

        // 3. Aplicar substituições personalizadas
        processed = ApplyReplacements(processed, _config.CharacterReplacements);

        // 4. Remover diacríticos (acentos)
        processed = RemoveDiacritics(processed);

        // 5. Remover caracteres não permitidos
        processed = DeleteCharacters(processed, _config.DeniedCharactersRegex);

        // 6. NOVO: Colapsar hífens múltiplos em um único hífen
        processed = CollapseConsecutiveHyphens(processed);

        // 7. NOVO: Remover hífens do início e fim
        processed = TrimHyphens(processed);

        // 8. Validar resultado final
        return ValidateAndReturnSlug(processed, input);
    }

    /// <summary>
    /// Limpa espaços em branco excessivos
    /// </summary>
    protected string CleanWhiteSpace(string str, bool collapse)
    {
        return collapse 
            ? CollapseWhitespaceRegex.Replace(str, " ")
            : SingleWhitespaceRegex.Replace(str, " ");
    }

    /// <summary>
    /// Remove acentos e diacríticos mantendo caracteres base
    /// </summary>
    protected string RemoveDiacritics(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var normalizedString = str.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Aplica substituições personalizadas de caracteres
    /// </summary>
    protected string ApplyReplacements(string str, IReadOnlyDictionary<string, string> replacements)
    {
        if (replacements.Count == 0) return str;

        var result = new StringBuilder(str);
        foreach (var (key, value) in replacements)
        {
            result.Replace(key, value);
        }

        return result.ToString();
    }

    /// <summary>
    /// Remove caracteres usando regex
    /// </summary>
    protected string DeleteCharacters(string str, string regexPattern)
    {
        if (string.IsNullOrEmpty(regexPattern)) return str;
        
        return Regex.Replace(str, regexPattern, string.Empty, RegexOptions.Compiled);
    }

    /// <summary>
    /// NOVO: Colapsa hífens consecutivos em um único hífen
    /// </summary>
    protected string CollapseConsecutiveHyphens(string str)
    {
        return CollapseHyphensRegex.Replace(str, "-");
    }

    /// <summary>
    /// NOVO: Remove hífens do início e fim da string
    /// </summary>
    protected string TrimHyphens(string str)
    {
        return str.Trim('-');
    }

    /// <summary>
    /// NOVO: Valida o resultado final e fornece fallback se necessário
    /// </summary>
    protected string ValidateAndReturnSlug(string processed, string original)
    {
        // Se o resultado ficou vazio, tentar gerar um slug baseado no hash
        if (string.IsNullOrWhiteSpace(processed))
        {
            if (_config.AllowEmptySlug)
                return string.Empty;

            // Fallback: usar hash do conteúdo original
            var hash = Math.Abs(original.GetHashCode()).ToString();
            return _config.EmptySlugFallback.Replace("{hash}", hash);
        }

        // Limitar tamanho se configurado
        if (_config.MaxLength.HasValue && processed.Length > _config.MaxLength.Value)
        {
            processed = processed[.._config.MaxLength.Value].TrimEnd('-');
        }

        return processed;
    }
}

/// <summary>
/// Configurações para geração de slugs
/// </summary>
public class SlugConfig
{
    /// <summary>
    /// Substituições de caracteres personalizadas
    /// </summary>
    public Dictionary<string, string> CharacterReplacements { get; set; } = new() { { " ", "-" } };

    /// <summary>
    /// Forçar minúsculas
    /// </summary>
    public bool ForceLowerCase { get; set; } = true;

    /// <summary>
    /// Colapsar espaços múltiplos em um único espaço
    /// </summary>
    public bool CollapseWhiteSpace { get; set; } = true;

    /// <summary>
    /// Regex para caracteres não permitidos
    /// </summary>
    public string DeniedCharactersRegex { get; set; } = @"[^a-zA-Z0-9\-\._]";

    /// <summary>
    /// NOVO: Permitir slug vazio
    /// </summary>
    public bool AllowEmptySlug { get; set; } = false;

    /// <summary>
    /// NOVO: Fallback quando slug fica vazio (use {hash} para incluir hash do original)
    /// </summary>
    public string EmptySlugFallback { get; set; } = "item-{hash}";

    /// <summary>
    /// NOVO: Tamanho máximo do slug
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Configuração padrão otimizada
    /// </summary>
    public static SlugConfig Default => new();

    /// <summary>
    /// Configuração para URLs mais permissivas
    /// </summary>
    public static SlugConfig Permissive => new()
    {
        DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._~]", // Permite mais caracteres válidos em URLs
        MaxLength = 100
    };

    /// <summary>
    /// Configuração para slugs mais restritivos (apenas letras e números)
    /// </summary>
    public static SlugConfig Strict => new()
    {
        DeniedCharactersRegex = @"[^a-zA-Z0-9\-]",
        CharacterReplacements = new()
        {
            { " ", "-" },
            { ".", "-" },
            { "_", "-" }
        },
        MaxLength = 50
    };

    /// <summary>
    /// Configuração para preservar mais caracteres especiais
    /// </summary>
    public static SlugConfig Extended => new()
    {
        DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._~:@!$&'()*+,;=]",
        ForceLowerCase = false,
        MaxLength = 150
    };
}

/// <summary>
/// Métodos de extensão para facilitar o uso
/// </summary>
public static class SlugExtensions
{
    private static readonly SlugHelper DefaultSlugHelper = new();

    /// <summary>
    /// Converte string em slug usando configuração padrão
    /// </summary>
    public static string ToSlug(this string input)
    {
        return DefaultSlugHelper.GenerateSlug(input);
    }

    /// <summary>
    /// Converte string em slug usando configuração específica
    /// </summary>
    public static string ToSlug(this string input, SlugConfig config)
    {
        var helper = new SlugHelper(config);
        return helper.GenerateSlug(input);
    }
}