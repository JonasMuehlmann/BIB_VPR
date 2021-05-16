using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Messenger.Core.Helpers
{
    public class Mapper
    {
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
    }
}
