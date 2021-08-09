#Benutzte Pakete
System.Linq
Serilog.Context
Serilog
Messenger.Core.Helpers
#Importschnittstellen
object.GetType()
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>.Add(string, System.Collections.Generic.HashSet<string>)
System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>.Remove(string)
System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>.TryGetValue(string, out System.Collections.Generic.HashSet<string>)
System.Collections.Generic.HashSet<string>.Add(string)
System.Collections.Generic.HashSet<string>.Remove(string)
#Exportschnittstellen
public void Add(string userId, string connectionId)
public IEnumerable<string> GetConnections(string userId)
public void Remove(string userId, string connectionId)
