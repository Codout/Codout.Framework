using System;
using System.Collections.Generic;
using Codout.Zenvia.Models.Enumerators;
using Newtonsoft.Json;

namespace Codout.Zenvia.Models.Responses
{
    public abstract class MessageSms : BaseObject
    {
        /// <summary>
        /// An ID for the message. It can be used for future message consulting or callback notifications.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        
        /// <summary>
        /// The identifier of the sender of the message. The sender is created when an integration for the channel is connected on the integrations console.
        /// More details on the channel's sender and recipient section.
        /// </summary>
        [JsonProperty("from")]
        public string From { get; set; }

        /// <summary>
        /// The identifier of the recipient (varies according to the channel) of the message.
        /// More details on the channel's sender and recipient section.
        /// </summary>
        [JsonProperty("to")]
        public string To { get; set; }
        
        /// <summary>
        /// Indicates whether the message is received from a channel (IN) or sent to a channel (OUT)
        /// </summary>
        [JsonProperty("direction")]
        public Direction? Direction { get; set; }
        
        /// <summary>
        /// Message channel
        /// </summary>
        [JsonProperty("channel")]
        public string Channel { get; set; }

        /// <summary>
        /// The list of contents to be sent
        /// </summary>
        [JsonProperty("contents")]
        public IList<Content> Contents { get; set; }

        /// <summary>
        /// Timestamp of the message. Usually received from the provider of the channel.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
    }
}
