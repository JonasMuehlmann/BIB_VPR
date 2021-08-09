#Benutzte Pakete
Messenger.Models
Messenger.ViewModels.DataViewModels
System
System.Collections.Concurrent
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
#Importschnittstellen
Messenger.Helpers.MessageHelpers.MessageManager.AddOrUpdateMessage(Messenger.Core.Models.Message)
Messenger.Helpers.MessageHelpers.MessageManager.AddOrUpdateMessage(System.Collections.Generic.IEnumerable<Messenger.Core.Models.Message>)
Messenger.Helpers.MessageHelpers.MessageManager.AddOrUpdateToDictionary(MessageViewModel)
Messenger.Helpers.MessageHelpers.MessageManager.CreateEntry(TeamViewModel)
System.Collections.Generic.List<MessageViewModel>.Add(MessageViewModel)
System.EventHandler<ManagerEventArgs>.Invoke(object?, ManagerEventArgs)
#Exportschnittstellen
public async Task<MessageViewModel> AddOrUpdateMessage(Message messageData)
public async Task<IList<MessageViewModel>> AddOrUpdateMessage(IEnumerable<Message> messageData)
public async Task CreateEntry(TeamViewModel viewModel)
public async Task CreateEntry(IEnumerable<TeamViewModel> viewModels)
public MessageManager()
public ChannelViewModel RemoveEntry(uint channelId)
public MessageViewModel RemoveMessage(Message data)
public bool TryGetMessages(uint channelId, out ObservableCollection<MessageViewModel> messages)
