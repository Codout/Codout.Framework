using Codout.Zenvia.Models.Enumerators;
using Newtonsoft.Json;

namespace Codout.Zenvia.Models.Responses
{
    public class Content
    {
        /// <summary>
        /// Content type discriminator
        /// </summary>
        [JsonProperty("type")]
        public ContentType Type { get; set; }

        /// <summary>
        /// Text to be sent
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Payload of selected button
        /// </summary>
        [JsonProperty("payload")]
        public string Payload { get; set; }

        /// <summary>
        /// The method used for selecting the message encoding used to dispatch the message to the provider.
        /// The default value AUTO will select the encoding method based on the text content, so this is only necessary if you need to enforce MORE_CHARACTERS_PER_MESSAGE method (not recommended regarding readability), or to enforce MORE_CHARACTER_SUPPORT if you have any trouble with AUTO.
        /// </summary>
        [JsonProperty("encodingStrategy")]
        public EncodingStrategy? EncodingStrategy { get; set; }
    }
}
