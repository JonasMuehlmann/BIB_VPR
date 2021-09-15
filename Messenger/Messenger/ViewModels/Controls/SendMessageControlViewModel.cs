using Messenger.Commands;
using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
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

        private ObservableCollection<EmojiCategory> _appliedFilters;

        public ObservableCollection<EmojiCategory> AppliedFilters
        {
            get { return _appliedFilters; }
            set { Set(ref _appliedFilters, value); }
        }

        private ObservableCollection<Mentionable> _searchedMentionables;

        public ObservableCollection<Mentionable> SearchedMentionables
        {
            get { return _searchedMentionables; }
            set { Set(ref _searchedMentionables, value); }
        }

        public IEnumerable<string> EmojiCategories { get => Enum.GetNames(typeof(EmojiCategory)).Where(c => c != "None"); }

        public ICommand AttachFileCommand { get => new AttachFileCommand(ParentViewModel); }

        public ICommand SendMessageCommand { get => new SendMessageCommand(ParentViewModel); }

        public ICommand ResetEmojisCommand { get => new RelayCommand(ResetEmojiFilter); }

        public SendMessageControlViewModel(ChatViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            EmojiPicker = new EmojiPicker();
            Emojis = new ObservableCollection<Emoji>();
            SearchedMentionables = new ObservableCollection<Mentionable>();
            AppliedFilters = new ObservableCollection<EmojiCategory>();
        }

        public async void SearchMentionables(string newText)
        {
            int lastMentionSymbolIndex = newText.LastIndexOf('@');
            string searchTerm = newText.Substring(lastMentionSymbolIndex + 1);
            char[] categories = new char[] { 'r', 'u', 'c', 'm' };

            SearchedMentionables.Clear();

            if (searchTerm.Length > 2
                && categories.Any(c => c == searchTerm[0])
                && searchTerm[1] == ':')
            {
                foreach (Mentionable mentionable in await MentionService.SearchMentionable(searchTerm, App.StateProvider.SelectedTeam.Id))
                {
                    SearchedMentionables.Add(mentionable);
                }
            }
        }

        public void AddEmojiFilter(EmojiCategory category)
        {
            if (Emojis == null) Emojis = new ObservableCollection<Emoji>();

            EmojiPicker.AddFilter(category);
            EmojiPicker.FilterCategories();

            AppliedFilters.Add(category);

            Emojis.Clear();

            foreach (Emoji emoji in EmojiPicker.emojis)
            {
                Emojis.Add(emoji);
            }
        }

        public void RemoveEmojiFilter(EmojiCategory category)
        {
            EmojiPicker.RemoveFilter(category);

            AppliedFilters.Remove(category);

            if (AppliedFilters.Count == 0)
            {
                EmojiPicker.ResetFilters();
                Emojis.Clear();
                return;
            }

            EmojiPicker.FilterCategories();
            Emojis.Clear();

            foreach (Emoji emoji in EmojiPicker.emojis)
            {
                Emojis.Add(emoji);
            }
        }

        public void ResetEmojiFilter()
        {
            EmojiPicker.ResetFilters();
            EmojiPicker.FilterCategories();

            AppliedFilters.Clear();
            Emojis.Clear();
        }

        public void SearchEmojis(string name)
        {
            EmojiPicker.Rank(name);

            Emojis.Clear();

            foreach (Emoji emoji in EmojiPicker.emojis)
            {
                Emojis.Add(emoji);
            }
        }
    }
}
