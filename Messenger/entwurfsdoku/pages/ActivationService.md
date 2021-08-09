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
#Exportschnittstellen
public async Task ActivateAsync(object activationArgs)
public ActivationService(App app, Type defaultNavItem, Lazy<UIElement> shell = null)
public async Task RedirectLoginPageAsync()
public void SetShell(Lazy<UIElement> shell)
