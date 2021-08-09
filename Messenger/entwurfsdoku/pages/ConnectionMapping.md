#Benutzte Pakete
System.Linq
Serilog.Context
Serilog
Messenger.Core.Helpers
#Exportschnittstellen
public void Add(string userId, string connectionId)
public IEnumerable<string> GetConnections(string userId)
public void Remove(string userId, string connectionId)
