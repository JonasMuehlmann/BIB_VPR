using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
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
        private Team _selectedTeam;
        private ObservableCollection<Team> _teams;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion

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

        public Team SelectedTeam
        {
            get
            {
                return _selectedTeam;
            }
            set
            {
                Set(ref _selectedTeam, value);
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

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand CreateTeamCommand => _createTeamCommand ?? (_createTeamCommand = new RelayCommand(CreateTeamAsync));

        public TeamNavViewModel()
        {
            Teams = new ObservableCollection<Team>();
            UserDataService.UserDataUpdated += OnUserDataUpdated;
            ChatHubService.TeamsUpdated += OnTeamsUpdated;
            ChatHubService.TeamUpdated += OnTeamUpdated;
            //Loads the teams list when the user ist available
            ChatHubService.UserAvailable += OnUserDataUpdated;
        }

        /// <summary>
        /// Fires on UserDataUpdated(mostly once on log-in) and refreshes the view
        /// This prevents the view model to read from default user, which is not wanted here
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="user">UserViewModel of the current user</param>
        private void OnUserDataUpdated(object sender, UserViewModel user)
        {
            ClearAndAddTeamsList(user.Teams);
        }

        /// <summary>
        /// Fires on TeamsUpdated(e.g. CreateTeam, UpdateTeam, etc.) and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="teams">Enumerable of teams</param>
        private void OnTeamsUpdated(object sender, IEnumerable<Team> teams)
        {
            ClearAndAddTeamsList(teams);
        }

        /// <summary>
        /// Updates the refactored team in the list
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="team">The updated team</param>
        private void OnTeamUpdated(object sender, Team team)
        {
            for (int i = 0; i < Teams.Count; i++) {
                if (Teams[i].Id == team.Id) {
                    Teams[i] = team;
                }
            }
        }

        /// <summary>
        /// Creates the team with the given name and description
        /// </summary>
        /// <param name="team">New team to be created with the name and description</param>
        public async void CreateTeamAsync()
        {
            if (ChatHubService.CurrentUser == null)
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
        }


        #region Helpers

        private void ClearAndAddTeamsList(IEnumerable<Team> teams)
        {
            Teams.Clear();

            foreach (var team in teams)
            {
                Teams.Add(team);
            }
        }

        #endregion
    }
}
