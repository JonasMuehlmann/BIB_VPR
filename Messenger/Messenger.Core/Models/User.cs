using System.Collections.Generic;


namespace Messenger.Core.Models
{
    public class User
    {
        // From Microsoft
        public string Id { get; set; }

        public string DisplayName { get; set; }

        public uint NameId { get; set; }

        public string Photo { get; set; }

        public string Mail { get; set; }

        // From Database
        public string Bio { get; set; }

        public User()
        {
            Id = "";
            DisplayName = "";
            Photo = "";
            Mail = "";
            Bio = "";
        }

        public override string ToString()
        {
            return $"User: Id={Id}, DisplayName={DisplayName}, NameId={NameId}, Photo={Photo}, Mail={Mail}, Bio={Bio}";
        }

        // TODO: Cleanup
        public string Surname { get; set; }

        public string UserPrincipalName { get; set; }

        public List<string> Userlist { get; set; }

        public List<Team> Teams { get; set; }
    }
}
