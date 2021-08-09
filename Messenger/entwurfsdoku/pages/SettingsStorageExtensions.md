#Benutzte Pakete
System.IO
System.Threading.Tasks
Messenger.Core.Helpers
Windows.Storage
Windows.Storage.Streams
#Importschnittstellen
ApplicationDataContainer.SaveString(string, string)
Messenger.Core.Helpers.Json.StringifyAsync(object)
Messenger.Core.Helpers.Json.ToObjectAsync<T>(string)
Messenger.Helpers.SettingsStorageExtensions.GetFileName(string)
string.Concat(string?, string?)
string.IsNullOrEmpty(string?)
System.IO.Path.Combine(string, string)
#Exportschnittstellen
public static bool IsRoamingStorageAvailable(this ApplicationData appData)
public static async Task<T> ReadAsync<T>(this StorageFolder folder, string name)
public static async Task<T> ReadAsync<T>(this ApplicationDataContainer settings, string key)
public static async Task<byte[]> ReadBytesAsync(this StorageFile file)
public static async Task<byte[]> ReadFileAsync(this StorageFolder folder, string fileName)
public static async Task SaveAsync<T>(this StorageFolder folder, string name, T content)
public static async Task SaveAsync<T>(this ApplicationDataContainer settings, string key, T value)
public static async Task<StorageFile> SaveFileAsync(this StorageFolder folder, byte[] content, string fileName, CreationCollisionOption options = CreationCollisionOption.ReplaceExisting)
public static void SaveString(this ApplicationDataContainer settings, string key, string value)
