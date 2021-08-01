using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.ViewModels.Controls;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Messenger.ViewModels.Pages
{
    public class TeamManageViewModel : Observable
    {
        public ICommand CreateChannelCommand => new CreateChannelCommand();

        public ICommand InviteUserCommand => new InviteUserCommand();

        public ICommand RemoveChannelClick => new RemoveChannelCommand();

        public ICommand NavigateBackCommand => new RelayCommand(() => NavigationService.Open<ChatPage>());

        public UserViewModel CurrentUser => App.StateProvider.CurrentUser;

        public TeamViewModel SelectedTeam => App.StateProvider.SelectedTeam;

        public MembersListControlViewModel MembersListControlViewModel { get; set; }

        public UserSearchPanelViewModel UserSearchPanelViewModel { get; set; }

        public TeamManageViewModel()
        {
            MembersListControlViewModel = new MembersListControlViewModel(this);
            UserSearchPanelViewModel = new UserSearchPanelViewModel(UserSearchFilter);
        }

        private bool UserSearchFilter(string resultString)
        {
            string[] data = resultString.Split('#');

            string name = data[0];
            uint nameId = Convert.ToUInt32(data[1]);

            bool isMember = SelectedTeam.Members
                .Any(member => member.Name == name && member.NameId == nameId);

            return !isMember;
        }
    }
}
