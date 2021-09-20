using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using Messenger.Views.DialogBoxes;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Messenger.ViewModels.Controls
{
    public class MembersListControlViewModel : Observable
    {
        private ObservableCollection<MemberViewModel> _members;

        public ObservableCollection<MemberViewModel> Members
        {
            get { return _members; }
            set { Set(ref _members, value); }
        }

        public TeamManageViewModel ParentViewModel { get; set; }

        public ICommand OpenManageRolesCommand { get => new RelayCommand(async () => await new ManageRolesDialog().ShowAsync()); }

        public ICommand OpenManageMemberRolesCommand { get => new RelayCommand<MemberViewModel>(async (member) => await new ManageMemberRolesDialog(member).ShowAsync()); }

        public ICommand RemoveUserCommand { get => new RemoveUserCommand(); }

        public MembersListControlViewModel(TeamManageViewModel parentViewModel)
        {
            Members = new ObservableCollection<MemberViewModel>();

            foreach (MemberViewModel member in App.StateProvider.SelectedTeam.Members)
            {
                Members.Add(member);
            }

            ParentViewModel = parentViewModel;

            App.EventProvider.TeamUpdated += OnTeamUpdated;
        }

        public void OnTeamUpdated(object sender, BroadcastArgs e)
        {
            TeamViewModel team = new TeamViewModel(e.Payload as TeamViewModel);

            if (team == null)
            {   
                return;
            }

            if (e.Reason == BroadcastReasons.Updated)
            {
                Members.Clear();

                foreach (MemberViewModel member in team.Members)
                {
                    Members.Add(member);
                }
            }
        }
    }
}
