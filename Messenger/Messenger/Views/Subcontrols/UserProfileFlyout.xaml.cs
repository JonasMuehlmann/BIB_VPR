using Messenger.Commands.PrivateChat;
using Messenger.Commands.TeamManage;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class UserProfileFlyout : UserControl
    {
        public MemberViewModel Member
        {
            get { return (MemberViewModel)GetValue(MemberProperty); }
            set { SetValue(MemberProperty, value); }
        }

        public static readonly DependencyProperty MemberProperty =
            DependencyProperty.Register("Member", typeof(MemberViewModel), typeof(UserProfileFlyout), new PropertyMetadata(null, OnMemberChanged));


        public ICommand StartChatWithUserCommand { get => new StartChatWithUserCommand(); }

        public UserProfileFlyout()
        {
            InitializeComponent();
        }

        private static void OnMemberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            if (App.StateProvider.SelectedTeam is PrivateChatViewModel) return;

            MemberViewModel memberValue = e.NewValue as MemberViewModel;
            UserProfileFlyout flyout = d as UserProfileFlyout;

            MemberViewModel selectedMember = CacheQuery.Get<MemberViewModel>(memberValue.TeamId, memberValue.Id);

            if (selectedMember != null)
            {
                flyout.Member = selectedMember;
            }
        }
    }
}
