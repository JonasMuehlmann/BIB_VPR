using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Messenger.Core.Models
{
    public class Team
    {
        public ObservableCollection<Channel> Channels { get; set; }
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
            Channels = new ObservableCollection<Channel>();
        }

        public override string ToString()
        {
            return $"Team: Id={Id}, Name={Name}, Description={Description}, CreationDate={CreationDate.ToString()}, Members=[{string.Join(",", Members)}]";
        }

        #region Helpers
        /// <summary>
        /// refactors the Channels list
        /// </summary>
        /// <param name="channels"></param>
        public void FilterAndUpdateChannels(IEnumerable<Channel> channels)
        {
            if (channels != null)
            {
                Channels.Clear();

                foreach (var channel in channels)
                {
                    Channels.Add(channel);
                }
            }
        }
        #endregion
    }
}
