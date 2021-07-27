namespace Messenger.Core.Models
{
    /// <summary>
    /// Represents an entity that can be mentioned in a message
    /// </summary>
    public enum MentionTarget
    {
        User,
        Role,
        Channel,
        Message,
        All
    }
}
