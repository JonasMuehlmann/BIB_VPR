#Benutzte Pakete
System.Threading.Tasks
Messenger.Helpers
Windows.ApplicationModel.Core
Windows.Storage
Windows.UI.Core
Windows.UI.Xaml
#Importschnittstellen
Messenger.Services.ThemeSelectorService.LoadThemeFromSettingsAsync()
Messenger.Services.ThemeSelectorService.SaveThemeInSettingsAsync(ElementTheme)
Messenger.Services.ThemeSelectorService.SetRequestedThemeAsync()
string.IsNullOrEmpty(string?)
System.Enum.TryParse(System.Type, string?, out object?)
#Exportschnittstellen
public static async Task InitializeAsync()
public static async Task SetRequestedThemeAsync()
public static async Task SetThemeAsync(ElementTheme theme)
