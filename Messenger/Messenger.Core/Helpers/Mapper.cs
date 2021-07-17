using Messenger.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Serilog;
using Serilog.Context;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messenger.Core.Helpers
{
    public class Mapper
    {
        public static ILogger logger => GlobalLogger.Instance;
        /// <summary>
        /// Maps to a full user model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped user object</returns>
        public static User UserFromDataRow(DataRow row)
        {
            return new User()
            {
                Id          = SqlHelpers.TryConvertDbValue(row["UserId"], Convert.ToString),
                DisplayName = SqlHelpers.TryConvertDbValue(row["UserName"], Convert.ToString),
                NameId      = SqlHelpers.TryConvertDbValue(row["NameId"], Convert.ToUInt32),
                Mail        = SqlHelpers.TryConvertDbValue(row["Email"], Convert.ToString),
                Photo       = SqlHelpers.TryConvertDbValue(row["PhotoURL"], Convert.ToString),
                Bio         = SqlHelpers.TryConvertDbValue(row["Bio"], Convert.ToString),
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
                Id          = userdata.Id,
                DisplayName = userdata.DisplayName,
                NameId      = userdata.NameId,
                Mail        = userdata.Mail,
                Photo       = userdata.Photo,
                Bio         = userdata.Bio
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
                Id           = SqlHelpers.TryConvertDbValue(row["TeamId"],Convert.ToUInt32),
                Name         = SqlHelpers.TryConvertDbValue(row["TeamName"], Convert.ToString),
                Description  = SqlHelpers.TryConvertDbValue(row["TeamDescription"], Convert.ToString),
                CreationDate = SqlHelpers.TryConvertDbValue(row["CreationDate"].ToString(), Convert.ToDateTime)
            };
        }

        /// <summary>
        /// Maps to a full message model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped message object</returns>
        public static Message MessageFromDataRow(DataRow row)
        {
            var sender = new User()
            {
                Id              = SqlHelpers.TryConvertDbValue(row["UserId"], Convert.ToString),
                NameId          = SqlHelpers.TryConvertDbValue(row["NameId"], Convert.ToUInt32),
                DisplayName     = SqlHelpers.TryConvertDbValue(row["UserName"], Convert.ToString)
            };

            return new Message()
            {
                Id              = SqlHelpers.TryConvertDbValue(row["MessageId"], Convert.ToUInt32),
                SenderId        = SqlHelpers.TryConvertDbValue(row["SenderId"], Convert.ToString),
                RecipientId     = SqlHelpers.TryConvertDbValue(row["RecipientId"], Convert.ToUInt32),
                Content         = SqlHelpers.TryConvertDbValue(row["Message"], Convert.ToString),
                CreationTime    = SqlHelpers.TryConvertDbValue(row["CreationDate"].ToString(), Convert.ToDateTime),
                ParentMessageId = SqlHelpers.TryConvertDbValue(row["ParentMessageId"], Convert.ToUInt32),
                Sender          = sender
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
                MembershipId = SqlHelpers.TryConvertDbValue(row["MembershipId"], Convert.ToUInt32),
                UserId       = SqlHelpers.TryConvertDbValue(row["UserId"], Convert.ToString),
                UserRole     = SqlHelpers.TryConvertDbValue(row["UserRole"], Convert.ToString),
                TeamId       = SqlHelpers.TryConvertDbValue(row["TeamId"], Convert.ToUInt32)
            };
        }

        /// <summary>
        /// Maps to a full Channel model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully-mapped Channel object</returns>
        public static Channel ChannelFromDataRow(DataRow row)
        {
            return new Channel()
            {
                ChannelId   = SqlHelpers.TryConvertDbValue(row["ChannelId"], Convert.ToUInt32),
                ChannelName = SqlHelpers.TryConvertDbValue(row["ChannelName"], Convert.ToString),
                TeamId      = SqlHelpers.TryConvertDbValue(row["TeamId"], Convert.ToUInt32)
            };
        }
        /// <summary>
        /// Maps to a full Reaction model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully mapped Reaction object</returns>
        public static Reaction ReactionFromDataRow(DataRow row)
        {
            return new Reaction()
            {
                Id        = SqlHelpers.TryConvertDbValue(row["Id"], Convert.ToUInt32),
                MessageId = SqlHelpers.TryConvertDbValue(row["MessageId"], Convert.ToUInt32),
                UserId    = SqlHelpers.TryConvertDbValue(row["UserId"], Convert.ToString),
                Symbol    = SqlHelpers.TryConvertDbValue(row["Reaction"], Convert.ToString)
            };
        }

        /// <summary>
        /// Maps to a full Notification model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully mapped Notification object</returns>
        public static Notification NotificationFromDataRow(DataRow row)
        {
            return new Notification()
            {
                Id           = SqlHelpers.TryConvertDbValue(row["Id"], Convert.ToUInt32),
                RecipientId  = SqlHelpers.TryConvertDbValue(row["RecipientId"], Convert.ToString),
                CreationTime = SqlHelpers.TryConvertDbValue(row["CreationTime"], Convert.ToDateTime),
                Message      = SqlHelpers.TryConvertDbValue(row["Message"], strToJObject)
            };
        }

                /// Maps to a full TeamRole model from the data rows
        /// </summary>
        /// <param name="row">DataRow from the DataSet</param>
        /// <returns>A fully mapped TeamRole object</returns>
        public static TeamRole TeamRoleFromDataRow(DataRow row)
        {
            return new TeamRole()
            {
                Id     = SqlHelpers.TryConvertDbValue(row["Id"], Convert.ToUInt32),
                Role   = SqlHelpers.TryConvertDbValue(row["Role"], Convert.ToString),
                TeamId = SqlHelpers.TryConvertDbValue(row["TeamId"], Convert.ToUInt32)
            };
        }

        /// <summary>
        ///	Convert a DataRows column to a string
        /// </summary>
        /// <param name="row">the row to convert</param>
        /// <param name="columnName">The column of the row to convert</param>
        /// <returns>A string representation of the selected column</returns>
        public static string StringFromDataRow(DataRow row, string columnName)
        {
            return SqlHelpers.TryConvertDbValue(row[columnName], Convert.ToString);
        }

        /// <summary>
        /// Parse a json encoded string to a JObject
        /// </summary>
        /// <param name="str">The json encoded string to convert</param>
        /// <returns>A JObject representing the input</returns>
        public static JObject strToJObject(object str)
        {
            return JObject.Parse(str as string);
        }
    }
}
