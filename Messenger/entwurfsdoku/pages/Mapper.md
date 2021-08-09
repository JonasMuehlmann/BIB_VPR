#Benutzte Pakete
System
System.Linq
System.Collections.Generic
System.Data
System.Data.SqlClient
Serilog
Serilog.Context
Newtonsoft.Json
Newtonsoft.Json.Linq
#Importschnittstellen
Messenger.Core.Helpers.SqlHelpers.TryConvertDbValue<T>(object, System.Func<object, T>)
System.Enum.Parse(System.Type, string)
#Exportschnittstellen
public static Channel ChannelFromDataRow(DataRow row)
public static Membership MembershipFromDataRow(DataRow row)
public static Mention MentionFromDataRow(DataRow row)
public static Mentionable MentionableFromDataRow(DataRow row)
public static Message MessageFromDataRow(DataRow row)
public static Notification NotificationFromDataRow(DataRow row)
public static NotificationMute NotificationMuteFromDataRow(DataRow row)
public static Reaction ReactionFromDataRow(DataRow row)
public static string StringFromDataRow(DataRow row, string columnName)
public static T StringToEnum<T>(object value)
public static Team TeamFromDataRow(DataRow row)
public static TeamRole TeamRoleFromDataRow(DataRow row)
public static User UserFromDataRow(DataRow row)
public static User UserFromMSGraph(User userdata)
public static JObject strToJObject(object str)
