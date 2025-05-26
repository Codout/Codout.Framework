using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Extensions;

/// <summary>
///     Extensões comuns para tipos relacionadas ao Inflector.
/// </summary>
public static class Inflector
{
    #region Construtores

    /// <summary>
    ///     Initializes the <see cref="Inflector" /> class.
    /// </summary>
    static Inflector()
    {
        AddPluralRule("$", "s");
        AddPluralRule("()r$", "$1res");
        AddPluralRule("()ão$", "$1ões");
        AddPluralRule("()um$", "$1uns");
        AddPluralRule("()s$", "$1ses");
        AddPluralRule("()il$", "$1is");
        AddPluralRule("()m$", "$1ns");
        AddPluralRule("()ol$", "$1óis");
        AddPluralRule("()x$", "$1xes");
        AddPluralRule("()al$", "$1ais");
        AddPluralRule("()el$", "$1éis");
        AddPluralRule("(ul)$", "$1es");
        AddPluralRule("()zul$", "$1zuis");
        AddPluralRule("()ês$", "$1eses");
        AddPluralRule("()z$", "$1zes");

        AddSingularRule("s$", "");
        AddSingularRule("(xul)es$", "$1");
        AddSingularRule("()res$", "$1r");
        AddSingularRule("()ões", "$1ão");
        AddSingularRule("()uns", "$1um");
        AddSingularRule("()ses$", "$1s");
        AddSingularRule("()is$", "$1il");
        AddSingularRule("()ns$", "$1m");
        AddSingularRule("(s|ç)óis$", "$1ol");
        AddSingularRule("()xes$", "$1x");
        AddSingularRule("(ei)xes$", "$1xe");
        AddSingularRule("()ais$", "$1al");
        AddSingularRule("()éis$", "$1el");
        AddSingularRule("()zuis$", "$1zul");
        AddSingularRule("()eses$", "$1ês");
        AddSingularRule("()zes$", "$1z");


        AddIrregularRule("catalão", "catalães");
        AddIrregularRule("alemão", "alemães");
        AddIrregularRule("cão", "cães");
        AddIrregularRule("capitão", "capitães");
        AddIrregularRule("escrivão", "escrivães");
        AddIrregularRule("pão", "pães");
        AddIrregularRule("cidadão", "cidadãos");
        AddIrregularRule("cortesão", "cortesãos");
        AddIrregularRule("cristão", "cristãos");
        AddIrregularRule("irmão", "irmãos");
        AddIrregularRule("pagão", "pagãos");
        AddIrregularRule("acórdão", "acórdãos");
        AddIrregularRule("bênção", "bênçãos");
        AddIrregularRule("órfão", "órfãos");
        AddIrregularRule("órgão", "órgãos");
        AddIrregularRule("sótão", "sótãos");
        AddIrregularRule("qualquer", "quaisquer");
        AddIrregularRule("palavra-chave", "palavras-chave");
        AddIrregularRule("segunda-feira", "segundas-feiras");
        AddIrregularRule("terça-feira", "terças-feiras");
        AddIrregularRule("quarta-feira", "quartas-feiras");
        AddIrregularRule("quinta-feira", "quintas-feiras");
        AddIrregularRule("sexta-feira", "sextas-feiras");

        AddUnknownCountRule("toráx");
        AddUnknownCountRule("fénix");
        AddUnknownCountRule("louva-a-deus");
    }

    #endregion

    #region AddIrregularRule

    /// <summary>
    ///     Adds the irregular rule.
    /// </summary>
    /// <param name="singular">The singular.</param>
    /// <param name="plural">The plural.</param>
    private static void AddIrregularRule(string singular, string plural)
    {
        AddPluralRule(string.Concat("(", singular[0], ")", singular.Substring(1), "$"),
            string.Concat("$1", plural.Substring(1)));
        AddSingularRule(string.Concat("(", plural[0], ")", plural.Substring(1), "$"),
            string.Concat("$1", singular.Substring(1)));
    }

    #endregion

    #region AddUnknownCountRule

    /// <summary>
    ///     Adds the unknown count rule.
    /// </summary>
    /// <param name="word">The word.</param>
    private static void AddUnknownCountRule(string word)
    {
        Uncountables.Add(word.ToLower());
    }

    #endregion

    #region AddPluralRule

    /// <summary>
    ///     Adds the plural rule.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <param name="replacement">The replacement.</param>
    private static void AddPluralRule(string rule, string replacement)
    {
        Plurals.Add(new InflectorRule(rule, replacement));
    }

    #endregion

    #region AddSingularRule

    /// <summary>
    ///     Adds the singular rule.
    /// </summary>
    /// <param name="rule">The rule.</param>
    /// <param name="replacement">The replacement.</param>
    private static void AddSingularRule(string rule, string replacement)
    {
        Singulars.Add(new InflectorRule(rule, replacement));
    }

    #endregion

    #region MakePlural

    /// <summary>
    ///     Makes the plural.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    public static string MakePlural(this string word)
    {
        return ApplyRules(Plurals, word);
    }

    #endregion

    #region MakeSingular

    /// <summary>
    ///     Makes the singular.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    public static string MakeSingular(this string word)
    {
        return ApplyRules(Singulars, word);
    }

    #endregion

    #region ApplyRules

    /// <summary>
    ///     Applies the rules.
    /// </summary>
    /// <param name="rules">The rules.</param>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    private static string ApplyRules(IList<InflectorRule> rules, string word)
    {
        var result = word;

        if (!Uncountables.Contains(word.ToLower()))
            for (var i = rules.Count - 1; i >= 0; i--)
            {
                var currentPass = rules[i].Apply(word);

                if (currentPass != null)
                {
                    result = currentPass;
                    break;
                }
            }

        return result;
    }

