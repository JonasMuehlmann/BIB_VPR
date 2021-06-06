using EasyConsole;
using Messenger.ConsoleMessenger.Models;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Messenger.ConsoleMessenger.Programs
{
    /// <summary>
    /// MessengerProgram > HomePage > ChatPage
    /// Page to join teams and send messages
    /// </summary>
    class ChatPage : Page
    {
        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        private string _userId;

        IList<Team> Teams;

        public ChatPage(Program program, string userId)
            : base("Teams List", program)
        {
            _userId = userId;
        }

        public override void Display()
        {
            base.Display();

            // Shows all the teams user is in
            Teams = BuildTeamsList(_userId);
            foreach (var team in Teams)
            {
                Output.WriteLine(ConsoleColor.Green, "Team #{0,2} {1}", team.Id, team.Name);
            }

            // User selecting the team
            uint teamId = (uint)Input.ReadInt("Select a team: ", 1, 99);
            Output.WriteLine("\nEntering team {0}...", teamId);

            // User entering the team
            Team selectedTeam = Teams.Where(t => t.Id == teamId).FirstOrDefault();
            Output.WriteLine("You are now in Team '{0}'#{1}", selectedTeam.Name, selectedTeam.Id);

            // Shows all the messages in the selected team
            LoadMessages(selectedTeam);

            // Starts the action loop in the selected team
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
                            SendMessage(selectedTeam.Id);
                            break;
                        case ChatMenuOption.View:
                            LoadMessages(selectedTeam);
                            break;
                        case ChatMenuOption.Exit:
                        default:
                            loopOn = false;
                            Program.NavigateBack();
                            break;
                    }
                }

                loopCount++;
            }
        }

        /// <summary>
        /// Loads all messages of the team
        /// </summary>
        /// <param name="team">Team to load messages from</param>
        private void LoadMessages(Team team)
        {
            var messages = MessengerService.LoadMessages(team.Id).GetAwaiter().GetResult() ?? Enumerable.Empty<Message>();
            
            Output.WriteLine("");
            foreach (var message in messages)
            {
                PrintMessage(message);
            }
        }

        /// <summary>
        /// Sends a message to the team
        /// </summary>
        /// <param name="teamId">Id of the team to send the message to</param>
        private void SendMessage(uint teamId)
        {
            var content = string.Empty;

            while (string.IsNullOrWhiteSpace(content))
            {
                content = Input.ReadString($"[Enter] to send >> ");
            }

            var message = BuildMessage(content, _userId, teamId);

            MessengerService.SendMessage(message).GetAwaiter().GetResult();
            Input.ReadString(string.Empty);
        }

        /// <summary>
        /// Returns a complete message instance
        /// </summary>
        /// <param name="content">Content of the message</param>
        /// <param name="userId">Id of the user signed in</param>
        /// <param name="teamId">Id of the team the user is in</param>
        /// <returns>A complete message instance</returns>
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

        /// <summary>
        /// Returns a list of teams the user is in
        /// </summary>
        /// <param name="userId">Id of the user signed in</param>
        /// <returns>A list of teams</returns>
        private IList<Team> BuildTeamsList(string userId)
        {
            return MessengerService
                .LoadTeams(userId)
                .GetAwaiter()
                .GetResult()
                .ToList();
        }

        /// <summary>
        /// Outputs the given message to the console
        /// </summary>
        /// <param name="message">Message to print on console</param>
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
