namespace Codout.Zenvia.Models.Enumerators
{
    /// <summary>
    /// Indicates whether the message is received from a channel (IN) or sent to a channel (OUT)
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// Received from a channel 
        /// </summary>
        In,

        /// <summary>
        /// Sent to a channel
        /// </summary>
        Out,
    }
}