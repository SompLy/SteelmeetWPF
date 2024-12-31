using System.Windows;
using System.Windows.Input;

namespace SteelmeetWPF
{
    public partial class TabControlStyleCustom : ResourceDictionary
    {
        public TabControlStyleCustom()
        {
            InitializeComponent();
        }

        private void TabHeader_MouseLeftButtonDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            // Find the parent window
            if( e.LeftButton == MouseButtonState.Pressed )
            {
                Window window = Window.GetWindow((DependencyObject)sender);
                window?.DragMove();
            }
        }
    }
}
