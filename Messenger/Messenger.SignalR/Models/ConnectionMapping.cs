using System.Collections.Generic;
using System.Linq;
using Serilog.Context;
using Serilog;
using Messenger.Core.Helpers;

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

        public ILogger logger = GlobalLogger.Instance;

        /// <summary>
        /// Adds a new connection id under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <param name="connectionId">Current connection id of the application</param>
        public void Add(string userId, string connectionId)
        {
            LogContext.PushProperty("Method","Add");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, connectionId={connectionId}");

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

            logger.Information($"Established new connection with connectionId={connectionId} for user identified by userId={userId}");
        }

        /// <summary>
        /// Gets all connection ids registered under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <returns>List of connection ids, empty enumerable if none exists</returns>
        public IEnumerable<string> GetConnections(string userId)
        {
            LogContext.PushProperty("Method","GetConnections");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter userId={userId}");

            HashSet<string> connections;
            if (_connections.TryGetValue(userId, out connections))
            {
                logger.Information("Returning established connections of the user identified by userId={userId}");
                return connections;
            }

            logger.Information("No established connections exist for the user identified by userId={userId}");
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Safely removes the connection id from the registered list under the user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <param name="connectionId">Connection id to remove from the list</param>
        public void Remove(string userId, string connectionId)
        {
            LogContext.PushProperty("Method","Remove");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, connectionId={connectionId}");

            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(userId, out connections))
                {
                    logger.Information($"No established connections exist for the user identified by userId={userId}");
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
            logger.Information($"Closed the session with connectionId={connectionId} of the user identified by userId={userId}");
        }
    }
}
