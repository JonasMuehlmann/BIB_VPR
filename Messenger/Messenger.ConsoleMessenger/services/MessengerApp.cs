using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace Messenger.ConsoleMessenger
{
    public class MessengerApp : IMessengerApp
    {
        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        private readonly ILogger<MessengerApp> _log;

        public event Action<string> OnConnection;

        public MessengerApp(ILogger<MessengerApp> log)
        {
            _log = log;
        }

        public void Run()
        {
            using (LogContext.PushProperty("Method", System.Reflection.MethodBase.GetCurrentMethod().Name))
            {
                _log.LogInformation("Application starting");
            }

            MessengerService
                .Initialize(null, false)
                .ContinueWith((task) => 
                { 
                    var id = MessengerService.HubConnectionId;
                    _log.LogInformation("Hub connection established (Connection Id: {connectionId}", id);
                });
        }
    }
}
