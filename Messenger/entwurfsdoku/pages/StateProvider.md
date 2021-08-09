#Benutzte Pakete
Messenger.Core.Services
Messenger.Helpers
Messenger.Helpers.MessageHelpers
Messenger.Helpers.TeamHelpers
Messenger.Models
Messenger.ViewModels.DataViewModels
System.Linq
System.Threading.Tasks
#Importschnittstellen
Messenger.Services.Providers.StateProvider.LoadAllMessages()
#Exportschnittstellen
public static async Task<StateProvider> Initialize(this StateProvider provider, UserViewModel user)
public static async Task LoadAllMessages(this StateProvider provider)
public StateProvider()
