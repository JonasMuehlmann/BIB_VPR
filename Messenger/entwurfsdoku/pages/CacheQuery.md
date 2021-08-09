#Benutzte Pakete
Messenger.Helpers.MessageHelpers
Messenger.Helpers.TeamHelpers
Messenger.ViewModels.DataViewModels
System
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
#Importschnittstellen
Messenger.Helpers.CacheQuery.GetMyChats()
Messenger.Helpers.CacheQuery.GetMyTeams()
Messenger.Helpers.CacheQuery.IsTypeOf<ChannelViewModel>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<MemberViewModel>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<MessageViewModel>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<PrivateChatViewModel>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<System.Collections.Generic.IEnumerable<ChannelViewModel>>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<System.Collections.Generic.IEnumerable<MemberViewModel>>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<System.Collections.Generic.IEnumerable<MessageViewModel>>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<System.Collections.Generic.IEnumerable<PrivateChatViewModel>>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<System.Collections.Generic.IEnumerable<TeamViewModel>>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<TeamRoleViewModel>(System.Type)
Messenger.Helpers.CacheQuery.IsTypeOf<TeamViewModel>(System.Type)
System.Convert.ChangeType(object?, System.Type)
#Exportschnittstellen
public static async Task<T> AddOrUpdate<T>(params object[] parameters)
public static IReadOnlyCollection<PrivateChatViewModel> GetMyChats() => App.StateProvider.TeamManager.MyChats;
public static IReadOnlyCollection<TeamViewModel> GetMyTeams() => App.StateProvider.TeamManager.MyTeams;
public static bool IsChannelOf<T>(ChannelViewModel viewModel)
public static bool IsTypeOf<T>(Type type)
public static T Remove<T>(params object[] parameters)
public static bool TryGetMessages(uint channelId, out ObservableCollection<MessageViewModel> messages) => App.StateProvider.MessageManager.TryGetMessages(channelId, out messages);
