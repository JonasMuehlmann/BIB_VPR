using Messenger.ConsoleMessenger.Interfaces;
using Messenger.ConsoleMessenger.Programs;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Messenger.ConsoleMessenger.Services
{
    public class MessengerApp : IMessengerApp
    {
        #region Private

        private const string CONNECTION_STRING = @"Server=tcp:bib-vpr.database.windows.net,1433;Initial Catalog=vpr_messenger_database;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        
        private IdentityService IdentityService => Singleton<IdentityService>.Instance;
        
        private MicrosoftGraphService MicrosoftGraphService => Singleton<MicrosoftGraphService>.Instance;
        
        private UserService UserService => Singleton<UserService>.Instance;

        private readonly ILogger<MessengerApp> _log;

        #endregion

        public MessengerApp(ILogger<MessengerApp> log)
        {
            IdentityService.InitializeConsoleForAadMultipleOrgs("ca176355-2137-4346-838b-53a79d8ed8b4");
            UserService.SetTestMode(CONNECTION_STRING);
            
            _log = log;
        }

        public void Run()
        {
            using (LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name))
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                _log.LogInformation("Application starting");
            }

            Authenticate().GetAwaiter().GetResult();
        }

        private async Task Authenticate()
        {
            using (LogContext.PushProperty("Method", nameof(Authenticate)))
            {
                _log.LogInformation("Authentication in progress...");
                var result = await IdentityService.LoginAsync();

                switch (result)
                {
                    case LoginResultType.Success:
                        OnLoginSuccess();
                        break;
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
            }
        }

        private void OnLoginSuccess()
        {
            _log.LogInformation("Login Success");
            _log.LogInformation("Connecting to Signal-R...");
            
            var user = GetUserFromGraphApiAsync().GetAwaiter().GetResult();
            
            _log.LogInformation("All done! Logged in as {username} ({mail})", user.DisplayName, user.Mail);

            MessengerProgram
                .CreateProgram(user)
                .Run();
        }

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
    }
}
