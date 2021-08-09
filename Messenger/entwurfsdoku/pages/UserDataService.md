#Benutzte Pakete
System.Collections.Generic
System.Threading.Tasks
Messenger.Core.Helpers
Messenger.Core.Models
Messenger.Core.Services
Messenger.Helpers
Messenger.Models
Messenger.Services.Providers
Messenger.ViewModels.DataViewModels
Windows.Storage
#Importschnittstellen
Messenger.Core.Services.IdentityService.GetAccessTokenForGraphAsync()
Messenger.Core.Services.IdentityService.GetAccountUserName()
Messenger.Core.Services.MessengerService.Initialize(string, string)
Messenger.Core.Services.MicrosoftGraphService.GetUserInfoAsync(string)
Messenger.Core.Services.MicrosoftGraphService.GetUserPhoto(string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
Messenger.Services.UserDataService.GetDefaultUserData()
Messenger.Services.UserDataService.GetUserFromCacheAsync()
Messenger.Services.UserDataService.GetUserFromGraphApiAsync()
Messenger.Services.UserDataService.GetUserViewModelFromData(Messenger.Core.Models.User)
Messenger.Services.UserDataService.InitializeSignalR(string)
string.IsNullOrEmpty(string?)
System.EventHandler<UserViewModel>.Invoke(object?, UserViewModel)
#Exportschnittstellen
public async Task<UserViewModel> GetUserAsync()
public void Initialize()
public UserDataService()
