using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;

namespace Messenger.ViewModels
{
    public class MainViewModel : Observable
    {
        /// <summary>
        /// Shows an example of using service in view model, and adding the data to source for the view
        /// </summary>
        /* This should be deleted (example code) */
        private AzureTestDataService testDataService => Singleton<AzureTestDataService>.Instance;

        /* This should be deleted (example code) */
        public ObservableCollection<SampleTeam> SampleTeamsSource { get; } = new ObservableCollection<SampleTeam>();

        public MainViewModel()
        {
            InitalizeData();
        }

        private async void InitalizeData()
        {
            /* This should be deleted (example code) */
            foreach (SampleTeam team in await testDataService.GetAllTeams())
            {
                SampleTeamsSource.Add(team);
            }
        }
    }
}
