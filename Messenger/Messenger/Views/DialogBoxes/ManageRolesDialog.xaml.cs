using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ManageRolesDialog : ContentDialog
    {
        public ObservableCollection<TeamRoleViewModel> MemberRoles
        {
            get { return (ObservableCollection<TeamRoleViewModel>)GetValue(MemberRolesProperty); }
            set { SetValue(MemberRolesProperty, value); }
        }

        public static readonly DependencyProperty MemberRolesProperty =
            DependencyProperty.Register("MemberRoles", typeof(ObservableCollection<TeamRoleViewModel>), typeof(ManageRolesDialog), new PropertyMetadata(new ObservableCollection<TeamRoleViewModel>()));

        public TeamRoleViewModel SelectedRole
        {
            get { return (TeamRoleViewModel)GetValue(SelectedRoleProperty); }
            set { SetValue(SelectedRoleProperty, value); }
        }

        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(TeamRoleViewModel), typeof(ManageRolesDialog), new PropertyMetadata(null));

        public ObservableCollection<Permissions> SelectablePermissions
        {
            get { return (ObservableCollection<Permissions>)GetValue(SelectablePermissionsProperty); }
            set { SetValue(SelectablePermissionsProperty, value); }
        }

        public static readonly DependencyProperty SelectablePermissionsProperty =
            DependencyProperty.Register("SelectablePermissions", typeof(ObservableCollection<Permissions>), typeof(ManageRolesDialog), new PropertyMetadata(new ObservableCollection<Permissions>()));

        public ManageRolesDialog()
        {
            InitializeComponent();
        }

        public async static Task<IAsyncOperation<ContentDialogResult>> Open()
        {
            TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

            ManageRolesDialog dialog = new ManageRolesDialog();
            dialog.MemberRoles = new ObservableCollection<TeamRoleViewModel>();

            IEnumerable<TeamRole> roles = await TeamService.ListRoles(selectedTeam.Id);

            if (roles == null || roles.Count() <= 0) return dialog.ShowAsync();

            foreach (TeamRole role in roles)
            {
                IList<Permissions> permissions = await TeamService.GetPermissionsOfRole(selectedTeam.Id, role.Role);

                dialog.MemberRoles.Add(
                    new TeamRoleViewModel()
                    {
                        Title = string.Concat(role.Role.Substring(0, 1).ToUpper(), role.Role.Substring(1)),
                        TeamId = selectedTeam.Id,
                        Permissions = permissions.ToList()
                    });
            }

            TeamRoleViewModel defaultRole = dialog.MemberRoles.First();
            defaultRole.Color = dialog.colorPicker.Color;
            dialog.SelectedRole = defaultRole;
            dialog.UpdateSelectablePermissions(defaultRole);

            return dialog.ShowAsync();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void SelectedPermissions_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TeamRoleViewModel role = e.AddedItems.Single() as TeamRoleViewModel;
            role.Color = colorPicker.Color;
            SelectedRole = role;
            SelectablePermissions.Clear();

            UpdateSelectablePermissions(role);
        }

        public void UpdateSelectablePermissions(TeamRoleViewModel role)
        {
            Array permissions = Enum.GetValues(typeof(Permissions));

            SelectablePermissionsComboBox.Items.Clear();

            foreach (Permissions permission in permissions.Cast<Permissions>().Where(p => !role.Permissions.Any(rp => rp == p)))
            {
                SelectablePermissionsComboBox.Items.Add(permission);
            }
        }

        private void ColorPickerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerButton.Flyout.Hide();
        }

        private void ColorPickerOkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedRole.Color = colorPicker.Color;
        }
    }
}
