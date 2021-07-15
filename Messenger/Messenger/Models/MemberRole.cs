using Messenger.Core.Models;
using Messenger.Helpers;
using System.Collections.Generic;

namespace Messenger.Models
{
    public class MemberRole : Observable
    {
        private string _title;

        private IList<Permissions> _permissions;

        private uint _teamId;

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public IList<Permissions> Permissions
        {
            get { return _permissions; }
            set { Set(ref _permissions, value); }
        }

        public uint TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }

        public MemberRole()
        {
            Permissions = new List<Permissions>();
        }
    }
}
