namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a team's channel
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// The unique id  of the channel
        /// </summary>
        public uint ChannelId { get; set; }

        /// <summary>
        /// The name of the channel
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The id of the team the channel belongs to
        /// </summary>
        public uint TeamId { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public Channel()
        {
            ChannelName = "";
        }

        public override string ToString()
        {
            return $"Channel: ChannelId={ChannelId}, ChannelName={ChannelName}, TeamId={TeamId}";
        }
    }
}
