using Messenger.Helpers;
using Messenger.Core.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands.TeamManage;
using Messenger.Core.Services;
using Windows.UI.Xaml.Controls;
using Messenger.Models;

namespace Messenger.ViewModels.DialogBoxes
{
    public class ManageRolesDialogViewModel : Observable
    {
        private string _newRoleTitle;

        private TeamRoleViewModel _selectedTeamRole;

        private ObservableCollection<Permissions> _grantablePermissions;

        private bool _hasFullPermissions;

        private ObservableCollection<TeamRoleViewModel> _teamRoles;

        public ObservableCollection<TeamRoleViewModel> TeamRoles
        {
            get { return _teamRoles; }
            set { Set(ref _teamRoles, value); }
        }

        public ObservableCollection<Permissions> GrantablePermissions
        {
            get { return _grantablePermissions; }
            set { Set(ref _grantablePermissions, value); }
        }

        public TeamRoleViewModel SelectedTeamRole
        {
            get { return _selectedTeamRole; }
            set
            {
                if (value == null) return;

                bool hasFullPermissions = true;

                foreach (Permissions permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
                {
                    hasFullPermissions &= value.Permissions.Any(p => p == permission);
                }

                HasFullPermissions = hasFullPermissions;

                Set(ref _selectedTeamRole, value);
            }
        }

        public string NewRoleTitle
        {
            get { return _newRoleTitle; }
            set { Set(ref _newRoleTitle, value); }
        }

        public bool HasFullPermissions
        {
            get { return _hasFullPermissions; }
            set { Set(ref _hasFullPermissions, value); }
        }

        public SelectionChangedEventHandler SelectedTeamRoleChanged;

        public ICommand CreateTeamRoleCommand { get => new CreateTeamRoleCommand(this); }

        public ICommand GrantPermissionCommand { get => new GrantPermissionCommand(this); }

        public ICommand RevokePermissionCommand { get => new RevokePermissionCommand(this); }

        public ManageRolesDialogViewModel()
        {
            App.EventProvider.TeamUpdated += OnTeamUpdated;
            SelectedTeamRoleChanged += OnSelectedTeamRoleChanged;
            GrantablePermissions = new ObservableCollection<Permissions>();

            /** REFERENCE TO STATE (UPDATES AUTOMATICALLY) **/
            TeamRoles = App.StateProvider.SelectedTeam.TeamRoles;
        }

        private void OnTeamUpdated(object sender, BroadcastArgs args)
        {
            TeamViewModel team = (TeamViewModel)args.Payload;

            /** UPDATE SELECTED TEAM ROLE FROM THE PAYLOAD **/
            if (SelectedTeamRole != null)
            {
                TeamRoleViewModel selectedRole = TeamRoles.SingleOrDefault(r => r.Id == SelectedTeamRole.Id);

                SelectedTeamRole = selectedRole;
            }
        }

        private void OnSelectedTeamRoleChanged(object sender, SelectionChangedEventArgs e) => FilterGrantablePermissions();

        private void FilterGrantablePermissions()
        {
            GrantablePermissions.Clear();

            foreach (Permissions permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
            {
                if (SelectedTeamRole.Permissions.Any(p => p == permission))
                {
                    continue;
                }

                GrantablePermissions.Add(permission);
            }

            if (GrantablePermissions.Count() > 0)
            {
                HasFullPermissions = false;
            }
            else
            {
                HasFullPermissions = true;
            }
        }
    }
}
