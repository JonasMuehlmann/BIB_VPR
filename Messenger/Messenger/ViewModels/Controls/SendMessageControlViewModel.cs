using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels.Controls
{
    public class SendMessageControlViewModel : Observable
    {
        public ChatViewModel ParentViewModel { get; set; }

        public EmojiPicker EmojiPicker { get; set; }

        private ObservableCollection<Emoji> _emojis;

        public ObservableCollection<Emoji> Emojis
        {
            get { return _emojis; }
            set { Set(ref _emojis, value); }
        }

        public IEnumerable<string> EmojiCategories { get => Enum.GetNames(typeof(EmojiCategory)); }

        public ICommand AttachFileCommand { get => new AttachFileCommand(ParentViewModel); }

        public ICommand SendMessageCommand { get => new SendMessageCommand(ParentViewModel); }

        public SendMessageControlViewModel(ChatViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            EmojiPicker = new EmojiPicker("");

            Emojis = new ObservableCollection<Emoji>(EmojiPicker.GetEmojisFromCategory(EmojiCategory.Smileys));
        }
    }
}
