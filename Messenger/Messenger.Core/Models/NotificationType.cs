namespace Messenger.Core.Models
{
    public enum NotificationType
    {
        UserMentioned,
        MessageInSubscribedChannel,
        MessageInSubscribedTeam,
        MessageInPrivateChat,
        InvitedToTeam,
        RemovedFromTeam,
        ReactionToMessage,
        MessageDeleted
    }
}
