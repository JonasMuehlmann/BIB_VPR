using System.Configuration;
using System.Data.SqlClient;
using Serilog;
using Messenger.Core.Helpers;

namespace Messenger.Core.Services
{
    public abstract class AzureServiceBase
    {
        #region Private

        #if BIB_VPR_DEBUG
        private static string connectionString = "Server=tcp:bib-vpr.database.windows.net,1433;Initial Catalog=vpr_messenger_database;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        #else
        // Testing connection string
        private static string connectionString = @"Server=tcp:bib-vpr.database.windows.net,1433;Initial Catalog=vpr_messenger_database_test;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        #endif

        public static ILogger logger => GlobalLogger.Instance;
        #endregion

        public static SqlConnection GetDefaultConnection() => new SqlConnection(connectionString);
    }
}
