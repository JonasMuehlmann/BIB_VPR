using System;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;

namespace Messenger.ViewModels
{
    public class MainViewModel : Observable
    {
        private AzureTestDataService testDataService => Singleton<AzureTestDataService>.Instance;

        public MainViewModel()
        {
            InitalizeData();
        }

        private async void InitalizeData()
        {
            await testDataService.GetAllTeams();
        }
    }
}
