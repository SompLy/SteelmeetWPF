using System;
using System.Windows;
using System.Windows.Controls;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for OtherOptionsControl.xaml
    /// </summary>
    public partial class OtherOptionsControl: UserControl
    {
        private ControlWindow controlWindow = null;

        public OtherOptionsControl()
        {
            InitializeComponent();

            Loaded += OtherOptionsControlLoaded;
        }

        private void OtherOptionsControlLoaded( object sender, RoutedEventArgs e )
        {
            controlWindow = Window.GetWindow( this ) as ControlWindow;
            ActiveGroupCob.Items.Clear();

            if( controlWindow == null )
                return;

            for( int i = 0 ; i < controlWindow.groupDataList.Count ; i++ )
                ActiveGroupCob.Items.Add( i + 1 ); // To not start at index 0

            for( int i = 0 ; i < 30 ; i++ )
                HeightCob.Items.Add( i );

            for( int i = 0 ; i < 15 ; i++ )
                RackCob.Items.Add( i );

            HeightCob.SelectedIndex = 12;
            RackCob.SelectedIndex = 12;

            ActiveGroupCob.SelectedIndex = 0;
        }

        private void OpenSpectatorWindowBtn_OnClick( object sender, RoutedEventArgs e )
        {
            controlWindow = Window.GetWindow( this ) as ControlWindow;
            if( controlWindow != null )
            {
                var specList = controlWindow.spectatorWindowList;
                specList.Add( new SpectatorWindow( controlWindow ) );
                specList[ specList.Count - 1 ].Show();
                controlWindow.liftingOrder.UpdateSpectator( specList[ specList.Count - 1 ] );
                controlWindow.ConfigureSpecDataGrid( specList[ specList.Count - 1 ].specDg );
            }
        }

        private void ActiveGroupCob_OnSelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            controlWindow.controlDgCollection.Clear();
            controlWindow.specDgCollection.Clear();

            if( ActiveGroupCob.SelectedIndex < 0 )
                controlWindow.currentGroupIndex = 0;
            else
                controlWindow.currentGroupIndex = ActiveGroupCob.SelectedIndex;

            for( int i = 0 ; i < controlWindow.Lifters.Count ; i++ )
            {
                if( controlWindow.Lifters[ i ].groupNumber - 1 == ActiveGroupCob.SelectedIndex ) // To not start at index 0
                {
                    var collection = new DgFormat(controlWindow.Lifters[i]);
                    controlWindow.controlDgCollection.Add( collection );
                    controlWindow.specDgCollection.Add( collection );
                }
            }

            // Update :
            // AutoScaleDataGrid
            // Color grids
            // liftingOrder
            // selectedLifterIndex
            // lifterInfo
            // nextGroupLiftOrder

            controlWindow.controlDg.AutoScaleDataGrid();

            controlWindow.judgeControl.ColorWholeDataGrid();

            controlWindow.liftingOrder.LiftingOrderList.Clear();
            controlWindow.liftingOrder.UpdateAll( controlWindow );

            controlWindow.SelectedLifterIndex = controlWindow.liftingOrder.LiftingOrderList[ 0 ].index;

            controlWindow.lifterInfo1.Update( controlWindow.liftingOrder.LiftingOrderList[ 0 ] );
            controlWindow.lifterInfo2.Update( controlWindow.liftingOrder.LiftingOrderList[ 1 ] );

            foreach( SpectatorWindow window in controlWindow.spectatorWindowList )
            {
                window.specDg.AutoScaleDataGrid();

                window.lifterInfo1.Update( controlWindow.liftingOrder.LiftingOrderList[ 0 ] );
                window.lifterInfo2.Update( controlWindow.liftingOrder.LiftingOrderList[ 1 ] );

                window.nextGroupOrderSpec.GroupLiftOrderUpdate( controlWindow );
            }
        }

        private void HeightCob_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( controlWindow != null &&
        controlWindow.SelectedLifterIndex >= 0 &&
        controlWindow.SelectedLifterIndex < controlWindow.Lifters.Count )
            {

                Lifter lifter = controlWindow.Lifters[ controlWindow.SelectedLifterIndex ];
                if( lifter.currentLiftType < Lifter.eLiftType.B1 )
                {
                    OtherOptionsHeightTb.Text = "Squat\nHeight"; // Fix this shit only update text when changing wtf bro
                    lifter.squatHeight = ( int )HeightCob.SelectedIndex;
                }
                else
                {
                    OtherOptionsHeightTb.Text = "Bench\nHeight";
                    lifter.benchHeight = ( int )HeightCob.SelectedIndex;
                }

                int withinGroupLifterIndex = 0;
                for( int i = 0 ; i < controlWindow.groupDataList[ controlWindow.currentGroupIndex ].lifters.Count ; i++ )
                    if( controlWindow.groupDataList[ lifter.groupNumber - 1 ].lifters[ i ] == lifter )
                    {
                        withinGroupLifterIndex = i;
                        break;
                    }

                controlWindow.controlDgCollection[ withinGroupLifterIndex ].UpdateFromLifter();
                controlWindow.specDgCollection[ withinGroupLifterIndex ].UpdateFromLifter();

                controlWindow.liftingOrder.UpdateControlInfoPanels( controlWindow );
                foreach( var specWindow in controlWindow.spectatorWindowList )
                    controlWindow.liftingOrder.UpdateSpectatorInfoPanels( specWindow );
            }
        }

        private void RackCob_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( controlWindow != null &&
        controlWindow.SelectedLifterIndex >= 0 &&
        controlWindow.SelectedLifterIndex < controlWindow.Lifters.Count )
            {
                Lifter lifter = controlWindow.Lifters[ controlWindow.SelectedLifterIndex ];
                lifter.benchRack = ( int )RackCob.SelectedIndex;

                int withinGroupLifterIndex = 0;
                for( int i = 0 ; i < controlWindow.groupDataList[ controlWindow.currentGroupIndex ].lifters.Count ; i++ )
                    if( controlWindow.groupDataList[ lifter.groupNumber - 1 ].lifters[ i ] == lifter )
                    {
                        withinGroupLifterIndex = i;
                        break;
                    }

                controlWindow.controlDgCollection[ withinGroupLifterIndex ].UpdateFromLifter();
                controlWindow.specDgCollection[ withinGroupLifterIndex ].UpdateFromLifter();

                controlWindow.liftingOrder.UpdateControlInfoPanels( controlWindow );
                foreach( var specWindow in controlWindow.spectatorWindowList )
                    controlWindow.liftingOrder.UpdateSpectatorInfoPanels( specWindow );
            }
        }
    }
}
