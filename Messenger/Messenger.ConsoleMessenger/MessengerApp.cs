using Messenger.ConsoleMessenger.Programs;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Messenger.ConsoleMessenger
{
    public class MessengerApp : IMessengerApp
    {
        #region Constants

        private const string CONNECTION_STRING = @"Server=tcp:bib-vpr.database.windows.net,1433;Initial Catalog=vpr_messenger_database;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private const string CLIENT_ID = @"ca176355-2137-4346-838b-53a79d8ed8b4";

        #endregion

        #region Private

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        
        private IdentityService IdentityService => Singleton<IdentityService>.Instance;
        
        private MicrosoftGraphService MicrosoftGraphService => Singleton<MicrosoftGraphService>.Instance;
        
        private UserService UserService => Singleton<UserService>.Instance;

        private readonly ILogger<MessengerApp> _log;
        
        private User _user { get; set; }

        #endregion

        public MessengerApp(ILogger<MessengerApp> log)
        {
            IdentityService.InitializeConsoleForAadMultipleOrgs(CLIENT_ID);
            
            // Force to initialize with the given connection string
            UserService.SetTestMode(CONNECTION_STRING);

            MessengerService.RegisterListenerForMessages(OnMessageReceived);

            _log = log;
        }

        /// <summary>
        /// Starts the application
        /// </summary>
        public void Run()
        {
            using (LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                _log.LogInformation("Application starting");
            }

            var isLoggedIn = Authenticate().GetAwaiter().GetResult();
            if (isLoggedIn)
            {
                OnLoginSuccess();

                MessengerProgram
                    .CreateProgram(_user)
                    .Run();
            }
        }

        /// <summary>
        /// Starts the authentication
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Authenticate()
        {
            using (LogContext.PushProperty("Method", nameof(Authenticate)))
            {
                _log.LogInformation("Authentication in progress...");
                var result = await IdentityService.LoginAsync();

                switch (result)
                {
                    case LoginResultType.Success:
                        return true;
                    case LoginResultType.CancelledByUser:
                        _log.LogError("Authentication cancelled");
                        break;
                    case LoginResultType.NoNetworkAvailable:
                        _log.LogError("No network available");
                        break;
                    default:
                        _log.LogError("Login failed");
                        break;
                }

                return false;
            }
        }

        #region Events

        /// <summary>
        /// Fires if the login was successful
        /// </summary>
        private void OnLoginSuccess()
        {
            using (LogContext.PushProperty("Method", nameof(OnLoginSuccess)))
            {
                _log.LogInformation("Login Success");
                _log.LogInformation("Connecting to Signal-R...");

                var user = GetUserFromGraphApiAsync().GetAwaiter().GetResult();

                _user = user;

                _log.LogInformation("All done! Logged in as {username} ({mail})", user.DisplayName, user.Mail);
                _log.LogInformation("Running the messenger program...\n");
            }
        }

        /// <summary>
        /// Fires on "MessageReceived" in MessengerService
        /// </summary>
        /// <param name="sender">Service that triggered the event</param>
        /// <param name="message">Message object retrieved from the event</param>
        private void OnMessageReceived(object sender, Message message)
        {
            using (LogContext.PushProperty("Method", nameof(OnMessageReceived)))
            {
                message.Sender = UserService.GetUser(message.SenderId).GetAwaiter().GetResult();
            
                bool isFromSelf = message.SenderId == _user.Id;

                if (isFromSelf)
                {
                    // Hub received the message and broadcasted back
                    _log.LogInformation("Message was sent successfully! @ {0} as {1}",
                        message.CreationTime,
                        message.Sender.DisplayName);
                }
                else
                {
                    // Message broadcasted from a member
                    _log.LogInformation("Message Received! {0} says: {1} @{2}",
                        message.Sender.DisplayName,
                        message.Content,
                        message.CreationTime);
                }
            }
        }

        #endregion

        #region Helpers

        private async Task<User> GetUserFromGraphApiAsync()
        {
            var accessToken = await IdentityService.GetAccessTokenForGraphAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var userData = await MicrosoftGraphService.GetUserInfoAsync(accessToken);

            return await GetUserModelFromData(userData);
        }

        private async Task<User> GetUserModelFromData(User userData)
        {
            if (userData == null)
            {
                return null;
            }

            var userFromDatabase = await UserService.GetOrCreateApplicationUser(userData);

            // Connect to signal-r hub
            // Optional parameter for connection string was given to initialize with it
            await MessengerService.Initialize(userData.Id, CONNECTION_STRING);

            // Merged with user model from the application database
            return new User()
            {
                Id = userData.Id,
                DisplayName = userFromDatabase.DisplayName,
                Bio = userFromDatabase.Bio,
                Mail = userFromDatabase.Mail
            };
        }

        #endregion
    }
}
