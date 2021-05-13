using System.Configuration;
using System.Data.SqlClient;

namespace Messenger.Core.Services
{
    public abstract class AzureServiceBase
    {
        private string connectionString => ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        protected SqlConnection GetConnection() => new SqlConnection(connectionString);
    }
}
