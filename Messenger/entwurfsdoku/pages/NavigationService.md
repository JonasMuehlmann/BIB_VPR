#Benutzte Pakete
System
Windows.UI.Xaml
Windows.UI.Xaml.Controls
Windows.UI.Xaml.Media.Animation
Windows.UI.Xaml.Navigation
#Importschnittstellen
Messenger.Services.NavigationService.Navigate(System.Type, object, NavigationTransitionInfo)
Messenger.Services.NavigationService.Open<LandingPage>(object)
Messenger.Services.NavigationService.RegisterFrameEvents()
Messenger.Services.NavigationService.UnregisterFrameEvents()
object.Equals(object?)
System.Type.IsSubclassOf(System.Type)
#Exportschnittstellen
public static bool GoBack()
public static void GoForward() => Frame.GoForward();
public static bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
