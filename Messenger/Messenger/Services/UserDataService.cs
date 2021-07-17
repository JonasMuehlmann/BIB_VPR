using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Windows.Storage;

namespace Messenger.Services
{
    public class UserDataService
    {
        private const string _userSettingsKey = "IdentityUser";

        private UserViewModel _user;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        public bool IsOnline => _user != null;

        public event EventHandler<UserViewModel> UserDataUpdated;

        public UserDataService()
        {
        }

        public void Initialize()
        {
            IdentityService.LoggedIn += OnLoggedIn;
            IdentityService.LoggedOut += OnLoggedOut;
            MessengerService.RegisterListenerForUserUpdate(OnUserUpdate);
        }

        public async Task<UserViewModel> GetUserAsync()
        {
            if (_user == null)
            {
                _user = await GetUserFromCacheAsync();
                if (_user == null)
                {
                    _user = GetDefaultUserData();
                }
            }

            return _user;
        }

        public async Task<bool> UpdateUserBio(string bio)
        {
            bool isSuccess = await MessengerService.UpdateUserBio(_user.Id, bio);

            return isSuccess;
        }

        private void OnUserUpdate(object sender, User user)
        {
            _user.Bio = user.Bio;
            _user.Name = user.DisplayName;
            _user.NameId = user.NameId;
        }

        private async void OnLoggedIn(object sender, EventArgs e)
        {
            _user = await GetUserFromGraphApiAsync();
            
            UserDataUpdated?.Invoke(this, _user);
        }

        private async void OnLoggedOut(object sender, EventArgs e)
        {
            _user = null;
            await ApplicationData.Current.LocalFolder.SaveAsync<User>(_userSettingsKey, null);
        }

        private async Task<UserViewModel> GetUserFromCacheAsync()
        {
            var cacheData = await ApplicationData.Current.LocalFolder.ReadAsync<User>(_userSettingsKey);
            return await GetUserViewModelFromData(cacheData);
        }

        private async Task<UserViewModel> GetUserFromGraphApiAsync()
        {
            var accessToken = await IdentityService.GetAccessTokenForGraphAsync();
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var userData = await MicrosoftGraphService.GetUserInfoAsync(accessToken);
            if (userData != null)
            {
                userData.Photo = await MicrosoftGraphService.GetUserPhoto(accessToken);
                await ApplicationData.Current.LocalFolder.SaveAsync(_userSettingsKey, userData);
            }

            return await GetUserViewModelFromData(userData);
        }

        private async Task<UserViewModel> GetUserViewModelFromData(User userData)
        {
            if (userData == null)
            {
                return null;
            }

            var userPhoto = string.IsNullOrEmpty(userData.Photo)
                ? ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
                : await ImageHelper.ImageFromStringAsync(userData.Photo);

            var userFromDatabase = await UserService.GetOrCreateApplicationUser(userData);

            // Connect to signal-r hub and retrieve the team list
            var teams = await InitializeSignalR(userData.Id);

            // Merged with user model from the application database
            return new UserViewModel()
            {
                Id = userData.Id,
                Name = userFromDatabase.DisplayName,
                NameId = userFromDatabase.NameId,
                Bio = userFromDatabase.Bio,
                Mail = userFromDatabase.Mail,
                Photo = userPhoto
            };
        }

        private UserViewModel GetDefaultUserData()
        {
            return new UserViewModel()
            {
                Name = IdentityService.GetAccountUserName(),
                Photo = ImageHelper.ImageFromAssetsFile("DefaultIcon.png")
            };
        }

        private async Task<IList<Team>> InitializeSignalR(string userId)
        {
            return await MessengerService.Initialize(userId);
        }
    }
}
