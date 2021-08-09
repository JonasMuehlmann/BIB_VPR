#Benutzte Pakete
Azure.Storage.Blobs
Messenger.Core.Helpers
System.IO
System.Threading.Tasks
System
Serilog
Serilog.Context
#Importschnittstellen
Messenger.Core.Services.FileSharingService.ConnectToContainer()
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
string.Substring(int, int)
System.Convert.FromBase64String(string)
System.Guid.NewGuid()
System.Guid.ToString()
System.IO.Path.Combine(string, string)
System.IO.Path.GetExtension(string?)
System.IO.Path.GetFileNameWithoutExtension(string?)
System.IO.Path.GetFullPath(string)
System.IO.Path.GetTempPath()
#Exportschnittstellen
public static async Task<bool> Delete(string blobFileName)
public static async Task<bool> Download(string blobFileName, string destinationDirectory = "")
public static async Task<string> Upload(string filePath)
public static async Task<string> UploadFromBase64(string data, string fileName)
