﻿using Messenger.Views.Pages;
using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Services
{
    /// <summary>
    /// Provides application UI navigation methods
    /// </summary>
    public static class NavigationService
    {
        public static event NavigatedEventHandler Navigated;

        public static event NavigationFailedEventHandler NavigationFailed;

        private static Frame _contentFrame;
        private static Frame _frame;
        private static object _lastParamUsed;

        public static Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
                    RegisterFrameEvents();
                }

                return _frame;
            }

            set
            {
                UnregisterFrameEvents();
                _frame = value;
                RegisterFrameEvents();
            }
        }

        public static Frame ContentFrame
        {
            get { return _contentFrame; }
            set { _contentFrame = value; }
        }

        public static bool CanGoBack => ContentFrame.CanGoBack;

        public static bool CanGoForward => ContentFrame.CanGoForward;

        public static bool GoBack()
        {
            if (CanGoBack)
            {
                ContentFrame.GoBack();
                return true;
            }

            return false;
        }

        public static void GoForward() => Frame.GoForward();

        public static bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            if (pageType == null || !pageType.IsSubclassOf(typeof(Page)))
            {
                throw new ArgumentException($"Invalid pageType '{pageType}', please provide a valid pageType.", nameof(pageType));
            }

            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(pageType, parameter, infoOverride);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                }

                if (pageType == typeof(TeamNavPage) || pageType == typeof(ChatNavPage))
                {
                    Open<LandingPage>();
                }

                return navigationResult;
            }
            else
            {
                return false;
            }
        }

        public static bool Navigate<T>(object parameter = null, NavigationTransitionInfo infoOverride = null)
            where T : Page
            => Navigate(typeof(T), parameter, infoOverride);

        public static bool Open<T>(object parameter = null) where T : Page
        {
            Type pageType = typeof(T);

            if (pageType == null || !pageType.IsSubclassOf(typeof(Page)))
            {
                return false;
            }

            Type currentPage = Frame.Content?.GetType();
            if (currentPage == typeof(SettingsPage)
                    && currentPage == pageType)
            {
                return ContentFrame.Navigate(typeof(ChatPage), parameter);
            }
            else if (currentPage == typeof(TeamManagePage)
                    && currentPage == pageType)
            {
                return ContentFrame.Navigate(typeof(ChatPage), parameter);
            }

            return ContentFrame.Navigate(typeof(T), parameter);
        }

        private static void RegisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated += Frame_Navigated;
                _frame.NavigationFailed += Frame_NavigationFailed;
            }
        }

        private static void UnregisterFrameEvents()
        {
            if (_frame != null)
            {
                _frame.Navigated -= Frame_Navigated;
                _frame.NavigationFailed -= Frame_NavigationFailed;
            }
        }

        private static void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => NavigationFailed?.Invoke(sender, e);

        private static void Frame_Navigated(object sender, NavigationEventArgs e) => Navigated?.Invoke(sender, e);
    }
}
