using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ConsoleMessenger.Programs
{
    class MessengerProgram : EasyConsole.Program
    {
        public User CurrentUser { get; set; }

        public MessengerProgram(User user)
            : base("Messenger Console App", breadcrumbHeader: false)
        {
            CurrentUser = user;

            AddPage(new HomePage(this));
            AddPage(new ChatPage(this, user.Id));
            
            SetPage<HomePage>();
        }

        public static MessengerProgram CreateProgram(User user)
        {
            return new MessengerProgram(user);
        }
    }
}
