namespace Messenger.Core.Models
{
    /// <summary>
    /// Holds types of notifications
    /// </summary>
    public enum NotificationType
    {
        UserMentioned,
        MessageInSubscribedChannel,
        MessageInSubscribedTeam,
        MessageInPrivateChat,
        InvitedToTeam,
        RemovedFromTeam,
        ReactionToMessage,
    }
}
