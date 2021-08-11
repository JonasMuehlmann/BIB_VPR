using Messenger.Commands;
using Messenger.Services;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
{
    public sealed partial class LandingPage : Page
    {
        public ICommand OpenSettingCommand { get => new RelayCommand(() => NavigationService.Open<SettingsPage>()); }
        public LandingPage()
        {
            InitializeComponent();
        }

    }
}
