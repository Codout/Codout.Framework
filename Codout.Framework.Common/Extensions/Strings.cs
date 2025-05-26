using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Codout.Framework.Common.Extensions;

/// <summary>
///     Extensões comuns para tipos relacionadas a strings.
/// </summary>
public static class Strings
{
    #region Variáveis

    private static readonly Dictionary<int, string> EntityTable = new();

    #endregion

    #region Construtores

    /// <summary>
    ///     Initializes the <see cref="Strings" /> class.
    /// </summary>
    static Strings()
    {
        FillEntities();
    }

    #endregion

    #region RemoveAcentos

    /// <summary>
    ///     Remove os acentos do texto.
    /// </summary>
    /// <param name="texto"></param>
    /// <returns></returns>
    public static string RemoveAccents(this string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        var bytes = Encoding.GetEncoding("iso-8859-8").GetBytes(texto);
        return Encoding.UTF8.GetString(bytes);
    }

    #endregion RemoveAcentos

    #region Matches

    /// <summary>
    ///     Compara duas strings.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="compare"></param>
    /// <returns></returns>
    public static bool Matches(this string source, string compare)
    {
        return string.Equals(source, compare, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region MatchesTrimmed

    /// <summary>
    ///     Compara duas strings ignorando espaços no início e no fim da string.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="compare"></param>
    /// <returns></returns>
    public static bool MatchesTrimmed(this string source, string compare)
    {
        return string.Equals(source.Trim(), compare.Trim(), StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region MatchesRegex

    /// <summary>
    ///     Verifica se a string está no formato especificado.
    /// </summary>
    /// <param name="inputString"></param>
    /// <param name="matchPattern"></param>
    /// <returns></returns>
    public static bool MatchesRegex(this string inputString, string matchPattern)
    {
        return Regex.IsMatch(inputString, matchPattern,
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
    }

    #endregion

    #region Chop

    /// <summary>
    ///     Strips the last specified chars from a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="removeFromEnd">The remove from end.</param>
    /// <returns></returns>
    public static string Chop(this string sourceString, int removeFromEnd)
    {
        var result = sourceString;
        if (removeFromEnd > 0 && sourceString.Length > removeFromEnd - 1)
            result = result.Remove(sourceString.Length - removeFromEnd, removeFromEnd);
        return result;
    }

    #endregion

    #region Chop

    /// <summary>
    ///     Strips the last specified chars from a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="backDownTo">The back down to.</param>
    /// <returns></returns>
    public static string Chop(this string sourceString, string backDownTo)
    {
        var removeDownTo = sourceString.LastIndexOf(backDownTo, StringComparison.Ordinal);
        var removeFromEnd = 0;
        if (removeDownTo > 0)
            removeFromEnd = sourceString.Length - removeDownTo;

        var result = sourceString;

        if (sourceString.Length > removeFromEnd - 1)
            result = result.Remove(removeDownTo, removeFromEnd);

        return result;
    }

    #endregion

    #region PluralToSingular

    /// <summary>
    ///     Plurals to singular.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string PluralToSingular(this string sourceString)
    {
        return sourceString.MakeSingular();
    }

    #endregion

    #region SingularToPlural

    /// <summary>
    ///     Singulars to plural.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string SingularToPlural(this string sourceString)
    {
        return sourceString.MakePlural();
    }

    #endregion

    #region Pluralize

    /// <summary>
    ///     Make plural when count is not one
    /// </summary>
    /// <param name="number">The number of things</param>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string Pluralize(this int number, string sourceString)
    {
        return number == 1
            ? string.Concat(number, " ", sourceString.MakeSingular())
            : $"{number} {sourceString.MakePlural()}";
    }

    #endregion

    #region Clip

    /// <summary>
    ///     Removes the specified chars from the beginning of a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="removeFromBeginning">The remove from beginning.</param>
    /// <returns></returns>
    public static string Clip(this string sourceString, int removeFromBeginning)
    {
        var result = sourceString;
        if (sourceString.Length > removeFromBeginning)
            result = result.Remove(0, removeFromBeginning);
        return result;
    }

    #endregion

    #region Clip

    /// <summary>
    ///     Removes chars from the beginning of a string, up to the specified string
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="removeUpTo">The remove up to.</param>
    /// <returns></returns>
    public static string Clip(this string sourceString, string removeUpTo)
    {
        var removeFromBeginning = sourceString.IndexOf(removeUpTo, StringComparison.Ordinal);
        var result = sourceString;

        if (sourceString.Length > removeFromBeginning && removeFromBeginning > 0)
            result = result.Remove(0, removeFromBeginning);

        return result;
    }

    #endregion

    #region Chop

    /// <summary>
    ///     Strips the last char from a a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string Chop(this string sourceString)
    {
        return Chop(sourceString, 1);
    }

    #endregion

    #region Clip

    /// <summary>
    ///     Strips the last char from a a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string Clip(this string sourceString)
    {
        return Clip(sourceString, 1);
    }

    #endregion

    #region FastReplace

    /// <summary>
    ///     Fasts the replace.
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="replacement">The replacement.</param>
    /// <returns></returns>
    public static string FastReplace(this string original, string pattern, string replacement)
    {
        return FastReplace(original, pattern, replacement, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region FastReplace

    /// <summary>
    ///     Fasts the replace.
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="pattern">The pattern.</param>
    /// <param name="replacement">The replacement.</param>
    /// <param name="comparisonType">Type of the comparison.</param>
    /// <returns></returns>
    public static string FastReplace(this string original, string pattern, string replacement,
        StringComparison comparisonType)
    {
        if (original == null)
            return null;

        if (string.IsNullOrEmpty(pattern))
            return original;

        var lenPattern = pattern.Length;
        var idxPattern = -1;
        var idxLast = 0;

        var result = new StringBuilder();

        while (true)
        {
            idxPattern = original.IndexOf(pattern, idxPattern + 1, comparisonType);

            if (idxPattern < 0)
            {
                result.Append(original, idxLast, original.Length - idxLast);
                break;
            }

            result.Append(original, idxLast, idxPattern - idxLast);
            result.Append(replacement);

            idxLast = idxPattern + lenPattern;
        }

        return result.ToString();
    }

    #endregion

    #region Crop

    /// <summary>
    ///     Returns text that is located between the startText and endText tags.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="startText">The text from which to start the crop</param>
    /// <param name="endText">The endpoint of the crop</param>
    /// <returns></returns>
    public static string Crop(this string sourceString, string startText, string endText)
    {
        var startIndex = sourceString.IndexOf(startText, StringComparison.CurrentCultureIgnoreCase);

        if (startIndex == -1)
            return string.Empty;

        startIndex += startText.Length;

        var endIndex = sourceString.IndexOf(endText, startIndex, StringComparison.CurrentCultureIgnoreCase);

        return endIndex == -1
            ? string.Empty
            : sourceString.Substring(startIndex, endIndex - startIndex);
    }

    #endregion

    #region Squeeze

    /// <summary>
    ///     Removes excess white space in a string.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string Squeeze(this string sourceString)
    {
        char[] delim = { ' ' };
        var lines = sourceString.Split(delim, StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var s in lines)
            if (!string.IsNullOrEmpty(s.Trim()))
                sb.Append(s + " ");
        //remove the last pipe
        var result = Chop(sb.ToString());
        return result.Trim();
    }

    #endregion

    #region ToAlphaNumericOnly

    /// <summary>
    ///     Removes all non-alpha numeric characters in a string
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string ToAlphaNumericOnly(this string sourceString)
    {
        return Regex.Replace(sourceString, @"\W*", "");
    }

    #endregion

    #region OnlyNumbers

    /// <summary>
    ///     Return only numbers of string
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string OnlyNumbers(this string sourceString)
    {
        return !string.IsNullOrWhiteSpace(sourceString)
            ? string.Concat(sourceString.Where(char.IsDigit))
            : string.Empty;
    }

    #endregion

    #region ToWords

    /// <summary>
    ///     Creates a string array based on the words in a sentence
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <returns></returns>
    public static string[] ToWords(this string sourceString)
    {
        var result = sourceString.Trim();
        return result.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }

    #endregion

    #region StripHtml

    /// <summary>
    ///     Strips all HTML tags from a string
    /// </summary>
    /// <param name="htmlString">The HTML string.</param>
    /// <returns></returns>
    public static string StripHtml(this string htmlString)
    {
        return StripHtml(htmlString, string.Empty);
    }

    #endregion

    #region StripHtml

    /// <summary>
    ///     Strips all HTML tags from a string and replaces the tags with the specified replacement
    /// </summary>
    /// <param name="htmlString">The HTML string.</param>
    /// <param name="htmlPlaceHolder">The HTML place holder.</param>
    /// <returns></returns>
    public static string StripHtml(this string htmlString, string htmlPlaceHolder)
    {
        const string pattern = @"<(.|\n)*?>";
        var sOut = Regex.Replace(htmlString, pattern, htmlPlaceHolder);
        sOut = sOut.Replace("&nbsp;", string.Empty);
        sOut = sOut.Replace("&amp;", "&");
        sOut = sOut.Replace("&gt;", ">");
        sOut = sOut.Replace("&lt;", "<");
        return sOut;
    }

    #endregion

    #region FindMatches

    /// <summary>
    ///     Procura padrões.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="find"></param>
    /// <returns></returns>
    public static List<string> FindMatches(this string source, string find)
    {
        var reg = new Regex(find, RegexOptions.IgnoreCase);

        return (from Match m in reg.Matches(source) select m.Value).ToList();
    }

    #endregion

    #region ToDelimitedList

    /// <summary>
    ///     Converts a generic List collection to a single comma-delimitted string.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <returns></returns>
    public static string ToDelimitedList(this IEnumerable<string> list)
    {
        return ToDelimitedList(list, ",");
    }

    #endregion

    #region ToDelimitedList

    /// <summary>
    ///     Converts a generic List collection to a single string using the specified delimitter.
    /// </summary>
    /// <param name="list">The list.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns></returns>
    public static string ToDelimitedList(this IEnumerable<string> list, string delimiter)
    {
        var sb = new StringBuilder();
        foreach (var s in list)
            sb.Append(string.Concat(s, delimiter));
        var result = sb.ToString();
        result = Chop(result);
        return result;
    }

    #endregion

    #region Strip

    /// <summary>
    ///     Strips the specified input.
    /// </summary>
    /// <param name="sourceString">The source string.</param>
    /// <param name="stripValue">The strip value.</param>
    /// <returns></returns>
    public static string Strip(this string sourceString, string stripValue)
    {
        if (string.IsNullOrEmpty(stripValue))
            return sourceString;

        var replace = stripValue.Split(new[] { ',' });

        foreach (var t in replace)
            if (!string.IsNullOrEmpty(sourceString))
                sourceString = Regex.Replace(sourceString, t, string.Empty);

        return sourceString;
    }

    #endregion

    #region AsciiToUnicode

    /// <summary>
    ///     Converts ASCII encoding to Unicode
    /// </summary>
    /// <param name="asciiCode">The ASCII code.</param>
    /// <returns></returns>
    public static string AsciiToUnicode(this int asciiCode)
    {
        var ascii = Encoding.UTF32;
        var c = (char)asciiCode;
        var b = ascii.GetBytes(c.ToString(CultureInfo.InvariantCulture));
        return ascii.GetString(b);
    }

    #endregion

    #region HtmlEncode

    /// <summary>
    ///     Converts Text to HTML-encoded string
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <returns></returns>
    public static string HtmlEncode(this string text)
    {
        text = EntityTable.Aggregate(text, (current, key) => current.Replace(AsciiToUnicode(key.Key), key.Value));
        return text.Replace(AsciiToUnicode(38), "&amp;");
    }

    #endregion

    #region HtmlDecode

    /// <summary>
    ///     Converts HTML-encoded bits to Text
    /// </summary>
    /// <param name="html">The html text.</param>
    /// <returns></returns>
    public static string HtmlDecode(this string html)
    {
        html = html.Replace("&amp;", "&");
        return EntityTable.Aggregate(html, (current, key) => current.Replace(key.Value, AsciiToUnicode(key.Key)));
    }

    #endregion

    #region ToFormattedString

    /// <summary>
    ///     Formats the args using String.Format with the target string as a format string.
    /// </summary>
    /// <param name="fmt">The format string passed to String.Format</param>
    /// <param name="args">The args passed to String.Format</param>
    /// <returns></returns>
    public static string ToFormattedString(this string fmt, params object[] args)
    {
        return string.Format(fmt, args);
    }

    #endregion

    #region ToEnum<T>

    /// <summary>
    ///     Strings to enum.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static T ToEnum<T>(this string value)
    {
        var oOut = default(T);
        var t = typeof(T);
        foreach (var fi in t.GetFields())
            if (fi.Name.Matches(value))
                oOut = (T)fi.GetValue(null);

        return oOut;
    }

    #endregion

    #region FillEntities

    /// <summary>
    ///     Fills the entities.
    /// </summary>
    private static void FillEntities()
    {
        EntityTable.Add(160, "&nbsp;");
        EntityTable.Add(161, "&iexcl;");
        EntityTable.Add(162, "&cent;");
        EntityTable.Add(163, "&pound;");
        EntityTable.Add(164, "&curren;");
        EntityTable.Add(165, "&yen;");
        EntityTable.Add(166, "&brvbar;");
        EntityTable.Add(167, "&sect;");
        EntityTable.Add(168, "&uml;");
        EntityTable.Add(169, "&copy;");
        EntityTable.Add(170, "&ordf;");
        EntityTable.Add(171, "&laquo;");
        EntityTable.Add(172, "&not;");
        EntityTable.Add(173, "&shy;");
        EntityTable.Add(174, "&reg;");
        EntityTable.Add(175, "&macr;");
        EntityTable.Add(176, "&deg;");
        EntityTable.Add(177, "&plusmn;");
        EntityTable.Add(178, "&sup2;");
        EntityTable.Add(179, "&sup3;");
        EntityTable.Add(180, "&acute;");
        EntityTable.Add(181, "&micro;");
        EntityTable.Add(182, "&para;");
        EntityTable.Add(183, "&middot;");
        EntityTable.Add(184, "&cedil;");
        EntityTable.Add(185, "&sup1;");
        EntityTable.Add(186, "&ordm;");
        EntityTable.Add(187, "&raquo;");
        EntityTable.Add(188, "&frac14;");
        EntityTable.Add(189, "&frac12;");
        EntityTable.Add(190, "&frac34;");
        EntityTable.Add(191, "&iquest;");
        EntityTable.Add(192, "&Agrave;");
        EntityTable.Add(193, "&Aacute;");
        EntityTable.Add(194, "&Acirc;");
        EntityTable.Add(195, "&Atilde;");
        EntityTable.Add(196, "&Auml;");
        EntityTable.Add(197, "&Aring;");
        EntityTable.Add(198, "&AElig;");
        EntityTable.Add(199, "&Ccedil;");
        EntityTable.Add(200, "&Egrave;");
        EntityTable.Add(201, "&Eacute;");
        EntityTable.Add(202, "&Ecirc;");
        EntityTable.Add(203, "&Euml;");
        EntityTable.Add(204, "&Igrave;");
        EntityTable.Add(205, "&Iacute;");
        EntityTable.Add(206, "&Icirc;");
        EntityTable.Add(207, "&Iuml;");
        EntityTable.Add(208, "&ETH;");
        EntityTable.Add(209, "&Ntilde;");
        EntityTable.Add(210, "&Ograve;");
        EntityTable.Add(211, "&Oacute;");
        EntityTable.Add(212, "&Ocirc;");
        EntityTable.Add(213, "&Otilde;");
        EntityTable.Add(214, "&Ouml;");
        EntityTable.Add(215, "&times;");
        EntityTable.Add(216, "&Oslash;");
        EntityTable.Add(217, "&Ugrave;");
        EntityTable.Add(218, "&Uacute;");
        EntityTable.Add(219, "&Ucirc;");
        EntityTable.Add(220, "&Uuml;");
        EntityTable.Add(221, "&Yacute;");
        EntityTable.Add(222, "&THORN;");
        EntityTable.Add(223, "&szlig;");
        EntityTable.Add(224, "&agrave;");
        EntityTable.Add(225, "&aacute;");
        EntityTable.Add(226, "&acirc;");
        EntityTable.Add(227, "&atilde;");
        EntityTable.Add(228, "&auml;");
        EntityTable.Add(229, "&aring;");
        EntityTable.Add(230, "&aelig;");
        EntityTable.Add(231, "&ccedil;");
        EntityTable.Add(232, "&egrave;");
        EntityTable.Add(233, "&eacute;");
        EntityTable.Add(234, "&ecirc;");
        EntityTable.Add(235, "&euml;");
        EntityTable.Add(236, "&igrave;");
        EntityTable.Add(237, "&iacute;");
        EntityTable.Add(238, "&icirc;");
        EntityTable.Add(239, "&iuml;");
        EntityTable.Add(240, "&eth;");
        EntityTable.Add(241, "&ntilde;");
        EntityTable.Add(242, "&ograve;");
        EntityTable.Add(243, "&oacute;");
        EntityTable.Add(244, "&ocirc;");
        EntityTable.Add(245, "&otilde;");
        EntityTable.Add(246, "&ouml;");
        EntityTable.Add(247, "&divide;");
        EntityTable.Add(248, "&oslash;");
        EntityTable.Add(249, "&ugrave;");
        EntityTable.Add(250, "&uacute;");
        EntityTable.Add(251, "&ucirc;");
        EntityTable.Add(252, "&uuml;");
        EntityTable.Add(253, "&yacute;");
        EntityTable.Add(254, "&thorn;");
        EntityTable.Add(255, "&yuml;");
        EntityTable.Add(402, "&fnof;");
        EntityTable.Add(913, "&Alpha;");
        EntityTable.Add(914, "&Beta;");
        EntityTable.Add(915, "&Gamma;");
        EntityTable.Add(916, "&Delta;");
        EntityTable.Add(917, "&Epsilon;");
        EntityTable.Add(918, "&Zeta;");
        EntityTable.Add(919, "&Eta;");
        EntityTable.Add(920, "&Theta;");
        EntityTable.Add(921, "&Iota;");
        EntityTable.Add(922, "&Kappa;");
        EntityTable.Add(923, "&Lambda;");
        EntityTable.Add(924, "&Mu;");
        EntityTable.Add(925, "&Nu;");
        EntityTable.Add(926, "&Xi;");
        EntityTable.Add(927, "&Omicron;");
        EntityTable.Add(928, "&Pi;");
        EntityTable.Add(929, "&Rho;");
        EntityTable.Add(931, "&Sigma;");
        EntityTable.Add(932, "&Tau;");
        EntityTable.Add(933, "&Upsilon;");
        EntityTable.Add(934, "&Phi;");
        EntityTable.Add(935, "&Chi;");
        EntityTable.Add(936, "&Psi;");
        EntityTable.Add(937, "&Omega;");
        EntityTable.Add(945, "&alpha;");
        EntityTable.Add(946, "&beta;");
        EntityTable.Add(947, "&gamma;");
        EntityTable.Add(948, "&delta;");
        EntityTable.Add(949, "&epsilon;");
        EntityTable.Add(950, "&zeta;");
        EntityTable.Add(951, "&eta;");
        EntityTable.Add(952, "&theta;");
        EntityTable.Add(953, "&iota;");
        EntityTable.Add(954, "&kappa;");
        EntityTable.Add(955, "&lambda;");
        EntityTable.Add(956, "&mu;");
        EntityTable.Add(957, "&nu;");
        EntityTable.Add(958, "&xi;");
        EntityTable.Add(959, "&omicron;");
        EntityTable.Add(960, "&pi;");
        EntityTable.Add(961, "&rho;");
        EntityTable.Add(962, "&sigmaf;");
        EntityTable.Add(963, "&sigma;");
        EntityTable.Add(964, "&tau;");
        EntityTable.Add(965, "&upsilon;");
        EntityTable.Add(966, "&phi;");
        EntityTable.Add(967, "&chi;");
        EntityTable.Add(968, "&psi;");
        EntityTable.Add(969, "&omega;");
        EntityTable.Add(977, "&thetasym;");
        EntityTable.Add(978, "&upsih;");
        EntityTable.Add(982, "&piv;");
        EntityTable.Add(8226, "&bull;");
        EntityTable.Add(8230, "&hellip;");
        EntityTable.Add(8242, "&prime;");
        EntityTable.Add(8243, "&Prime;");
        EntityTable.Add(8254, "&oline;");
        EntityTable.Add(8260, "&frasl;");
        EntityTable.Add(8472, "&weierp;");
        EntityTable.Add(8465, "&image;");
        EntityTable.Add(8476, "&real;");
        EntityTable.Add(8482, "&trade;");
        EntityTable.Add(8501, "&alefsym;");
        EntityTable.Add(8592, "&larr;");
        EntityTable.Add(8593, "&uarr;");
        EntityTable.Add(8594, "&rarr;");
        EntityTable.Add(8595, "&darr;");
        EntityTable.Add(8596, "&harr;");
        EntityTable.Add(8629, "&crarr;");
        EntityTable.Add(8656, "&lArr;");
        EntityTable.Add(8657, "&uArr;");
        EntityTable.Add(8658, "&rArr;");
        EntityTable.Add(8659, "&dArr;");
        EntityTable.Add(8660, "&hArr;");
        EntityTable.Add(8704, "&forall;");
        EntityTable.Add(8706, "&part;");
        EntityTable.Add(8707, "&exist;");
        EntityTable.Add(8709, "&empty;");
        EntityTable.Add(8711, "&nabla;");
        EntityTable.Add(8712, "&isin;");
        EntityTable.Add(8713, "&notin;");
        EntityTable.Add(8715, "&ni;");
        EntityTable.Add(8719, "&prod;");
        EntityTable.Add(8721, "&sum;");
        EntityTable.Add(8722, "&minus;");
        EntityTable.Add(8727, "&lowast;");
        EntityTable.Add(8730, "&radic;");
        EntityTable.Add(8733, "&prop;");
        EntityTable.Add(8734, "&infin;");
        EntityTable.Add(8736, "&ang;");
        EntityTable.Add(8743, "&and;");
        EntityTable.Add(8744, "&or;");
        EntityTable.Add(8745, "&cap;");
        EntityTable.Add(8746, "&cup;");
        EntityTable.Add(8747, "&int;");
        EntityTable.Add(8756, "&there4;");
        EntityTable.Add(8764, "&sim;");
        EntityTable.Add(8773, "&cong;");
        EntityTable.Add(8776, "&asymp;");
        EntityTable.Add(8800, "&ne;");
        EntityTable.Add(8801, "&equiv;");
        EntityTable.Add(8804, "&le;");
        EntityTable.Add(8805, "&ge;");
        EntityTable.Add(8834, "&sub;");
        EntityTable.Add(8835, "&sup;");
        EntityTable.Add(8836, "&nsub;");
        EntityTable.Add(8838, "&sube;");
        EntityTable.Add(8839, "&supe;");
        EntityTable.Add(8853, "&oplus;");
        EntityTable.Add(8855, "&otimes;");
        EntityTable.Add(8869, "&perp;");
        EntityTable.Add(8901, "&sdot;");
        EntityTable.Add(8968, "&lceil;");
        EntityTable.Add(8969, "&rceil;");
        EntityTable.Add(8970, "&lfloor;");
        EntityTable.Add(8971, "&rfloor;");
        EntityTable.Add(9001, "&lang;");
        EntityTable.Add(9002, "&rang;");
        EntityTable.Add(9674, "&loz;");
        EntityTable.Add(9824, "&spades;");
        EntityTable.Add(9827, "&clubs;");
        EntityTable.Add(9829, "&hearts;");
        EntityTable.Add(9830, "&diams;");
        EntityTable.Add(34, "&quot;");
        //_entityTable.Add(38, "&amp;");
        EntityTable.Add(60, "&lt;");
        EntityTable.Add(62, "&gt;");
        EntityTable.Add(338, "&OElig;");
        EntityTable.Add(339, "&oelig;");
        EntityTable.Add(352, "&Scaron;");
        EntityTable.Add(353, "&scaron;");
        EntityTable.Add(376, "&Yuml;");
        EntityTable.Add(710, "&circ;");
        EntityTable.Add(732, "&tilde;");
        EntityTable.Add(8194, "&ensp;");
        EntityTable.Add(8195, "&emsp;");
        EntityTable.Add(8201, "&thinsp;");
        EntityTable.Add(8204, "&zwnj;");
        EntityTable.Add(8205, "&zwj;");
        EntityTable.Add(8206, "&lrm;");
        EntityTable.Add(8207, "&rlm;");
        EntityTable.Add(8211, "&ndash;");
        EntityTable.Add(8212, "&mdash;");
        EntityTable.Add(8216, "&lsquo;");
        EntityTable.Add(8217, "&rsquo;");
        EntityTable.Add(8218, "&sbquo;");
        EntityTable.Add(8220, "&ldquo;");
        EntityTable.Add(8221, "&rdquo;");
        EntityTable.Add(8222, "&bdquo;");
        EntityTable.Add(8224, "&dagger;");
        EntityTable.Add(8225, "&Dagger;");
        EntityTable.Add(8240, "&permil;");
        EntityTable.Add(8249, "&lsaquo;");
        EntityTable.Add(8250, "&rsaquo;");
        EntityTable.Add(8364, "&euro;");
    }

    #endregion

    #region [ Truncate ]

    /// <summary>
    ///     Truncar (reduz) string ao tamanho passado como parâmetro
    /// </summary>
    /// <param name="value">Valor a ser truncado</param>
    /// <param name="maxLength">Tamanho maxímo retornado</param>
    /// <returns></returns>
    public static string Truncate(this string value, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength) return value.Substring(0, maxLength);
        return value;
    }

    #endregion [ Truncate ]

    #region [ RemoveCharactersSpecial ]

    public static string RemoveCharactersSpecial(this string text, bool aceitaEspaco = true, bool replaceAccents = true)
    {
        var ret = text;

        if (string.IsNullOrEmpty(ret))
            return ret;

        if (replaceAccents)
            ret = RemoveAccents(ret);

        ret = Regex.Replace(ret,
            aceitaEspaco
                ? @"[^0-9a-zA-ZéúíóáÉÚÍÓÁèùìòàÈÙÌÒÀõãñÕÃÑêûîôâÊÛÎÔÂëÿüïöäËYÜÏÖÄçÇ\s]+?"
                : @"[^0-9a-zA-ZéúíóáÉÚÍÓÁèùìòàÈÙÌÒÀõãñÕÃÑêûîôâÊÛÎÔÂëÿüïöäËYÜÏÖÄçÇ]+?", string.Empty);

        return ret;
    }

    #endregion [ RemoveCharactersSpecial ]
}