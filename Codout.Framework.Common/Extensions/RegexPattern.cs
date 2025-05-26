namespace Codout.Framework.Common.Extensions;

/// <summary>
/// Extensões comuns para tipos relacionadas a expressões regulares.
/// </summary>
public class RegexPattern
{
    public const string Alpha = "[^a-zA-Z]";
    public const string AlphaNumeric = "[^a-zA-Z0-9]";
    public const string AlphaNumericSpace = @"[^a-zA-Z0-9\s]";
    public const string CreditCardAmericanExpress = @"^(?:(?:[3][4|7])(?:\d{13}))$";
    public const string CreditCardCarteBlanche = @"^(?:(?:[3](?:[0][0-5]|[6|8]))(?:\d{11,12}))$";
    public const string CreditCardDinersClub = @"^(?:(?:[3](?:[0][0-5]|[6|8]))(?:\d{11,12}))$";
    public const string CreditCardDiscover = @"^(?:(?:6011)(?:\d{12}))$";
    public const string CreditCardEnRoute = @"^(?:(?:[2](?:014|149))(?:\d{11}))$";
    public const string CreditCardJcb = @"^(?:(?:(?:2131|1800)(?:\d{11}))$|^(?:(?:3)(?:\d{15})))$";
    public const string CreditCardMasterCard = @"^(?:(?:[5][1-5])(?:\d{14}))$";
    public const string CreditCardStripNonNumeric = @"(\-|\s|\D)*";
    public const string CreditCardVisa = @"^(?:(?:[4])(?:\d{12}|\d{15}))$";
    public const string Email = @"^([0-9a-zA-Z]+[-._&]+)*[0-9a-zA-Z]+[-._&]*?@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$";
    public const string EmbeddedClassNameMatch = "(?<=^_).*?(?=_)";
    public const string EmbeddedClassNameReplace = "^_.*?_";
    public const string EmbeddedClassNameUnderscoreMatch = "(?<=^UNDERSCORE).*?(?=UNDERSCORE)";
    public const string EmbeddedClassNameUnderscoreReplace = "^UNDERSCORE.*?UNDERSCORE";
    public const string Guid = "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
    public const string IpAddress = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
    public const string LowerCase = @"^[a-z]+$";
    public const string Numeric = "[^0-9]";
    public const string SocialSecurity = @"^\d{3}[-]?\d{2}[-]?\d{4}$";
    public const string SqlEqual = @"\=";
    public const string SqlGreater = @"\>";
    public const string SqlGreaterOrEqual = @"\>.*\=";
    public const string SqlIs = @"\x20is\x20";
    public const string SqlIsNot = @"\x20is\x20not\x20";
    public const string SqlLess = @"\<";
    public const string SqlLessOrEqual = @"\<.*\=";
    public const string SqlLike = @"\x20like\x20";
    public const string SqlNotEqual = @"\<.*\>";
    public const string SqlNotLike = @"\x20not\x20like\x20";
    public const string StrongPassword = @"(?=^.{8,255}$)((?=.*\d)(?=.*[A-Z])(?=.*[a-z])|(?=.*\d)(?=.*[^A-Za-z0-9])(?=.*[a-z])|(?=.*[^A-Za-z0-9])(?=.*[A-Z])(?=.*[a-z])|(?=.*\d)(?=.*[A-Z])(?=.*[^A-Za-z0-9]))^.*";
    public const string UpperCase = @"^[A-Z]+$";
    public const string Url = @"^^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
    public const string UsCurrency = @"^\$(([1-9]\d*|([1-9]\d{0,2}(\,\d{3})*))(\.\d{1,2})?|(\.\d{1,2}))$|^\$[0](.00)?$";
    public const string UsTelephone = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
    public const string UsZipcode = @"^\d{5}$";
    public const string UsZipcodePlusFour = @"^\d{5}((-|\s)?\d{4})$";
    public const string UsZipcodePlusFourOptional = @"^\d{5}((-|\s)?\d{4})?$";
}
