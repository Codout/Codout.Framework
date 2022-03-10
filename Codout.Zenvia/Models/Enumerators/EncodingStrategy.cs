using System.Runtime.Serialization;

namespace Codout.Zenvia.Models.Enumerators
{
    /// <summary>
    /// The method used for selecting the message encoding used to dispatch the message to the provider
    /// The default value AUTO will select the encoding method based on the text content, so this is only necessary
    /// if you need to enforce MORE_CHARACTERS_PER_MESSAGE method (not recommended regarding readability),
    /// or to enforce MORE_CHARACTER_SUPPORT if you have any trouble with AUTO.
    /// </summary>
    public enum EncodingStrategy
    {
        [EnumMember(Value = "AUTO")] 
        AUTO,

        [EnumMember(Value = "MORE_CHARACTER_SUPPORT")] 
        MORE_CHARACTER_SUPPORT,

        [EnumMember(Value = "MORE_CHARACTERS_PER_MESSAGE")] 
        MORE_CHARACTERS_PER_MESSAGE
    }
}