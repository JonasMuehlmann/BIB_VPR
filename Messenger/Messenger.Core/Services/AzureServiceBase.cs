using System.Configuration;
using System.Data.SqlClient;

namespace Messenger.Core.Services
{
    public abstract class AzureServiceBase
    {
        #region Private

        private string connectionString => ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        private bool testMode = false;

        private string testConnectionString;

        #endregion

        protected SqlConnection GetConnection() => testMode ? new SqlConnection(testConnectionString) : new SqlConnection(connectionString);

        public void SetTestMode(string connectionString)
        {
            testConnectionString = connectionString;
            testMode = true;
        }
    }
}
