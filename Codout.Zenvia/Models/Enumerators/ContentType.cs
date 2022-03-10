using System.Runtime.Serialization;

namespace Codout.Zenvia.Models.Enumerators
{
    public enum ContentType
    {
        [EnumMember(Value = "text")] 
        Text,
        
        [EnumMember(Value = "template")] 
        Template,
    }
}