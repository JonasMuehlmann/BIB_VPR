
using System.Collections.Generic;


namespace Messenger.Core.Models
{
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
        CanDeleteTeam
    }
}
