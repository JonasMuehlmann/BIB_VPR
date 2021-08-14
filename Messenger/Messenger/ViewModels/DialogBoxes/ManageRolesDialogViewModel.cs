using Messenger.Helpers;
using Messenger.Core.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.TeamManage;
using Messenger.Models;

namespace Messenger.ViewModels.DialogBoxes
{
    public class ManageRolesDialogViewModel : Observable
    {
        #region Private

        private string _newRoleTitle;

        private TeamRoleViewModel _selectedTeamRole;

        private ObservableCollection<Permissions> _grantablePermissions;

        private bool _hasFullPermissions;

        private bool _hasChanged;

        private bool _isInEditMode;

        private TeamRoleViewModel _pendingChange;

        private ObservableCollection<TeamRoleViewModel> _teamRoles;

        #endregion

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

        /// <summary>
        /// Actual model of selected team role
        /// </summary>
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

                /** CREATE SEPARATE INSTANCE FROM THE SELECTED VIEW MODEL **/
                PendingChange = new TeamRoleViewModel(value);

                Set(ref _selectedTeamRole, value);
            }
        }

        /// <summary>
        /// A copy of selected team role that can be modified with ui to make & save changes
        /// </summary>
        public TeamRoleViewModel PendingChange
        {
            get { return _pendingChange; }
            set { Set(ref _pendingChange, value); }
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

        public bool HasChanged
        {
            get { return _hasChanged; }
            set { Set(ref _hasChanged, value); }
        }

        public bool IsInEditMode
        {
            get { return _isInEditMode; }
            set { Set(ref _isInEditMode, value); }
        }

        #region Commands

        public ICommand CreateTeamRoleCommand { get => new CreateTeamRoleCommand(this); }

        public ICommand UpdateTeamRoleCommand { get => new UpdateTeamRoleCommand(); }

        public ICommand RemoveTeamRoleCommand { get => new RemoveTeamRoleCommand(); }

        public ICommand RevokePermissionCommand { get => new RevokePermissionCommand(this); }

        #endregion

        public ManageRolesDialogViewModel()
        {
            GrantablePermissions = new ObservableCollection<Permissions>();

            /** REFERENCE TO GLOBAL STATE **/
            TeamRoles = App.StateProvider.SelectedTeam.TeamRoles;

            /** EVENTS REGISLATION **/
            App.EventProvider.TeamUpdated += OnTeamUpdated;
        }

        #region Events

        public void OnTeamUpdated(object sender, BroadcastArgs args)
        {
            TeamViewModel team = (TeamViewModel)args.Payload;

            /** UPDATE SELECTED TEAM ROLE FROM THE PAYLOAD **/
            if (SelectedTeamRole != null)
            {
                TeamRoleViewModel selectedRole = TeamRoles.SingleOrDefault(r => r.Id == SelectedTeamRole.Id);

                SelectedTeamRole = selectedRole;
            }
        }

        #endregion

        #region Helpers

        public void FilterGrantablePermissions()
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

        #endregion
    }
}
