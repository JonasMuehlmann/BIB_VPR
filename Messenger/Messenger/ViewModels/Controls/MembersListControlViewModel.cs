using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
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

        public ICommand OpenManageRolesCommand { get => new RelayCommand(async () => await new ManageRolesDialog().ShowAsync()); }

        public ICommand OpenManageMemberRolesCommand { get => new RelayCommand<MemberViewModel>(async (member) => await new ManageMemberRolesDialog(member).ShowAsync()); }

        public ICommand RemoveUserCommand { get => new RemoveUserCommand(); }

        public MembersListControlViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            Members = App.StateProvider.SelectedTeam.Members;
        }
    }
}
