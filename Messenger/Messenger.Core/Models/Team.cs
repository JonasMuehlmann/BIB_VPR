using System;
using System.Collections.Generic;


namespace Messenger.Core.Models
{
    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreationDate { get; set; }

        public List<User> Members { get; set; }

        public override string ToString()
        {
            return $"Team: Id={Id}, Name={Name}, Description={Description}, CreationDate={CreationDate.ToString()}, Members=[{string.Join(",", Members)}]";
        }
    }
}
