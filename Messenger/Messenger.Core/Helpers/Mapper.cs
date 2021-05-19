using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Messenger.Core.Services;

namespace Messenger.Core.Helpers
{
    public class Mapper
    {
        /// <summary>
        /// Maps to a full user model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped user object</returns>
        public static User UserFromDataRow(DataRow row)
        {
            return new User()
            {
                Id = row["UserId"].ToString(),
                DisplayName = row["UserName"].ToString(),
                NameId = Convert.ToInt32(row["NameId"]),
                Mail = row["Email"].ToString(),
                Bio = row["Bio"].ToString(),
            };
        }

        /// <summary>
        /// Maps to a user model with minimal information from the MS-Graph service
        /// </summary>
        /// <param name="userdata">User object from the MS-Graph service</param>
        /// <returns>An user object with the information from MS-Graph</returns>
        public static User UserFromMSGraph(User userdata)
        {
            return new User()
            {
                Id = userdata.Id,
                DisplayName = userdata.DisplayName,
                NameId = userdata.NameId,
                Mail = userdata.Mail
            };
        }

        /// <summary>
        /// Maps to a full team model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped team object</returns>
        public static Team TeamFromDataRow(DataRow row)
        {
            return new Team()
            {
                Id = Convert.ToInt32(row["TeamId"]),
                Name = row["TeamName"].ToString(),
                Description = row["TeamDescription"].ToString(),
                CreationDate = Convert.ToDateTime(row["CreationDate"].ToString())
            };
        }

        public static Message MessageFromDataRow(DataRow row, SqlConnection connection)
        {
                return new Message{
                    Id = Convert.ToInt32(row["MessageId"]),
                    SenderId = row["SenderId"].ToString(),
                    Content = row["Message"].ToString(),
                    CreationTime = Convert.ToDateTime(row["CreationDate"].ToString()),
                    TeamId = Convert.ToInt32(row["TeamId"]),
                    ParentMessageId = Convert.ToInt32(row["ParentMessageId"]),
                };
        }
    }
}
