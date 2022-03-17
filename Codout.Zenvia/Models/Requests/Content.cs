using Codout.Zenvia.Models.Enumerators;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Codout.Zenvia.Models.Requests
{
    public class Content
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContentType Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
