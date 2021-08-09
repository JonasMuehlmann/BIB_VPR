#Benutzte Pakete
System.Collections.Generic
System.Linq
System.Threading.Tasks
Messenger.Activation
Messenger.Core.Helpers
Messenger.Core.Services
Messenger.Services
Windows.ApplicationModel.Activation
Windows.Foundation
Windows.UI.ViewManagement
Windows.UI.Xaml
Windows.UI.Xaml.Controls
#Importschnittstellen
Messenger.Core.Services.IdentityService.AcquireTokenSilentAsync()
Messenger.Core.Services.IdentityService.InitializeWithAadAndPersonalMsAccounts()
Messenger.Core.Services.IdentityService.IsAuthorized()
Messenger.Core.Services.IdentityService.IsLoggedIn()
Messenger.Services.ActivationService.GetActivationHandlers()
Messenger.Services.ActivationService.HandleActivationAsync(object)
Messenger.Services.ActivationService.InitializeAsync()
Messenger.Services.ActivationService.IsInteractive(object)
Messenger.Services.ActivationService.RedirectLoginPageAsync()
Messenger.Services.ActivationService.StartupAsync()
#Exportschnittstellen
public async Task ActivateAsync(object activationArgs)
public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
public async Task RedirectLoginPageAsync()
public void SetShell(Lazy<UIElement> shell)
