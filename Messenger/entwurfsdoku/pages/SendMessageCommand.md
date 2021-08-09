#Benutzte Pakete
Messenger.Core.Models
System.Windows.Input
Serilog
Messenger.Core.Helpers
Messenger.Views.DialogBoxes
Messenger.Core.Services
Messenger.ViewModels.DataViewModels
Messenger.ViewModels.Pages
#Importschnittstellen
Messenger.Core.Services.MessengerService.SendMessage(Messenger.Core.Models.Message, uint)
Serilog.ILogger.Information(string)
string.IsNullOrEmpty(string?)
#Exportschnittstellen
public bool CanExecute(object parameter)
public async void Execute(object parameter)
public SendMessageCommand(ChatViewModel viewModel)
