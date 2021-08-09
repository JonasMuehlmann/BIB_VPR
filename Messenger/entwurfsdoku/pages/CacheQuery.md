#Benutzte Pakete
Messenger.Helpers.MessageHelpers
Messenger.Helpers.TeamHelpers
Messenger.ViewModels.DataViewModels
System
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
#Exportschnittstellen
public static async Task<T> AddOrUpdate<T>(params object[] parameters)
public static IReadOnlyCollection<PrivateChatViewModel> GetMyChats() => App.StateProvider.TeamManager.MyChats;
public static IReadOnlyCollection<TeamViewModel> GetMyTeams() => App.StateProvider.TeamManager.MyTeams;
public static bool IsChannelOf<T>(ChannelViewModel viewModel)
public static bool IsTypeOf<T>(Type type)
public static T Remove<T>(params object[] parameters)
public static bool TryGetMessages(uint channelId, out ObservableCollection<MessageViewModel> messages) => App.StateProvider.MessageManager.TryGetMessages(channelId, out messages);
