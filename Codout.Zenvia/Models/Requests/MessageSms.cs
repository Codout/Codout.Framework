using System.Collections.Generic;
using Newtonsoft.Json;

namespace Codout.Zenvia.Models.Requests
{
    public class MessageSms : BaseObject
    {
        [JsonProperty("from")]
        public string From { get; set; }
        
        [JsonProperty("to")]
        public string To { get; set; }
        
        [JsonProperty("contents")]
        public IList<Content> Contents { get; set; }
    }
}
