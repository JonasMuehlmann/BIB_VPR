#Benutzte Pakete
System.Configuration
System.Linq
System.Net.NetworkInformation
System.Threading.Tasks
Messenger.Core.Helpers
Microsoft.Identity.Client
#Exportschnittstellen
public async Task<bool> AcquireTokenSilentAsync() => await AcquireTokenSilentAsync(_graphScopes);
public async Task<string> GetAccessTokenAsync(string[] scopes)
public async Task<string> GetAccessTokenForGraphAsync() => await GetAccessTokenAsync(_graphScopes);
public string GetAccountUserName()
public void InitializeConsoleForAadMultipleOrgs(string clientId)
public void InitializeWithAadAndPersonalMsAccounts()
public void InitializeWithAadMultipleOrgs(bool integratedAuth = false)
public void InitializeWithAadSingleOrg(string tenant, bool integratedAuth = false)
public bool IsAuthorized()
public bool IsLoggedIn() => _authenticationResult != null;
public async Task<LoginResultType> LoginAsync()
public async Task LogoutAsync()
