using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Helpers;
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
            Members = App.StateProvider.SelectedTeam.Members;

            ParentViewModel = parentViewModel;
        }
    }
}
