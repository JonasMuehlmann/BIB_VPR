using Messenger.Core.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;

namespace Messenger.ViewModels.DataViewModels
{
    public class TeamRoleViewModel : DataViewModel
    {
        private string _title;

        private ObservableCollection<Permissions> _permissions;

        private uint _teamId;

        private Color _color;

        private uint _id;

        public uint Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public ObservableCollection<Permissions> Permissions
        {
            get { return _permissions; }
            set { Set(ref _permissions, value); }
        }

        public uint TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }

        public Color Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        public TeamRoleViewModel()
        {
            Permissions = new ObservableCollection<Permissions>();
        }

        public TeamRoleViewModel(TeamRoleViewModel data)
        {
            Id = data.Id;
            Title = data.Title;
            Permissions = data.Permissions;
            TeamId = data.TeamId;
            Color = data.Color;
        }
    }
}
