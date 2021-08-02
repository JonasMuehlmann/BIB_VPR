using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.ViewModels.Controls;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Messenger.ViewModels.DialogBoxes
{
    public class CreateChatDialogViewModel : Observable
    {
        public UserViewModel CurrentUser { get => App.StateProvider.CurrentUser; }

        public IReadOnlyCollection<PrivateChatViewModel> MyChats { get => CacheQuery.GetMyChats(); }

        public User SelectedUser { get => UserSearchPanelViewModel?.SelectedUser; }

        public UserSearchPanelViewModel UserSearchPanelViewModel { get; set; }

        public CreateChatDialogViewModel()
        {
            UserSearchPanelViewModel = new UserSearchPanelViewModel(UserSearchFilter);
        }

        private bool UserSearchFilter(string resultString)
        {
            string[] data = resultString.Split('#');

            string name = data[0];
            uint nameId = Convert.ToUInt32(data[1]);

            bool exists = MyChats.Where((chat) => chat.Partner.Name == name && chat.Partner.NameId == nameId).Count() > 0;

            /** SEARCH ONLY CURRENT USER HAS NOT STARTED A CHAT WITH **/
            return !exists;
        }
    }
}
