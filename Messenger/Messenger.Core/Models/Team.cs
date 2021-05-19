using System;
using System.Collections.Generic;
using System.Text;

namespace Messenger.Core.Models
{
    public class Team
    {
        public string Name { get; set; }

        public List<TeamChannel> Channels { get; set; }
    }
}
