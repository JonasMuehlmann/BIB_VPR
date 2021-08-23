using Messenger.Core.Models;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.DialogBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ManageRolesDialog : ContentDialog
    {
        public ManageRolesDialogViewModel ViewModel { get; } = new ManageRolesDialogViewModel();

        #region UI Components

        public List<ToolTip> OpenTooltips { get; set; }

        public List<Button> ActivatedButtons { get; set; }

        #endregion

        public Color OriginalColor { get; set; }

        public string OriginalTitle { get; set; }

        /// <summary>
        /// Independent content dialog with own view model to add, remove and update team roles
        /// </summary>
        public ManageRolesDialog()
        {
            InitializeComponent();

            OpenTooltips = new List<ToolTip>();
            ActivatedButtons = new List<Button>();
        }

        protected override void OnApplyTemplate()
        {
            App.EventProvider.TeamUpdated += ViewModel.OnTeamUpdated;

            base.OnApplyTemplate();
        }

        #region Team Roles List

        private void TeamRolesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.HasChanged)
            {
                TeamRoleViewModel previousRole = e.RemovedItems.Single() as TeamRoleViewModel;

                if (previousRole.Title != ViewModel.PendingChange.Title)
                {
                    previousRole.Title = OriginalTitle;
                }

                previousRole.Color = OriginalColor;

                ViewModel.HasChanged = false;
            }

            ViewModel.FilterGrantablePermissions();

            ClearAll();
        }

        private void RemoveTeamRoleButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            TeamRoleViewModel teamRole = button.DataContext as TeamRoleViewModel;

            object flagged = ToolTipService.GetToolTip(button);

            if (flagged != null)
            {
                (flagged as ToolTip).IsOpen = false;
                OpenTooltips.Remove((flagged as ToolTip));

                ViewModel.RemoveTeamRoleCommand.Execute(teamRole);
            }
            else
            {
                button.Foreground = new SolidColorBrush(Colors.IndianRed);
                button.Opacity = 1.0;

                ToolTip toolTip = new ToolTip();
                toolTip.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Right;
                toolTip.Foreground = new SolidColorBrush(Colors.White);
                toolTip.Background = new SolidColorBrush(Colors.IndianRed);
                toolTip.Content = "Click again to remove";

                ToolTipService.SetToolTip(button, toolTip);
                toolTip.IsOpen = true;

                OpenTooltips.Add(toolTip);
            }
        }

        #endregion

        #region Edit Color

        private void ColorPickerOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (OriginalColor != colorPicker.Color)
            {
                ViewModel.PendingChange.Color = colorPicker.Color;
                ViewModel.HasChanged = true;
            }

            ColorPickerButton.Flyout.Hide();
        }

        private void ColorPickerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PendingChange.Color = OriginalColor;
            ColorPickerButton.Flyout.Hide();
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            ViewModel.PendingChange.Color = sender.Color;
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            OriginalColor = ViewModel.SelectedTeamRole.Color;
        }

        #endregion

        #region Edit Title

        private void EditTitleButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.IsInEditMode = true;
            OriginalTitle = ViewModel.SelectedTeamRole.Title;
        }

        private void EditTitleAcceptButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (OriginalTitle != ViewModel.PendingChange.Title)
            {
                ViewModel.HasChanged = true;
            }

            ViewModel.IsInEditMode = false;
        }

        private void EditTitleCancelButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.PendingChange.Title = OriginalTitle;
            ViewModel.IsInEditMode = false;
        }

        #endregion

        #region Edit Permissions

        private void RevokePermissionButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            Permissions permission = (Permissions)button.DataContext;

            object flagged = ToolTipService.GetToolTip(button);

            if (flagged != null)
            {
                (flagged as ToolTip).IsOpen = false;
                OpenTooltips.Remove(flagged as ToolTip);

                ViewModel.RevokePermissionCommand.Execute(permission);
            }
            else
            {
                button.Foreground = new SolidColorBrush(Colors.IndianRed);
                button.Opacity = 1.0;

                ToolTip toolTip = new ToolTip();
                toolTip.Placement = Windows.UI.Xaml.Controls.Primitives.PlacementMode.Right;
                toolTip.Foreground = new SolidColorBrush(Colors.White);
                toolTip.Background = new SolidColorBrush(Colors.IndianRed);
                toolTip.Content = "Click again to remove";

                ToolTipService.SetToolTip(button, toolTip);
                toolTip.IsOpen = true;

                OpenTooltips.Add(toolTip);
            }
        }

        private void GrantablePermission_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox permissionCheckBox = sender as CheckBox;

            if (Enum.TryParse(typeof(Permissions), permissionCheckBox.Content.ToString(), out object result))
            {
                Permissions permission = (Permissions)result;

                ViewModel.PendingChange.Permissions.Add(permission);
                ViewModel.PendingChange.PendingPermissions.Add(permission);

                ViewModel.GrantablePermissions.Remove(permission);

                ViewModel.HasChanged = true;
            }
        }

        #endregion

        #region Actions

        private void UpdateTeamRoleButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.UpdateTeamRoleCommand?.Execute(ViewModel.PendingChange);
        }

        private void CloseButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            App.EventProvider.TeamUpdated -= ViewModel.OnTeamUpdated;

            ClearAll();
            Hide();
        }

        #endregion

        #region Helpers

        private void ClearAll()
        {
            if (OpenTooltips.Count > 0)
            {
                foreach (ToolTip toolTip in OpenTooltips)
                {
                    toolTip.IsOpen = false;
                }

                OpenTooltips.Clear();
            }

            if (ActivatedButtons.Count > 0)
            {
                foreach (Button button in ActivatedButtons)
                {
                    button.Foreground = new SolidColorBrush(Colors.White);
                    button.Opacity = .35;
                }

                ActivatedButtons.Clear();
            }
        }

        #endregion
    }
}
