
namespace Messenger.Core.Models
{
    public class Channel
    {
        public uint ChannelId { get; set; }

        public string ChannelName { get; set; }

        public uint TeamId { get; set; }

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
