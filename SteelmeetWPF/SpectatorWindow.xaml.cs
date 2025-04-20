using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for SpectatorWindow.xaml
    /// </summary>
    public partial class SpectatorWindow : Window
    {
        private ControlWindow parentWindow;
        private int windowIndex = 0;

        private readonly double _originalWindowWidth = 1920;
        private readonly double _originalWindowHeight = 1080;
        Fullscreen fullscreen = new Fullscreen();
        bool isFullscreen = false;

        private const float NextGroupMarginOffsetPerLifter = -39.65f;

        public SpectatorWindow( ControlWindow _controlWindow )
        {
            InitializeComponent();
            
            parentWindow = _controlWindow;
            windowIndex = parentWindow.spectatorWindowList.Count();
            Loaded += SpectatorWindowLoaded;
        }

        private void SpectatorWindowLoaded(object sender, RoutedEventArgs e)
        {
            SetNextGroupMargin( NextGroupMarginOffsetPerLifter * 10 );
        }
        
        void SetNextGroupMargin(float newMargin)
        {
            if( nextGroupOrderSpec != null )
            {
                if( nextGroupOrderSpec.RenderTransform == null || !( nextGroupOrderSpec.RenderTransform is TranslateTransform ) )
                {
                    nextGroupOrderSpec.RenderTransform = new TranslateTransform();
                }
                
                TranslateTransform translateTransform = nextGroupOrderSpec.RenderTransform as TranslateTransform;

                if( translateTransform != null )
                {
                    double from = nextGroupOrderSpec.Margin.Top;
                    double to = newMargin;

                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = from,
                        To = to,
                        Duration = new Duration(TimeSpan.FromMilliseconds(1000))
                    };
                    
                    translateTransform.BeginAnimation( TranslateTransform.YProperty, animation );
                }
            }
        }

        private void SpectatorWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            double scaleX = this.ActualWidth / _originalWindowWidth;
            double scaleY = this.ActualHeight / _originalWindowHeight;

            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);
            MainGrid.LayoutTransform = scaleTransform;

            MainGrid.RenderTransformOrigin = new System.Windows.Point( 0.5, 0.5 );
        }

        private void SpectatorWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if( ( e.Key == Key.F || e.Key == Key.F11 ) )
            {
                fullscreen.ToggleFullscreen( isFullscreen, this );
                isFullscreen = !isFullscreen;
            }
            if( e.Key == Key.Escape )
            {
                var result = MessageBox.Show("Är du säker att du vill avrätta detta fönster?", "STEELMEET Avrättning", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if( result == MessageBoxResult.Yes )
                {
                    parentWindow.spectatorWindowList.RemoveAt(windowIndex);
                    for (int i = windowIndex; i < parentWindow.spectatorWindowList.Count; i++)
                        parentWindow.spectatorWindowList [i].windowIndex--;

                    this.Close();
                }
            }
        }

        private void SpecDg_OnBeginningEditInDg_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SpecDg_OnCellEditEndingDg_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            // Find the parent window
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Window window = Window.GetWindow((DependencyObject)sender);
                window?.DragMove();
            }
        }
    }
}
