using Messenger.ViewModels.DataViewModels;

namespace Messenger.Models
{
    public class ToggleReactionArg
    {
        public ReactionType Type { get; set; }

        public MessageViewModel Message { get; set; }
    }
}
