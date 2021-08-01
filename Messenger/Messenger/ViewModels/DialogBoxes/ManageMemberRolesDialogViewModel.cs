using Messenger.Commands.TeamManage;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Messenger.ViewModels.DialogBoxes
{
    public class ManageMemberRolesDialogViewModel : Observable
    {
        private MemberViewModel _member;

        private TeamRoleViewModel _selectedAssignableTeamRole;

        private TeamRoleViewModel _selectedAssignedTeamRole;

        public MemberViewModel Member
        {
            get { return _member; }
            set { Set(ref _member, value); }
        }

        public TeamRoleViewModel SelectedAssignableTeamRole
        {
            get { return _selectedAssignableTeamRole; }
            set
            {
                if (SelectedAssignedTeamRole != null)
                {
                    SelectedAssignedTeamRole = null;
                }

                Set(ref _selectedAssignableTeamRole, value);
            }
        }

        public TeamRoleViewModel SelectedAssignedTeamRole
        {
            get { return _selectedAssignedTeamRole; }
            set
            {
                if (SelectedAssignableTeamRole != null)
                {
                    SelectedAssignableTeamRole = null;
                }

                Set(ref _selectedAssignedTeamRole, value);
            }
        }

        public ICommand AssignTeamRoleCommand { get => new AssignTeamRoleCommand(this); }

        public ICommand UnassignTeamRoleCommand { get => new UnassignTeamRoleCommand(this); }

        public ManageMemberRolesDialogViewModel(MemberViewModel member)
        {
            Member = member;

            Initialize();
        }

        private async void Initialize()
        {
            if (Member.MemberRoles.Count > 0)
            {
                foreach (TeamRoleViewModel memberRole in Member.MemberRoles)
                {
                    IEnumerable<Permissions> permissions = await TeamService.GetPermissionsOfRole(memberRole.TeamId, memberRole.Title);

                    if (permissions == null || permissions.Count() <= 0) return;

                    memberRole.Permissions.Clear();

                    foreach (Permissions permission in permissions)
                    {
                        memberRole.Permissions.Add(permission);
                    }
                }
            }

            if (Member.AssignableMemberRoles.Count > 0)
            {
                foreach (TeamRoleViewModel assignable in Member.AssignableMemberRoles)
                {
                    IEnumerable<Permissions> permissions = await TeamService.GetPermissionsOfRole(assignable.TeamId, assignable.Title);

                    if (permissions == null || permissions.Count() <= 0) return;

                    assignable.Permissions.Clear();

                    foreach (Permissions permission in permissions)
                    {
                        assignable.Permissions.Add(permission);
                    }
                }
            }
        }

        public void OnTeamUpdated(object sender, BroadcastArgs args)
        {
            if (Member == null) return;

            TeamViewModel team = args.Payload as TeamViewModel;
            MemberViewModel member = team.Members.SingleOrDefault(m => m.Id == Member.Id);

            Member = member;
        }
    }
}
