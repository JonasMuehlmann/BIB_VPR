using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Messenger.ConsoleMessenger
{
    public class MessengerApp : IMessengerApp
    {
        private readonly ILogger<MessengerApp> _log;

        public MessengerApp(ILogger<MessengerApp> log)
        {
            _log = log;
        }

        public void Run()
        {
            _log.LogInformation("Application starting");
        }
    }
}
