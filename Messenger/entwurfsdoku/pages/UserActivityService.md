#Benutzte Pakete
System.Threading.Tasks
Windows.ApplicationModel.Core
Windows.ApplicationModel.UserActivities
Windows.UI.Core
Windows.UI.Shell
#Exportschnittstellen
public static async Task CreateUserActivityAsync(UserActivityData activityData)
public static async Task CreateUserActivityAsync(UserActivityData activityData, IAdaptiveCard adaptiveCard)
