using System;

using Messenger.Helpers;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
                _shellViewModel.ChatName = "Chatname";
            } }

        public ChatViewModel()
        {
        }

    }
}
