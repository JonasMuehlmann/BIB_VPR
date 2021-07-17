using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Models
{
    public class Member : UserViewModel
    {
        private List<MemberRole> _memberRoles = new List<MemberRole>();

        public List<MemberRole> MemberRoles
        {
            get { return _memberRoles; }
            set { Set(ref _memberRoles, value); }
        }
    }
}
