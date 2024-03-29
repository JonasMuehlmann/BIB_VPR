﻿using System;
using System.Threading.Tasks;

using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Microsoft.Toolkit.Uwp.Helpers;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Messenger.Services
{
    public static class FirstRunDisplayService
    {
        private static bool shown = false;

        internal static async Task ShowIfAppropriateAsync()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, async () =>
                {
                    if (SystemInformation.IsFirstRun && !shown)
                    {
                        shown = true;
                        var dialog = new FirstRunDialog();
                        await dialog.ShowAsync();
                    }
                });
        }
    }
}
