using Messenger.ConsoleMessenger.Programs;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Serilog.Context;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Messenger.ConsoleMessenger
{
    public class MessengerApp : IMessengerApp
    {
        #region Constants

        private const string CLIENT_ID = @"ca176355-2137-4346-838b-53a79d8ed8b4";

        #endregion

        #region Private

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private MicrosoftGraphService MicrosoftGraphService => Singleton<MicrosoftGraphService>.Instance;

        private UserService UserService => Singleton<UserService>.Instance;

        private readonly ILogger _log;

        private User _user { get; set; }

        #endregion

        public MessengerApp()
        {
            IdentityService.InitializeConsoleForAadMultipleOrgs(CLIENT_ID);

            // Force to initialize with the given connection string
            MessengerService.RegisterListenerForMessages(OnMessageReceived);

            _log = GlobalLogger.Instance;
        }

        /// <summary>
        /// Starts the application
        /// </summary>
        public void Run()
        {
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            _log.Fatal("Application starting");

            var isLoggedIn = Authenticate().GetAwaiter().GetResult();
            if (isLoggedIn)
            {
                OnLoginSuccess();

                _log.Fatal("Running the messenger program...\n");
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
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            _log.Debug("Authentication in progress...");
            var result = await IdentityService.LoginAsync();

            switch (result)
            {
                case LoginResultType.Success:
                    _log.Fatal("Login Success");
                    return true;
                case LoginResultType.CancelledByUser:
                    _log.Fatal("Authentication cancelled");
                    break;
                case LoginResultType.NoNetworkAvailable:
                    _log.Fatal("No network available");
                    break;
                default:
                    _log.Fatal("Login failed");
                    break;
            }

            return false;
        }

        #region Events

        /// <summary>
        /// Fires if the login was successful
        /// </summary>
        private void OnLoginSuccess()
        {
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            _log.Fatal("Authentication success! Starting the process to get the current user data...");
            var user = GetUserFromGraphApiAsync().GetAwaiter().GetResult();

            _log.Fatal("All done! Logged in as {username} ({mail})", user.DisplayName, user.Mail);
            _user = user;
        }

        /// <summary>
        /// Fires on "MessageReceived" in MessengerService
        /// </summary>
        /// <param name="sender">Service that triggered the event</param>
        /// <param name="message">Message object retrieved from the event</param>
        private void OnMessageReceived(object sender, Message message)
        {
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            _log.Fatal("Getting the sender information...");
            message.Sender = UserService.GetUser(message.SenderId).GetAwaiter().GetResult();

            bool isFromSelf = message.SenderId == _user.Id;

            if (isFromSelf)
            {
                // Hub received the message and broadcasted back
                _log.Fatal("Message was sent successfully! @ {0} as {1}",
                    message.CreationTime,
                    message.Sender.DisplayName);
            }
            else
            {
                // Message broadcasted from a member
                _log.Fatal("Message Received! {0} says: {1} @{2}",
                    message.Sender.DisplayName,
                    message.Content,
                    message.CreationTime);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the user information from Microsoft Graph Service and maps to the user model
        /// </summary>
        /// <returns>A complete user object</returns>
        private async Task<User> GetUserFromGraphApiAsync()
        {
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            _log.Fatal("Getting access token for the microsoft graph service...");
            var accessToken = await IdentityService.GetAccessTokenForGraphAsync();

            if (string.IsNullOrEmpty(accessToken))
            {
                _log.Fatal("Failed to acquire the token.");
                return null;
            }

            _log.Fatal("Getting user data from the microsoft graph service...");
            var userData = await MicrosoftGraphService.GetUserInfoAsync(accessToken);

            return await GetUserModelFromData(userData);
        }

        /// <summary>
        /// Gets the user data from the database and merges with the data from Microsoft Graph Service
        /// </summary>
        /// <param name="userData">User data from Microsoft Graph Service</param>
        /// <returns>A complete user object</returns>
        private async Task<User> GetUserModelFromData(User userData)
        {
            LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name);
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            if (userData == null)
            {
                _log.Fatal("Failed to fetch user data from the microsoft graph service.");
                return null;
            }

            _log.Fatal("Getting the user from the database...");
            var userFromDatabase = await UserService.GetOrCreateApplicationUser(userData);

            // Connect to signal-r hub
            // Optional parameter for connection string was given to initialize with it
            _log.Fatal("Connecting to signal-r hub with the current user data...");
            await MessengerService.Initialize(userData.Id);

            _log.Information($"Current user: {userData}");
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
