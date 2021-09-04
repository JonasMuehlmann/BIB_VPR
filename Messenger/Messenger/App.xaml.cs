using System;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.Pages;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Messenger
{
    public sealed partial class App : Application
    {
        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private Lazy<ActivationService> _activationService;

        private ActivationService ActivationService
        {
            get { return _activationService.Value; }
        }

        internal static StateProvider StateProvider;

        internal static EventProvider EventProvider;

        public App()
        {
            InitializeComponent();
            UnhandledException += OnAppUnhandledException;

            EventProvider = new EventProvider();
            StateProvider = new StateProvider();

            // Deferred execution until used. Check https://docs.microsoft.com/dotnet/api/system.lazy-1 for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
            IdentityService.LoggedOut += OnLoggedOut;

            ApplicationView.PreferredLaunchViewSize = new Size(1440, 900);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(args);
            }

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);

            // Handle notification activation
            if (args is ToastNotificationActivatedEventArgs toastActivationArgs)
            {
                // Obtain the arguments from the notification
                string argument = toastActivationArgs.Argument;

                switch (argument)
                {
                    case "NavigateTeams":
                        NavigationService.Navigate<TeamNavPage>();
                        NavigationService.Open<LandingPage>();
                        break;
                    case "NavigateChats":
                        NavigationService.Navigate<ChatNavPage>();
                        NavigationService.Open<LandingPage>();
                        break;
                    default:
                        break;
                }

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastActivationArgs.UserInput;

                // TODO: Show the corresponding content
            }
        }

        private void OnAppUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO WTS: Please log and handle the exception as appropriate to your scenario
            // For more info see https://docs.microsoft.com/uwp/api/windows.ui.xaml.application.unhandledexception
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(this, typeof(Views.Pages.TeamNavPage), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new Views.Pages.ShellPage();
        }

        private async void OnLoggedOut(object sender, EventArgs e)
        {
            ActivationService.SetShell(new Lazy<UIElement>(CreateShell));
            await ActivationService.RedirectLoginPageAsync();
        }
    }
}
