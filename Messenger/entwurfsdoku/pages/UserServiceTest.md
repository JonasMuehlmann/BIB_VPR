#Benutzte Pakete
Messenger.Core.Models
Messenger.Core.Services
System.Data.SqlClient
System.Collections.Generic
Microsoft.VisualStudio.TestTools.UnitTesting
#Importschnittstellen
Messenger.Core.Services.UserService.DeleteUser(string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
Messenger.Core.Services.UserService.GetUser(string)
Messenger.Core.Services.UserService.SearchUser(string)
Messenger.Core.Services.UserService.Update(string, string, string)
Messenger.Core.Services.UserService.UpdateUserBio(string, string)
Messenger.Core.Services.UserService.UpdateUserMail(string, string)
Messenger.Core.Services.UserService.UpdateUsername(string, string)
Messenger.Core.Services.UserService.UpdateUserPhoto(string, string)
string.Join(char, params string?[])
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
#Exportschnittstellen
public void ChangeBio_Test()
public void ChangeMail_Test()
public void ChangePhoto_Test()
public void Cleanup()
public void DeleteNonExistentUser_Test()
public void DeleteUser_Test()
public void GetOrCreateApplicationNonExisting_Test()
public void GetOrCreateApplicationUserExisting_Test()
public void GetOrCreateApplicationUserSecondNameId_Test()
public void SearchUser_Test()
public void UpdateUserInfo_Test()
public void UpdateUsername_Test()
