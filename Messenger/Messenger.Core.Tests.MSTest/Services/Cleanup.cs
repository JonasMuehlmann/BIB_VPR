using Messenger.Core.Services;
using Messenger.Core.Helpers;
using System.Data.SqlClient;

namespace Messenger.Tests.MSTest
{
    public static class ServiceCleanup
    {
        public static void Cleanup()
        {
            // Reset DB
            string query = @"
                                DELETE FROM NotificationMutes;
                                DELETE FROM Notifications;
                                DELETE FROM Mentions;
                                DELETE FROM PinnedMessages;
                                DELETE FROM Reactions;
                                DELETE FROM Messages;
                                DELETE FROM Memberships;
                                DELETE FROM Channels;
                                DELETE FROM Role_permissions;
                                DELETE FROM User_roles;
                                DELETE FROM Team_roles;
                                DELETE FROM Teams;
                                DELETE FROM Users;

                                DBCC CHECKIDENT (NotificationMutes,    RESEED, 0);
                                DBCC CHECKIDENT (Notifications,        RESEED, 0);
                                DBCC CHECKIDENT (Mentions,             RESEED, 0);
                                DBCC CHECKIDENT (Memberships,          RESEED, 0);
                                DBCC CHECKIDENT (Messages,             RESEED, 0);
                                DBCC CHECKIDENT (Channels,             RESEED, 0);
                                DBCC CHECKIDENT (Team_roles,           RESEED, 0);
                                DBCC CHECKIDENT (User_roles,           RESEED, 0);
                                DBCC CHECKIDENT (Role_permissions,     RESEED, 0);
                                DBCC CHECKIDENT (Reactions,            RESEED, 0);
                                DBCC CHECKIDENT (Teams,                RESEED, 0);
                    ";

            using (SqlConnection connection = AzureServiceBase.GetDefaultConnection())
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
