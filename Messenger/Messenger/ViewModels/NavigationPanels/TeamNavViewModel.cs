using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class TeamNavViewModel : Observable
    {
        #region Privates

        private ShellViewModel _shellViewModel;
        private ICommand _itemInvokedCommand;
        private ICommand _createTeamCommand;
        private ObservableCollection<Team> _teams;
        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion
        private bool _isBusy;

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
            set
            {
                Set(ref _shellViewModel, value);
            }
        }

        public ObservableCollection<Team> Teams
        {
            get
            {
                return _teams;
            }
            set
            {
                Set(ref _teams, value);
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                Set(ref _isBusy, value);
            }
        }

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand CreateTeamCommand => _createTeamCommand ?? (_createTeamCommand = new RelayCommand(CreateTeamAsync));

        public TeamNavViewModel()
        {
            IsBusy = true;

            Teams = new ObservableCollection<Team>();

            ChatHubService.TeamsUpdated += OnTeamsUpdated;
            Initialize();
            ChatHubService.TeamUpdated += OnTeamUpdated;
        }

        /// <summary>
        //  Loads the teams list if the user data has already been loaded
        /// </summary>
        private void Initialize()
        {
            switch (ChatHubService.ConnectionState)
            {
                case ChatHubConnectionState.Loading:
                    IsBusy = true;
                    break;
                case ChatHubConnectionState.NoDataFound:
                    IsBusy = false;
                    break;
                case ChatHubConnectionState.LoadedWithData:
                    FilterAndUpdateTeams(ChatHubService.CurrentUser.Teams);
                    IsBusy = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Fires on TeamsUpdated(e.g. CreateTeam, UpdateTeam, etc.) and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="teams">Enumerable of teams</param>
        private void OnTeamsUpdated(object sender, IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                FilterAndUpdateTeams(teams);
            }

            IsBusy = false;
        }

        /// <summary>
        /// Updates the refactored team in the list
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="team">The updated teams</param>
        private async void OnTeamUpdated(object sender,Team team)
        {
            if (ChatHubService.CurrentUser.Teams != null)
            {
                FilterAndUpdateTeams(await ChatHubService.GetTeamsList());
            }

            IsBusy = false;
        }



        /// <summary>
        /// Creates the team with the given name and description
        /// </summary>
        /// <param name="team">New team to be created with the name and description</param>
        public async void CreateTeamAsync()
        {
            if (CurrentUser == null)
            {
                return;
            }

            // Opens the dialog box for the input
            var dialog = new CreateTeamDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            // Create team on confirm
            if (result == ContentDialogResult.Primary)
            {
                await ChatHubService.CreateTeam(dialog.TeamName, dialog.TeamDescription);

                await ResultConfirmationDialog
                    .Set(true, $"You created a new team {dialog.TeamName}")
                    .ShowAsync();
            }
        }

        /// <summary>
        /// Fires on click and invokes ChatHubService to load messages of the selected team
        /// </summary>
        /// <param name="args">Event argument from the event, contains the data of the invoked item</param>
        private async void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
        {
            uint teamId = (args.InvokedItem as Team).Id;

            // Invokes TeamSwitched event
            await ChatHubService.SwitchTeam(teamId);

            NavigationService.Open<ChatPage>();
        }


        #region Helpers

        private void FilterAndUpdateTeams(IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                IEnumerable<Team> teamsList = teams.Where(t => !string.IsNullOrEmpty(t.Name));

                Teams.Clear();
                
                foreach (var team in teamsList)
                {
                    Teams.Add(team);
                }
            }
        }

        #endregion
    }
}
