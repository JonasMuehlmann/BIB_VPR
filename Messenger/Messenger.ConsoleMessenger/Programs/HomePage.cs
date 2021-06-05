using EasyConsole;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Messenger.ConsoleMessenger.Programs
{
    class HomePage : MenuPage
    {
        public HomePage(Program program)
            : base("Teams", program,
                  new Option("Join Team", () => { program.NavigateTo<ChatPage>(); }))
        {
        }

        public override void Display()
        {
            base.Display();

            Input.ReadString("Press [Enter] to close the application.");
        }
    }
}
