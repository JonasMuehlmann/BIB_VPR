using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ManageRolesDialog : ContentDialog
    {
        public ObservableCollection<MemberRole> MemberRoles
        {
            get { return (ObservableCollection<MemberRole>)GetValue(MemberRolesProperty); }
            set { SetValue(MemberRolesProperty, value); }
        }

        public static readonly DependencyProperty MemberRolesProperty =
            DependencyProperty.Register("MemberRoles", typeof(ObservableCollection<MemberRole>), typeof(ManageRolesDialog), new PropertyMetadata(new ObservableCollection<MemberRole>()));

        public MemberRole SelectedRole
        {
            get { return (MemberRole)GetValue(SelectedRoleProperty); }
            set { SetValue(SelectedRoleProperty, value); }
        }

        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(MemberRole), typeof(ManageRolesDialog), new PropertyMetadata(null));

        public ManageRolesDialog()
        {
            InitializeComponent();
        }

        public async static Task<IAsyncOperation<ContentDialogResult>> Open()
        {
            TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

            ManageRolesDialog dialog = new ManageRolesDialog();
            dialog.MemberRoles = new ObservableCollection<MemberRole>();

            IEnumerable<TeamRole> roles = await TeamService.ListRoles(selectedTeam.Id);

            if (roles == null || roles.Count() <= 0) return dialog.ShowAsync();

            foreach (TeamRole role in roles)
            {
                IList<Permissions> permissions = await TeamService.GetPermissionsOfRole(selectedTeam.Id, role.Role);

                dialog.MemberRoles.Add(
                    new MemberRole()
                    {
                        Title = role.Role.ToUpper(),
                        TeamId = selectedTeam.Id,
                        Permissions = permissions.ToList()
                    });
            }

            return dialog.ShowAsync();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SelectablePermissions.ItemsSource = Enum.GetNames(typeof(Permissions));
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
