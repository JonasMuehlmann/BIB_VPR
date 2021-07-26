using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.TeamManage;
using Messenger.Core.Helpers;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;

namespace Messenger.ViewModels
{
    public class TeamNavViewModel : Observable
    {
        #region Privates

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        private ObservableCollection<TeamViewModel> _teams;

        private bool _isBusy;

        #endregion

        #region Properties

        public ObservableCollection<TeamViewModel> Teams
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

        public UserViewModel CurrentUser { get; set; }

        #endregion

        #region Commands

        public ICommand TeamChannelSwitchCommand => new ChannelSwitchCommand();

        public ICommand CreateTeamCommand => new CreateTeamCommand();

        #endregion

        public TeamNavViewModel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            IsBusy = true;
            Teams = new ObservableCollection<TeamViewModel>();

            /** GET DATA FROM CACHE IF ALREADY INITIALIZED **/
            if (App.StateProvider != null)
            {
                Teams.Clear();

                foreach (TeamViewModel team in CacheQuery.GetMyTeams())
                {
                    Teams.Add(team);
                }

                IsBusy = false;
            }

            CurrentUser = await UserDataService.GetUserAsync();
        }

        public void OnTeamsLoaded(object sender, BroadcastArgs e)
        {
            IEnumerable<TeamViewModel> teams = e.Payload as IEnumerable<TeamViewModel>;

            if (teams != null && teams.Count() > 0)
            {
                Teams.Clear();

                foreach (TeamViewModel team in teams)
                {
                    Teams.Add(team);
                }
            }

            IsBusy = false;
        }

        public void OnTeamUpdated(object sender, BroadcastArgs e)
        {
            TeamViewModel team = e.Payload as TeamViewModel;

            if (team == null)
            {
                return;
            }

            if (e.Reason == BroadcastReasons.Created)
            {
                _teams.Add(team);
            }
            else if (e.Reason == BroadcastReasons.Updated)
            {
                TeamViewModel target = _teams.Single(t => t.Id == team.Id);
                int index = _teams.IndexOf(target);

                _teams[index] = team;
            }
            else if (e.Reason == BroadcastReasons.Deleted)
            {
                TeamViewModel target = _teams.Single(t => t.Id == team.Id);

                if (target != null)
                {
                    _teams.Remove(target);
                }
            }
        }

        public void OnChannelUpdated(object sender, BroadcastArgs e)
        {
            ChannelViewModel channel = e.Payload as ChannelViewModel;

            if (channel == null)
            {
                return;
            }

            if (e.Reason == BroadcastReasons.Created)
            {
                foreach (TeamViewModel team in _teams)
                {
                    if (team.Id == channel.TeamId)
                    {
                        team.Channels.Add(channel);
                        break;
                    }
                }
            }
            else if (e.Reason == BroadcastReasons.Updated)
            {
                foreach (TeamViewModel team in _teams)
                {
                    if (team.Id == channel.TeamId)
                    {
                        ChannelViewModel target = team.Channels.Single(ch => ch.ChannelId == channel.ChannelId);
                        int index = team.Channels.IndexOf(target);

                        team.Channels[index] = channel;
                    }
                }
            }
            else if (e.Reason == BroadcastReasons.Deleted)
            {
                foreach (TeamViewModel team in _teams)
                {
                    if (team.Id == channel.TeamId)
                    {
                        ChannelViewModel target = team.Channels.Single(ch => ch.ChannelId == channel.ChannelId);

                        team.Channels.Remove(target);
                    }
                }
            }
        }

        public void OnMessageUpdated(object sender, BroadcastArgs e)
        {
            if (e.Reason == BroadcastReasons.Created)
            {
                MessageViewModel message = e.Payload as MessageViewModel;

                foreach (TeamViewModel team in Teams)
                {
                    if (team.Channels.Any(c => c.ChannelId == message.ChannelId))
                    {
                        ChannelViewModel channel = team.Channels.Single(c => c.ChannelId == message.ChannelId);

                        channel.LastMessage = message;
                        break;
                    }
                }
            }
        }
    }
}
