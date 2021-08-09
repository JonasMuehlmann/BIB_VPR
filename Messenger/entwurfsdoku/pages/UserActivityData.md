#Benutzte Pakete
System.Threading.Tasks
Messenger.Activation
Windows.ApplicationModel.UserActivities
Windows.UI
#Importschnittstellen
string.IsNullOrEmpty(string?)
#Exportschnittstellen
public async Task<UserActivity> ToUserActivity()
public UserActivityData(string activityId, SchemeActivationData activationData, string displayText, Color backgroundColor = default, string description = null)
