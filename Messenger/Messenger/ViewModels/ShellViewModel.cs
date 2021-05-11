﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.Views;

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ShellViewModel : Observable
    {
        //private readonly KeyboardAccelerator _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);
        //private readonly KeyboardAccelerator _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

        //private bool _isBackEnabled;
        //private IList<KeyboardAccelerator> _keyboardAccelerators;
        //private WinUI.NavigationView _navigationView;
        //private WinUI.NavigationViewItem _selected;
        private ICommand _loadedCommand;
        private ICommand _itemInvokedCommand;
        private ICommand _userProfileCommand;
        private UserViewModel _user;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        //public bool IsBackEnabled
        //{
        //    get { return _isBackEnabled; }
        //    set { Set(ref _isBackEnabled, value); }
        //}

        //public WinUI.NavigationViewItem Selected
        //{
        //    get { return _selected; }
        //    set { Set(ref _selected, value); }
        //}

        //TODO Load again
        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        //public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand UserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OnUserProfile));

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame)
        {
            //_keyboardAccelerators = keyboardAccelerators;
            NavigationService.Frame = frame;
            NavigationService.NavigationFailed += Frame_NavigationFailed;
            NavigationService.Navigated += Frame_Navigated;
            IdentityService.LoggedOut += OnLoggedOut;
            UserDataService.UserDataUpdated += OnUserDataUpdated;
        }

        private async void OnLoaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            //_keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            //_keyboardAccelerators.Add(_backKeyboardAccelerator);
            User = await UserDataService.GetUserAsync();
        }

        private void OnUserDataUpdated(object sender, UserViewModel userData)
        {
            User = userData;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            NavigationService.NavigationFailed -= Frame_NavigationFailed;
            NavigationService.Navigated -= Frame_Navigated;
            UserDataService.UserDataUpdated -= OnUserDataUpdated;
            IdentityService.LoggedOut -= OnLoggedOut;
        }

        private void OnUserProfile()
        {
            NavigationService.Navigate<SettingsPage>();
        }

        //private void OnItemInvoked(WinUI.NavigationViewItemInvokedEventArgs args)
        //{
        //    if (args.IsSettingsInvoked)
        //    {
        //        NavigationService.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
        //    }
        //    else if (args.InvokedItemContainer is WinUI.NavigationViewItem selectedItem)
        //    {
        //        var pageType = selectedItem.GetValue(NavHelper.NavigateToProperty) as Type;
        //        NavigationService.Navigate(pageType, null, args.RecommendedNavigationTransitionInfo);
        //    }
        //}

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw e.Exception;
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            //IsBackEnabled = NavigationService.CanGoBack;
            if (e.SourcePageType == typeof(SettingsPage))
            {
                //Selected = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            //var selectedItem = GetSelectedItem(_navigationView.MenuItems, e.SourcePageType);
            //if (selectedItem != null)
            //{
            //    Selected = selectedItem;
            //}
        }

        //private WinUI.NavigationViewItem GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
        //{
        //    foreach (var item in menuItems.OfType<WinUI.NavigationViewItem>())
        //    {
        //        if (IsMenuItemForPageType(item, pageType))
        //        {
        //            return item;
        //        }

        //        var selectedChild = GetSelectedItem(item.MenuItems, pageType);
        //        if (selectedChild != null)
        //        {
        //            return selectedChild;
        //        }
        //    }

        //    return null;
        //}

        //private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, Type sourcePageType)
        //{
        //    var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
        //    return pageType == sourcePageType;
        //}

        //private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
        //{
        //    var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
        //    if (modifiers.HasValue)
        //    {
        //        keyboardAccelerator.Modifiers = modifiers.Value;
        //    }

        //    keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
        //    return keyboardAccelerator;
        //}

        //private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        //{
        //    var result = NavigationService.GoBack();
        //    args.Handled = result;
        //}
    }
}
