using System.Windows;
using System.Windows.Controls;
using Imb.Utils;

namespace Imb.UI
{
    /// <summary>
    /// Interaction logic for CategoryTree.xaml
    /// </summary>
    public partial class CategoryTree : UserControl
    {
        public CategoryTree()
        {
            InitializeComponent();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            DropHandler.Current.Drop(null, e);
        }
    }
}
