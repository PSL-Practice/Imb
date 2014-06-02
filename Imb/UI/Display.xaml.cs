using System.Windows;
using System.Windows.Controls;
using Imb.Utils;

namespace Imb.UI
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : UserControl
    {
        public Display()
        {
            InitializeComponent();
        }

        private void Image_OnImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            DropHandler.Current.Drop(null, e);
        }
    }
}