    #endregion

    #region ToTitleCase

    /// <summary>
    ///     Converts the string to title case.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    public static string ToTitleCase(this string word)
    {
        return Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])",
            match => match.Captures[0].Value.ToUpper());
    }

    #endregion

    #region ToHumanCase

    /// <summary>
    ///     Converts the string to human case.
    /// </summary>
    /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
    /// <returns></returns>
    public static string ToHumanCase(this string lowercaseAndUnderscoredWord)
    {
        return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
    }

    #endregion

    #region ToProper

    /// <summary>
    ///     Convert string to proper case
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string ToProper(this string sourceString)
    {
        var propertyName = sourceString.ToPascalCase();
        return propertyName;
    }

    #endregion

    #region ToPascalCase

    /// <summary>
    ///     Converts the string to pascal case.
    /// </summary>
    /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
    /// <returns></returns>
    public static string ToPascalCase(this string lowercaseAndUnderscoredWord)
    {
        return ToPascalCase(lowercaseAndUnderscoredWord, true);
    }

    #endregion

    #region ToPascalCase

    /// <summary>
    ///     Converts text to pascal case...
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="removeUnderscores">if set to <c>true</c> [remove underscores].</param>
    /// <returns></returns>
    public static string ToPascalCase(this string text, bool removeUnderscores)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace("_", " ");
        var joinString = removeUnderscores ? string.Empty : "_";
        var words = text.Split(' ');
        if (words.Length > 1 || words[0].IsUpperCase())
        {
            for (var i = 0; i < words.Length; i++)
                if (words[i].Length > 0)
                {
                    var word = words[i];
                    var restOfWord = word.Substring(1);

                    if (restOfWord.IsUpperCase())
                        restOfWord = restOfWord.ToLower(CultureInfo.CurrentUICulture);

                    var firstChar = char.ToUpper(word[0], CultureInfo.CurrentUICulture);
                    words[i] = string.Concat(firstChar, restOfWord);
                }

            return string.Join(joinString, words);
        }

        return string.Concat(words[0].Substring(0, 1).ToUpper(CultureInfo.CurrentUICulture), words[0].Substring(1));
    }

    #endregion

    #region ToCamelCase

    /// <summary>
    ///     Converts the string to camel case.
    /// </summary>
    /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
    /// <returns></returns>
    public static string ToCamelCase(this string lowercaseAndUnderscoredWord)
    {
        return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
    }

    #endregion

    #region AddUnderscores

    /// <summary>
    ///     Adds the underscores.
    /// </summary>
    /// <param name="pascalCasedWord">The pascal cased word.</param>
    /// <returns></returns>
    public static string AddUnderscores(this string pascalCasedWord)
    {
        return
            Regex.Replace(
                Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
                    "$1_$2"), @"[-\s]", "_").ToLower();
    }

    #endregion

    #region MakeInitialCaps

    /// <summary>
    ///     Makes the initial caps.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    public static string MakeInitialCaps(this string word)
    {
        return string.Concat(word.Substring(0, 1).ToUpper(), word.Substring(1).ToLower());
    }

    #endregion

    #region MakeInitialLowerCase

    /// <summary>
    ///     Makes the initial lower case.
    /// </summary>
    /// <param name="word">The word.</param>
    /// <returns></returns>
    public static string MakeInitialLowerCase(this string word)
    {
        return string.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
    }

    #endregion

    #region AddOrdinalSuffix

    /// <summary>
    ///     Adds the ordinal suffix.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <returns></returns>
    public static string AddOrdinalSuffix(this string number)
    {
        if (number.IsStringNumeric())
        {
            var n = int.Parse(number);
            var nMod100 = n % 100;

            if (nMod100 >= 11 && nMod100 <= 13)
                return string.Concat(number, "th");

            switch (n % 10)
            {
                case 1:
                    return string.Concat(number, "st");
                case 2:
                    return string.Concat(number, "nd");
                case 3:
                    return string.Concat(number, "rd");
                default:
                    return string.Concat(number, "th");
            }
        }

        return number;
    }

    #endregion

    #region ConvertUnderscoresToDashes

    /// <summary>
    ///     Converts the underscores to dashes.
    /// </summary>
    /// <param name="underscoredWord">The underscored word.</param>
    /// <returns></returns>
    public static string ConvertUnderscoresToDashes(this string underscoredWord)
    {
        return underscoredWord.Replace('_', '-');
    }

    #endregion

    #region Nested type: InflectorRule

    /// <summary>
    ///     Summary for the InflectorRule class
    /// </summary>
    private class InflectorRule
    {
        #region Construtores

        /// <summary>
        ///     Initializes a new instance of the <see cref="InflectorRule" /> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="replacementText">The replacement text.</param>
        public InflectorRule(string regexPattern, string replacementText)
        {
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            _replacement = replacementText;
        }

        #endregion

        #region Apply

        /// <summary>
        ///     Applies the specified word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string Apply(string word)
        {
            if (!_regex.IsMatch(word))
                return null;

            var replace = _regex.Replace(word, _replacement);
            if (word == word.ToUpper())
                replace = replace.ToUpper();

            return replace;
        }

        #endregion

        #region Variáveis

        /// <summary>
        /// </summary>
        private readonly Regex _regex;

        /// <summary>
        /// </summary>
        private readonly string _replacement;

        #endregion
    }

    #endregion

    #region Variáveis

    private static readonly List<InflectorRule> Plurals = [];
    private static readonly List<InflectorRule> Singulars = [];
    private static readonly List<string> Uncountables = [];

    #endregion
}