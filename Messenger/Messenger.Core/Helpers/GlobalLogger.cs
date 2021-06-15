using Serilog;
using System.IO;

namespace Messenger.Core.Helpers
{
    public static class GlobalLogger
    {
        private static ILogger _instance;

        public static ILogger Instance => _instance ?? 
            (_instance = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(Path.GetTempPath(), "BIB_VPR", "log.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .CreateLogger());
    }
}