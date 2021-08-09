#Benutzte Pakete
System
Windows.UI.Xaml
Windows.UI.Xaml.Controls
Windows.UI.Xaml.Media.Animation
Windows.UI.Xaml.Navigation
#Exportschnittstellen
public static bool GoBack()
public static void GoForward() => Frame.GoForward();
public static bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
