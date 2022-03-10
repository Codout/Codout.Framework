using Codout.Zenvia.Models.Enumerators;
using Newtonsoft.Json;

namespace Codout.Zenvia.Models.Requests
{
    public class Content
    {
        [JsonProperty("type")]
        public ContentType Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
