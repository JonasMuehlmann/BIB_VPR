using Serilog;

namespace Messenger.Core.Helpers
{
    public static class GlobalLogger
    {
        private static ILogger _instance;

        public static ILogger Instance => _instance ?? 
            (_instance = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
                .CreateLogger());
    }
}