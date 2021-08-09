#Benutzte Pakete
Windows.UI.Xaml.Controls
#Importschnittstellen
Messenger.ConsoleMessenger.Models
Messenger.Core.Helpers
Messenger.Core.Models
Messenger.Core.Services
Serilog
System
System.Collections.Generic
System.Linq
#Importschnittstellen
#Exportschnittstellen
public ChatPage()
Messenger.ConsoleMessenger.Programs.ChatPage.BuildTeamsList(string)
Messenger.ConsoleMessenger.Programs.ChatPage.LoadMessages(Messenger.Core.Models.Team)
Messenger.ConsoleMessenger.Programs.ChatPage.PrintMessage(Messenger.Core.Models.Message)
Messenger.ConsoleMessenger.Programs.ChatPage.SendMessage(uint)
Messenger.Core.Services.MessengerService.SendMessage(Messenger.Core.Models.Message, uint)
Serilog.ILogger.Information(string)
Serilog.ILogger.Information<string>(string, string)
Serilog.ILogger.Information<uint>(string, uint)
string.IsNullOrWhiteSpace(string?)
string.Join(char, params string?[])
#Exportschnittstellen
public ChatPage(Program program, string userId)
public override void Display()
