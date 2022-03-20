using System.Collections.Generic;
using System.Linq;

namespace VueServer.Modules.Chat.Services.Hubs
{
    public class UserConnectionMapping
    {
        private readonly Dictionary<string, HashSet<string>> _connections =
            new Dictionary<string, HashSet<string>>();

        public int Count => _connections.Count;

        public void Add(string user, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(user, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(user, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(string user)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(user, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetConnections(IEnumerable<string> users)
        {
            HashSet<string> connections = new HashSet<string>();
            foreach (var user in users)
            {
                if (_connections.TryGetValue(user, out var newConns))
                {
                    foreach (var conn in newConns)
                    {
                        connections.Add(conn);
                    }
                }
            }

            return connections;
        }

        public void Remove(string user, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(user, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(user);
                    }
                }
            }
        }
    }
}