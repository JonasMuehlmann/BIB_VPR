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
                NameId = Convert.ToUInt32(row["NameId"]),
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
                Id = Convert.ToUInt32(row["TeamId"]),
                Name = row["TeamName"].ToString(),
                Description = row["TeamDescription"].ToString(),
                CreationDate = Convert.ToDateTime(row["CreationDate"].ToString())
            };
        }

        /// <summary>
        /// Maps to a full message model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped message object</returns>
        public static Message MessageFromDataRow(DataRow row)
        {
            return new Message() 
            {
                Id = Convert.ToUInt32(row["MessageId"]),
                SenderId = row["SenderId"].ToString(),
                RecipientId = Convert.ToUInt32(row["TeamId"]),
                Content = row["Message"].ToString(),
                CreationTime = Convert.ToDateTime(row["CreationDate"].ToString()),
                ParentMessageId = Convert.ToUInt32(row["ParentMessageId"])
            };
        }

        /// <summary>
        /// Maps to a full membership model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped membership object</returns>
        public static Membership MembershipFromDataRow(DataRow row)
        {
            return new Membership()
            {
                MembershipId = Convert.ToUInt32(row["MembershipId"]),
                UserId = row["UserId"].ToString(),
                UserRole = row["UserRole"].ToString(),
                TeamId = Convert.ToUInt32(row["TeamId"])
            };
        }
    }
}
