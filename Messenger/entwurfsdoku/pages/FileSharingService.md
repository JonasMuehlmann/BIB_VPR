#Benutzte Pakete
Azure.Storage.Blobs
Messenger.Core.Helpers
System.IO
System.Threading.Tasks
System
Serilog
Serilog.Context
#Exportschnittstellen
public static async Task<bool> Delete(string blobFileName)
public static async Task<bool> Download(string blobFileName, string destinationDirectory = "")
public static async Task<string> Upload(string filePath)
public static async Task<string> UploadFromBase64(string data, string fileName)
