using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for WindowsControls.xaml
    /// </summary>
    /// 
    public partial class WindowsControls : UserControl
    {
        public WindowsControls()
        {
            InitializeComponent();
        }

        private void MinimizeBtn_Click( object sender, RoutedEventArgs e )
        {
            Window.GetWindow( this ).WindowState = WindowState.Minimized;
        }

        private void FullscreenBtn_Click( object sender, RoutedEventArgs e )
        {
            if (Window.GetWindow( this ) is ControlWindow controlWindow)
            {
                controlWindow.ToggleFullscreen();
            }
        }

        private void ExitBtn_Click( object sender, RoutedEventArgs e )
        {
            if (Window.GetWindow( this ) is ControlWindow controlWindow)
            {
                controlWindow.Shutdown();
            }
        }
    }
}