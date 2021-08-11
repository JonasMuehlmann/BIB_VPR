using Messenger.ViewModels.DataViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Helpers
{
    /// <summary>
    /// Defines "Attached Properties", mainly used to conditionally set styles on XAML
    /// </summary>
    public class BindingHelper
    {
        #region My Message Horizontal Alignment

        public static bool GetHorizontalAlignBindingPath(DependencyObject obj)
        {
            return (bool)obj.GetValue(HorizontalAlignBindingPathProperty);
        }

        public static void SetHorizontalAlignBindingPath(DependencyObject obj, bool value)
        {
            obj.SetValue(HorizontalAlignBindingPathProperty, value);
        }

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

        #endregion
    }
}
