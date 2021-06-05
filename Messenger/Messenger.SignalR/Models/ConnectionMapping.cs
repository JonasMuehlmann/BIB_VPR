using System.Collections.Generic;
using System.Linq;

namespace Messenger.SignalR.Models
{
    /// <summary>
    /// Thread-safe dictionary mapping class for connection ids
    /// </summary>
    public class ConnectionMapping
    {
        private readonly Dictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        /// <summary>
        /// Adds a new connection id under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <param name="connectionId">Current connection id of the application</param>
        public void Add(string userId, string connectionId)
        {
            // Locks other thread access to the shared resource
            // Any other thread waits until the lock is released
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(userId, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(userId, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        /// <summary>
        /// Gets all connection ids registered under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <returns>List of connection ids, empty enumerable if none exists</returns>
        public IEnumerable<string> GetConnections(string userId)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(userId, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Safely removes the connection id from the registered list under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <param name="connectionId">Connection id to remove from the list</param>
        public void Remove(string userId, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(userId, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(userId);
                    }
                }
            }
        }
    }
}
