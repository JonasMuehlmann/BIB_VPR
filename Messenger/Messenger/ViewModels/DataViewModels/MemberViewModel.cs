using Messenger.Core.Models;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class MemberViewModel : UserViewModel
    {
        private List<MemberRole> _memberRoles = new List<MemberRole>();

        private uint _teamId;

        public List<MemberRole> MemberRoles
        {
            get { return _memberRoles; }
            set { Set(ref _memberRoles, value); }
        }

        public uint TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }
    }
}
