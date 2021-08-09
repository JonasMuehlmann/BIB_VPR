#Benutzte Pakete
Newtonsoft.Json
#Importschnittstellen
System.Threading.Tasks.Task.Run<string>(System.Func<string>)
System.Threading.Tasks.Task.Run<T>(System.Func<T>)
#Exportschnittstellen
public static async Task<string> StringifyAsync(object value)
public static async Task<T> ToObjectAsync<T>(string value)
