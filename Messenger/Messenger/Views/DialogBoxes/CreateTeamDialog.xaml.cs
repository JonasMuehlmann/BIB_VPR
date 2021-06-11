using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class CreateTeamDialog : ContentDialog
    {
        public string TeamName
        {
            get { return (string)GetValue(TeamNameProperty); }
            set { SetValue(TeamNameProperty, value); }
        }

        public static readonly DependencyProperty TeamNameProperty =
            DependencyProperty.Register("TeamName", typeof(string), typeof(CreateTeamDialog), new PropertyMetadata(string.Empty));

        public string TeamDescription
        {
            get { return (string)GetValue(TeamDescriptionProperty); }
            set { SetValue(TeamDescriptionProperty, value); }
        }

        public static readonly DependencyProperty TeamDescriptionProperty =
            DependencyProperty.Register("TeamDescription", typeof(string), typeof(CreateTeamDialog), new PropertyMetadata(string.Empty));

        public CreateTeamDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(TeamName))
            {
                args.Cancel = true;
                errorTextBlock.Text = "Team name is required.";
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
