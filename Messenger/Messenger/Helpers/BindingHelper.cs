using Messenger.Controls.ChatControls;
using Messenger.ViewModels.DataViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers
{
    public class BindingHelper
    {
        public static bool GetHorizontalAlignBindingPath(DependencyObject obj)
        {
            return (bool)obj.GetValue(HorizontalAlignBindingPathProperty);
        }

        public static void SetHorizontalAlignBindingPath(DependencyObject obj, bool value)
        {
            obj.SetValue(HorizontalAlignBindingPathProperty, value);
        }

        // Using a DependencyProperty as the backing store for HorizontalAlignBindingPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalAlignBindingPathProperty =
            DependencyProperty.RegisterAttached("HorizontalAlignBindingPath", typeof(bool), typeof(BindingHelper), new PropertyMetadata(null, HorizontalAlignBindingPathPropertyChanged));

        private static void HorizontalAlignBindingPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var element = obj as ListViewItem;
            var message = element.Content as MessageViewModel;

            if (message.IsMyMessage)
            {
                element.HorizontalContentAlignment = HorizontalAlignment.Right;
            }
            else
            {
                element.HorizontalContentAlignment = HorizontalAlignment.Left;
            }
        }
    }
}
