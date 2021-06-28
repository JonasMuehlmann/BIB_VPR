using Messenger.Core.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.TemplateSelectors
{
    public class TeamDataTemplateSelector: DataTemplateSelector
    {
        public DataTemplate TeamTemplate { get; set; }

        public DataTemplate TeamChannelTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return GetTemplate(item) ?? base.SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return GetTemplate(item) ?? base.SelectTemplateCore(item, container);
        }

        private DataTemplate GetTemplate(object item)
        {
            switch (item)
            {
                case Team team:
                    return TeamTemplate;
                case Channel teamChannel:
                    return TeamChannelTemplate;
            }

            return null;
        }
    }
}
