using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Extensions;

public class SlugHelper
{
    public SlugHelper() :
        this(new Config())
    {
    }

    public SlugHelper(Config config)
    {
        if (config != null)
            _config = config;
        else
            throw new ArgumentNullException(nameof(config), "can't be null use default config or empty construct.");
    }

    protected Config _config { get; set; }

    public string GenerateSlug(string str)
    {
        if (_config.ForceLowerCase)
            str = str.ToLower();

        str = CleanWhiteSpace(str, _config.CollapseWhiteSpace);
        str = ApplyReplacements(str, _config.CharacterReplacements);
        str = RemoveDiacritics(str);
        str = DeleteCharacters(str, _config.DeniedCharactersRegex);

        return str;
    }

    protected string CleanWhiteSpace(string str, bool collapse)
    {
        return Regex.Replace(str, collapse ? @"\s+" : @"\s", " ");
    }

    protected string RemoveDiacritics(string str)
    {
        var stFormD = str.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var t in stFormD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(t);

            if (uc != UnicodeCategory.NonSpacingMark)
                sb.Append(t);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    protected string ApplyReplacements(string str, Dictionary<string, string> replacements)
    {
        var sb = new StringBuilder(str);

        foreach (var replacement in replacements)
            sb.Replace(replacement.Key, replacement.Value);

        return sb.ToString();
    }

    protected string DeleteCharacters(string str, string regex)
    {
        return Regex.Replace(str, regex, "");
    }

    public class Config
    {
        public Config()
        {
            CharacterReplacements = new Dictionary<string, string> { { " ", "-" } };
            ForceLowerCase = true;
            CollapseWhiteSpace = true;
            DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]";
        }

        public Dictionary<string, string> CharacterReplacements { get; set; }
        public bool ForceLowerCase { get; set; }
        public bool CollapseWhiteSpace { get; set; }
        public string DeniedCharactersRegex { get; set; }
    }
}