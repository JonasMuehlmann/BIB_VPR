using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace Messenger.Services
{
    internal partial class ToastNotificationsService
    {
        public void ShowInitialization()
        {
            string tag = "initialization";

            var content = new ToastContentBuilder()
                .AddText("Initializing the application...")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = "Progress",
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .GetToastContent();

            // Generate the toast notification
            var toast = new ToastNotification(content.GetXml())
            {
                Tag = tag,
            };

            toast.Data = new NotificationData();
            toast.Data.Values["progressValue"] = "0";
            toast.Data.Values["progressValueString"] = $"0/2 Loaded";
            toast.Data.Values["progressStatus"] = "Loading Teams and Chats...";

            toast.Data.SequenceNumber = 1;

            ShowToastNotification(toast);
        }

        public void UpdateInitialization(uint sequence)
        {
            string tag = "initialization";

            var data = new NotificationData
            {
                SequenceNumber = sequence
            };

            data.Values["progressValue"] = $"{sequence - 1}";
            data.Values["progressValueString"] = $"{sequence - 1}/2 Loaded";

            if (sequence == 2)
            {
                data.Values["progressStatus"] = "Loading Messages...";
            }
            else if (sequence == 3)
            {
                data.Values["progressStatus"] = "Complete!";
            }

            ToastNotificationManager.CreateToastNotifier().Update(data, tag);
        }

        public void ShowNotificationLoggedIn(UserViewModel user)
        {
            var content = new ToastContentBuilder()
                .AddText($"Welcome back {user.Name}!")
                .AddText("Connection looks good :D")
                .AddAttributionText("Via Windows 10")
                .GetToastContent();

            var toast = new ToastNotification(content.GetXml())
            {
                Tag = "loggedIn"
            };

            ToastNotificationManager.History.Clear();
            ShowToastNotification(toast);
        }

        public void ShowMessageReceived(TeamViewModel team, MessageViewModel message)
        {
            var builder = new ToastContentBuilder();

            if (team is PrivateChatViewModel)
            {
                builder
                    .AddText($"{message.Sender.Name}")
                    .AddText($"{message.Content}")
                    .AddCustomTimeStamp(message.CreationTime);
            }
            else
            {
                builder
                    .AddText($"{team.TeamName}")
                    .AddText($"{message.Sender.Name}")
                    .AddText($"{message.Content}")
                    .AddCustomTimeStamp(message.CreationTime);
            }

            var toast = new ToastNotification(builder.GetToastContent().GetXml())
            {
                Tag = "messageReceived"
            };

            ShowToastNotification(toast);
        }

        public void ShowInvitationReceived(TeamViewModel team)
        {
            var builder = new ToastContentBuilder();

            if (team is PrivateChatViewModel)
            {
                ToastButton navigateChat = new ToastButton("Start Chat", $"NavigateChats");

                builder
                    .AddText($"{(team as PrivateChatViewModel).Partner.Name} has started chat with you!")
                    .AddText("Check out what's up!")
                    .AddButton(navigateChat);
            }
            else
            {
                ToastButton navigateTeam = new ToastButton("Say Hello", "NavigateTeams");

                navigateTeam.ActivationType = ToastActivationType.Foreground;
                
                builder
                    .AddText($"You are invited to {team.TeamName}.")
                    .AddText("Say hello to the members!")
                    .AddButton(navigateTeam);
            }

            var toast = new ToastNotification(builder.GetToastContent().GetXml())
            {
                Tag = "invitationReceived"
            };

            ShowToastNotification(toast);
        }
    }
}
