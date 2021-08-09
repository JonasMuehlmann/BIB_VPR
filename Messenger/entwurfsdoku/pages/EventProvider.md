#Benutzte Pakete
Messenger.Core.Models
Messenger.Core.Services
Messenger.Helpers
Messenger.Models
Messenger.ViewModels.DataViewModels
System
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Text
System.Threading.Tasks
#Importschnittstellen
Messenger.Services.Providers.EventProvider.Broadcast(BroadcastOptions, BroadcastReasons, object)
Messenger.Services.Providers.EventProvider.SubscribeEvents()
Messenger.Services.Providers.EventProvider.UpdateSelectedTeam(TeamViewModel)
string.IsNullOrEmpty(string?)
System.EventHandler<BroadcastArgs>.Invoke(object?, BroadcastArgs)
#Exportschnittstellen
public void Broadcast(BroadcastOptions target, BroadcastReasons reason = BroadcastReasons.Loaded, object parameter = null)
public EventProvider()
