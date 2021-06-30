using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Messenger.Controls.Navigation
{
    public sealed partial class NavigationHeader : UserControl
    {



        public ICommand OpenTeamManagerCommand
        {
            get { return (ICommand)GetValue(OpenTeamManagerCommandProperty); }
            set { SetValue(OpenTeamManagerCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenTeamManagerCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenTeamManagerCommandProperty =
            DependencyProperty.Register("OpenTeamManagerCommand", typeof(ICommand), typeof(NavigationHeader), new PropertyMetadata(null));



        public ICommand OpenProfileCommand
        {
            get { return (ICommand)GetValue(OpenProfileCommandProperty); }
            set { SetValue(OpenProfileCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenProfileCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpenProfileCommandProperty =
            DependencyProperty.Register("OpenProfileCommand", typeof(ICommand), typeof(NavigationHeader), new PropertyMetadata(null));



        public ICommand ChangeTeamDetailsCommand
        {
            get { return (ICommand)GetValue(ChangeTeamDetailsCommandProperty); }
            set { SetValue(ChangeTeamDetailsCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OpenProfileCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChangeTeamDetailsCommandProperty =
            DependencyProperty.Register("ChangeTeamDetailsCommand", typeof(ICommand), typeof(NavigationHeader), new PropertyMetadata(null));




        public string CurrentTeamName
        {
            get { return (string)GetValue(CurrentTeamNameProperty); }
            set { SetValue(CurrentTeamNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTeamName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTeamNameProperty =
            DependencyProperty.Register("CurrentTeamName", typeof(string), typeof(NavigationHeader), new PropertyMetadata(string.Empty));

        public string CurrentTeamDescription
        {
            get { return (string)GetValue(CurrentTeamDescriptionProperty); }
            set { SetValue(CurrentTeamDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTeamName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTeamDescriptionProperty =
            DependencyProperty.Register("CurrentTeamDescription", typeof(string), typeof(NavigationHeader), new PropertyMetadata(string.Empty));




        public NavigationHeader()
        {
            this.InitializeComponent();
        }
    }
}
