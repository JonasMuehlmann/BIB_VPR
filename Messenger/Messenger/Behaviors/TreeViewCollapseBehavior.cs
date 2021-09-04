using System.Collections.Generic;
using System.Windows.Input;
using Messenger.Commands;

using Microsoft.Xaml.Interactivity;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.Behaviors
{
    /// <summary>
    /// Defines UI-Behavior(Collapse) of XAML component 'TreeView'
    /// </summary>
    public class TreeViewCollapseBehavior : Behavior<WinUI.TreeView>
    {
        public ICommand CollapseAllCommand { get; }

        public TreeViewCollapseBehavior()
        {
            CollapseAllCommand = new RelayCommand(() => CollapseNodes(AssociatedObject.RootNodes));
        }

        private void CollapseNodes(IList<WinUI.TreeViewNode> nodes)
        {
            foreach (var node in nodes)
            {
                CollapseNodes(node.Children);
                AssociatedObject.Collapse(node);
            }
        }
    }
}
