﻿using System.Collections.Generic;


namespace Messenger.Core.Models
{
    public class User
    {
        // From Microsoft
        public string Id { get; set; }

        public string DisplayName { get; set; }
        
        public string NameId { get; set; }

        public string Photo { get; set; }

        public string Mail { get; set; }

        // From Database
        public string Bio { get; set; }
    }
}
