#Benutzte Pakete
System
System.Data.SqlClient
System.Threading.Tasks
Messenger.Core.Models
Messenger.Core.Helpers
System.Data
System.Linq
Serilog.Context
System.Collections.Generic
#Exportschnittstellen
public static async Task<bool> DeleteUser(string userId)
public static async Task<User> GetOrCreateApplicationUser(User userdata)
public static async Task<User> GetUser(string userId)
public static async Task<User> GetUser(string userName, uint nameId)
public static async Task<IList<string>> SearchUser(string userName)
public static async Task<bool> Update(string userId, string columnToChange, string newVal)
public static async Task<bool> UpdateUserBio(string userId, string newBio)
public static async Task<bool> UpdateUserMail(string userId, string newMail)
public static async Task<bool> UpdateUserPhoto(string userId, string newPhoto)
public static async Task<bool> UpdateUsername(string userId, string newUsername)
