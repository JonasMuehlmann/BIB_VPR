using System;

using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Security.Authentication.Web;
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
