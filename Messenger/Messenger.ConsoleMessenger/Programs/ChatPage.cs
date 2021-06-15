﻿using EasyConsole;
using Messenger.ConsoleMessenger.Models;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Serilog;
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

        private ILogger _logger => GlobalLogger.Instance;

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
                Output.WriteLine(ConsoleColor.Green, $"Team #{team.Id, 2} {team.Name}");
            }

            // User selecting the team
            uint teamId = (uint)Input.ReadInt("Select a team: ", 1, 99);
            Output.WriteLine($"\nEntering team {teamId}...");

            // User entering the team
            Team selectedTeam = Teams.Where(t => t.Id == teamId).FirstOrDefault();
            Output.WriteLine($"You are now in Team '{selectedTeam.Name}'#{selectedTeam.Id}");

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
            _logger.Information("Loading messages with the current team id {0}", team.Id);
            var messages = MessengerService.LoadMessages(team.Id).GetAwaiter().GetResult() ?? Enumerable.Empty<Message>();

            _logger.Information("Total of {0} messages were loaded.", messages.Count());
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

            var message = new Message()
            {
                Content = content,
                SenderId = _userId,
                RecipientId = teamId,
                CreationTime = DateTime.Now
            };

            MessengerService.SendMessage(message).GetAwaiter().GetResult();
            Input.ReadString(string.Empty);
        }

        /// <summary>
        /// Returns a list of teams the user is in
        /// </summary>
        /// <param name="userId">Id of the user signed in</param>
        /// <returns>A list of teams</returns>
        private IList<Team> BuildTeamsList(string userId)
        {
            var teams = MessengerService
                        .LoadTeams(userId)
                        .GetAwaiter()
                        .GetResult()
                        .ToList();

            _logger.Information("Following teams are loaded: {0}", string.Join(", ", teams.Select(t => t.Name)));

            return teams;
        }

        /// <summary>
        /// Outputs the given message to the console
        /// </summary>
        /// <param name="message">Message to print on console</param>
        private void PrintMessage(Message message)
        {
            bool isFromSelf = message.SenderId == _userId;
            string sender = isFromSelf ? "You" : message.Sender.DisplayName;

            Output.WriteLine($"[ {sender} ]: {message.Content} @{message.CreationTime}");
        }
    }
}