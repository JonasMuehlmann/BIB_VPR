namespace Messenger.Core.Models
{
    /// <summary>
    /// Holds permissions assignable to a team's roles
    /// </summary>
    public enum Permissions
    {
        CanAddUser,
        CanRemoveUser,
        CanAddRole,
        CanRemoveRole,
        CanAssignRole,
        CanUnassignRole,
        CanChangeTeamName,
        CanChangeTeamDescription,
        CanAddChannel,
        CanRemoveChannel,
        CanChangeChannelName,
        CanChangeChannelDescription,
        CanAttachFiles,
        CanDeleteTeam,
        CanPinMessages,
        CanUnpinMessages
    }
}
