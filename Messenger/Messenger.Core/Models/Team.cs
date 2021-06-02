using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger.Core.Models
{
    public class Team
    {
        public List<TeamChannel> Channels { get; set; }

        public uint Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreationDate { get; set; }

        public List<User> Members { get; set; }

        public Team()
        {
            Name = "";
            Description = "";
            Members = new List<User>();
        }

        public override string ToString()
        {
            return $"Team: Id={Id}, Name={Name}, Description={Description}, CreationDate={CreationDate.ToString()}, Members=[{string.Join(",", Members)}]";
        }
    }
}
