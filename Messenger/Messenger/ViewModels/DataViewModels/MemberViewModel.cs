using Messenger.Core.Models;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class MemberViewModel : UserViewModel
    {
        private List<TeamRoleViewModel> _memberRoles = new List<TeamRoleViewModel>();

        private List<TeamRoleViewModel> _assignableMemberRoles;

        private uint _teamId;

        public List<TeamRoleViewModel> MemberRoles
        {
            get { return _memberRoles; }
            set { Set(ref _memberRoles, value); }
        }

        public uint TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }

        public List<TeamRoleViewModel> AssignableMemberRoles
        {
            get { return _assignableMemberRoles; }
            set { Set(ref _assignableMemberRoles, value); }
        }

        public MemberViewModel()
        {

        }

        public MemberViewModel(MemberViewModel data)
        {
            Id = data.Id;
            Name = data.Name;
            NameId = data.NameId;
            Bio = data.Bio;
            Mail = data.Mail;
            Photo = data.Photo;
            TeamId = data.TeamId;
            MemberRoles = data.MemberRoles;
            AssignableMemberRoles = data.AssignableMemberRoles;
        }
    }
}
