using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a Team
    /// </summary>
    public class Team
    {
        // TODO: IDK if this makes sense (here)
        public ObservableCollection<Channel> Channels { get; set; }

        /// <summary>
        /// The unique id of the team
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The name of the team
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the team
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The time at which the team was created
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The members of the team
        /// </summary>
        public List<User> Members { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null
        /// and List values get initialized instead of being null)
        /// </summary>
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

        // TODO: IDK if this makes sense (here)
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
