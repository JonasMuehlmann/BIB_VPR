using EasyConsole;
using Messenger.ConsoleMessenger.Models;
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
    class ChatPage : Page
    {
        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        private UserService UserService => Singleton<UserService>.Instance;

        private string _userId;

        IList<Team> Teams;

        public ChatPage(Program program, string userId)
            : base("Teams List", program)
        {
            _userId = userId;

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
        }

        public override void Display()
        {
            base.Display();

            Teams = BuildTeamsList(_userId);

            foreach (var team in Teams)
            {
                Output.WriteLine(ConsoleColor.Green, "Team #{0,2} {1}", team.Id, team.Name);
            }

            uint teamId = (uint)Input.ReadInt("Select a team: ", 1, 99);

            Output.WriteLine("Entering team {0}...", teamId);

            Team selectedTeam = Teams.Where(t => t.Id == teamId).FirstOrDefault();

            Output.WriteLine("[ {0} ]", selectedTeam.Name);

            LoadMessages(selectedTeam);

            bool loopOn = true;
            uint loopCount = 0;
            while (loopOn)
            {
                if (loopCount > 0)
                {
                    var option = Input.ReadEnum<ChatMenuOption>("\nWhat should we do next?");
                    switch (option)
                    {
                        case ChatMenuOption.Send:
                            break;
                        case ChatMenuOption.Exit:
                        default:
                            loopOn = false;
                            Program.NavigateBack();
                            break;
                    }
                }

                var content = string.Empty;

                while (string.IsNullOrWhiteSpace(content))
                {
                    content = Input.ReadString($"([Enter] to send) >> ");
                }

                var message = BuildMessage(content, _userId, teamId);

                MessengerService.SendMessage(message).GetAwaiter().GetResult();

                Output.WriteLine("Message Sent! @{0}", message.CreationTime);
                Input.ReadString("Press [Enter] to continue");
                loopCount++;
            }
        }

        private void OnMessageReceived(object sender, Message message)
        {
            message.Sender = UserService.GetUser(message.SenderId).GetAwaiter().GetResult();

            Output.WriteLine("\nMessage Received!");
            PrintMessage(message);

            Input.ReadString("Press [Enter] to confirm");
        }

        private void LoadMessages(Team team)
        {
            var messages = MessengerService.LoadMessages(team.Id).GetAwaiter().GetResult() ?? Enumerable.Empty<Message>();
            
            foreach (var message in messages)
            {
                PrintMessage(message);
            }
        }

        private Message BuildMessage(string content, string userId, uint teamId)
        {
            return new Message()
            {
                Content = content,
                SenderId = userId,
                RecipientId = teamId,
                CreationTime = DateTime.Now
            };
        }

        private IList<Team> BuildTeamsList(string userId)
        {
            return MessengerService
                .LoadTeams(userId)
                .GetAwaiter()
                .GetResult()
                .ToList();
        }

        private void PrintMessage(Message message)
        {
            bool isFromSelf = message.SenderId == _userId;

            Output.WriteLine("[ {0} ]: {1} {2}",
                isFromSelf ? "You" : message.Sender.DisplayName,
                message.Content,
                message.CreationTime);
        }
    }
}
