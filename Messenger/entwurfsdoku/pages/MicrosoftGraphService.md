#Benutzte Pakete
System.Net.Http
System.Net.Http.Headers
System.Threading.Tasks
Messenger.Core.Helpers
Messenger.Core.Models
#Importschnittstellen
Messenger.Core.Helpers.Json.ToObjectAsync<Messenger.Core.Models.User>(string)
Messenger.Core.Services.MicrosoftGraphService.GetDataAsync(string, string)
string.IsNullOrEmpty(string?)
#Exportschnittstellen
public static async Task<User> GetUserInfoAsync(string accessToken)
public static async Task<string> GetUserPhoto(string accessToken)
