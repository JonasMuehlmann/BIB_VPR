using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using Serilog;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public abstract class AzureServiceBase
    {
        #region Private

        private string connectionString => ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        private bool testMode = false;

        private string testConnectionString;

        public ILogger logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
                .CreateLogger();

        #endregion

        public SqlConnection GetConnection() => testMode ? new SqlConnection(testConnectionString) : new SqlConnection(connectionString);

        public static SqlConnection GetConnection(string connectionString) => new SqlConnection(connectionString);

        public void SetTestMode(string connectionString)
        {
            testConnectionString = connectionString;
            testMode = true;
        }
    }
}
