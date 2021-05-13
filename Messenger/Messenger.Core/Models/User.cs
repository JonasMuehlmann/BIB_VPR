using System.Collections.Generic;


namespace Messenger.Core.Models
{
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhotoURL { get; set; }

        public string Bio { get; set; }
    }
}