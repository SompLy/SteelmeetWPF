using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.ExtendedProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for TimerControl.xaml
    /// </summary>
    public partial class TimerControl : UserControl
    {
        private ControlWindow controlWindow = null;

        int secondsLapp = 0;
        int minutesLapp = 0;
        int secondsLyft = 0;
        int minutesLyft = 0;

        DispatcherTimer liftTimer = new DispatcherTimer();
        TimeSpan liftTime;
        DispatcherTimer lappTimer = new DispatcherTimer();
        TimeSpan lappTime;

        public enum TimerOptions
        {
            LIFT_ONE_MINUTE = 0,
            CUSTOM_TIME     = 1,
            LAPP_ONE_MINUTE = 2,
            LIFT_RESET = 3,
            LAPP_RESET = 4,
        }

        public TimerControl()
        {
            InitializeComponent();

            liftTimer.Interval = TimeSpan.FromSeconds( 1 );
            liftTimer.Tick += LiftTimer_Tick;
            liftTime = TimeSpan.Zero;

            lappTimer.Interval = TimeSpan.FromSeconds( 1 );
            lappTimer.Tick += LappTimer_Tick;
            lappTime = TimeSpan.Zero;

            for (int i = 1; i < 25; i++)
                TimestampHourCb.Items.Add( i );
            for( int i = 1 ; i < 61 ; i++ )
                TimestampMinCb.Items.Add( i );
            for( int i = 1 ; i < 61 ; i++ )
                MinutesMinCb.Items.Add( i );

            TimestampHourCb.SelectedIndex = 0;
            TimestampMinCb.SelectedIndex = 0;
            MinutesMinCb.SelectedIndex = 0;

            Loaded += TimerControl_Loaded;
        }

        private void TimerControl_Loaded( object sender, RoutedEventArgs e ) 
        {
            controlWindow = Window.GetWindow( this ) as ControlWindow;
        }
        private void TimestampBtn_Click(object sender, RoutedEventArgs e)
        {
            // Sätter klockan baserat på systemtiden
            int newMinutes = ( int )TimestampHourCb.SelectedValue - DateTime.Now.Hour;
            newMinutes *= 60;
            newMinutes += ( int )TimestampMinCb.SelectedValue - DateTime.Now.Minute - 1;

            int newSeconds = 60 - DateTime.Now.Second;

            TimerController( TimerOptions.CUSTOM_TIME, newMinutes, newSeconds );
        }

        private void MinutesBtn_Click(object sender, RoutedEventArgs e)
        {
            TimerController( TimerOptions.CUSTOM_TIME, ( int )MinutesMinCb.SelectedValue, 0 );
        }

        public void TimerController( TimerOptions _option, int _customMinTime = 0, int _customSecTime = 0 )
        {
            switch( _option )
            {
                case TimerOptions.LIFT_ONE_MINUTE:
                {
                    liftTime = TimeSpan.FromSeconds( 60 );
                    liftTimer.Start();
                    break;
                }
                case TimerOptions.CUSTOM_TIME:    
                {
                    int TotSeconds = ( _customMinTime * 60 ) + _customSecTime;
                    liftTime = TimeSpan.FromSeconds( TotSeconds );
                    liftTimer.Start();
                    break;
                }
                case TimerOptions.LAPP_ONE_MINUTE:
                {
                    lappTime = TimeSpan.FromSeconds( 60 );
                    lappTimer.Start();
                    break;
                }
                case TimerOptions.LIFT_RESET:     
                {
                    liftTime = TimeSpan.Zero;
                    liftTimer.Stop();
                    string time = liftTime.ToString( @"mm\:ss" );
                    liftTimerTb.Text = time;

                    foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                        specWindow.timer.liftTimerSpecTb.Text = time;
                    break;
                }
                case TimerOptions.LAPP_RESET:
                {
                    lappTime = TimeSpan.Zero;
                    lappTimer.Stop();
                    string time = lappTime.ToString( @"mm\:ss" );
                    smallPieceOfPaperTimerTb.Text = time;

                    foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                        specWindow.timer.smallPieceOfPaperTimerSpecTb.Text = time;
                    break;
                }
                default: { break; }
            }
        }

        private void LiftTimer_Tick( object sender, EventArgs e )
        {
            liftTime = liftTime.Add( TimeSpan.FromSeconds( -1 ) );
            string time = liftTime.ToString( @"mm\:ss" );
            liftTimerTb.Text = time;

            foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                specWindow.timer.liftTimerSpecTb.Text = time;
        }

        private void LappTimer_Tick( object sender, EventArgs e )
        {
            lappTime = lappTime.Add( TimeSpan.FromSeconds( -1 ) );
            string time = lappTime.ToString( @"mm\:ss" );
            smallPieceOfPaperTimerTb.Text = time;

            foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                specWindow.timer.smallPieceOfPaperTimerSpecTb.Text = time;
        }

        private void MinutesResetBtn_Click( object sender, RoutedEventArgs e )
        {
            TimerController( TimerControl.TimerOptions.LIFT_RESET );
        }
        
        private void TimestampResetBtn_Click( object sender, RoutedEventArgs e )
        {
            TimerController( TimerControl.TimerOptions.LAPP_RESET );
        }


    }
}
