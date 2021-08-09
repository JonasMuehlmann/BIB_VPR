#Benutzte Pakete
Messenger.Core.Services
Messenger.Helpers.TeamHelpers
Messenger.Models
Messenger.ViewModels
Messenger.ViewModels.DataViewModels
System
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
#Importschnittstellen
MessageViewModel.WithReactions()
MessageViewModel.WithSender()
Messenger.Core.Services.ChannelService.GetChannel(uint)
Messenger.Core.Services.MessengerService.GetMessages(uint)
Messenger.Core.Services.MessengerService.GetReactions(uint)
Messenger.Core.Services.UserService.GetUser(string)
Messenger.Helpers.MessageHelpers.MessageBuilder.Build(Messenger.Core.Models.Message)
Messenger.Helpers.MessageHelpers.MessageBuilder.Map(Messenger.Core.Models.Message)
Messenger.Helpers.MessageHelpers.MessageBuilder.Map(Messenger.Core.Models.User)
System.Array.GetLength(int)
System.Collections.Generic.List<Attachment>.Add(Attachment)
System.Collections.Generic.List<MessageViewModel>.Add(MessageViewModel)
System.Enum.Parse(System.Type, string)
#Exportschnittstellen
public static async Task<MessageViewModel> Build(this Message message)
public static async Task<IEnumerable<MessageViewModel>> Build(this IEnumerable<Message> messages)
public static async Task<IEnumerable<Message>> GetMessagesFromDatabase(ChannelViewModel channel)
public static MessageViewModel Map(Message message)
public static UserViewModel Map(User user)
public static List<Attachment> Parse(this IEnumerable<string> blobName)
public static IList<MessageViewModel> SortReplies(this IEnumerable<MessageViewModel> viewModels)
public static async Task<MessageViewModel> WithReactions(this MessageViewModel viewModel)
public static async Task<MessageViewModel> WithSender(this MessageViewModel viewModel)
