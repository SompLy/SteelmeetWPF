using DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

// TODO :
// Lifting orderupdate  DONE
// BesSBDUpdate         DONE
// GLPointsCalculator   DONE
// TimerController
// RankUpdate           DONE
// Set current cell?
// EstimatedUpdate      DOINE
// Mark the row of the current lifter in the datagrid

namespace SteelmeetWPF
{
    public partial class JudgeControl : UserControl
    {
        ControlWindow controlWindow;

        bool isRecord = false;

        public JudgeControl()
        {
            InitializeComponent();

            Loaded += JudgeControlLoaded;
        }

        private void JudgeControlLoaded( object sender, RoutedEventArgs e )
        {
            controlWindow = Window.GetWindow(this) as ControlWindow;
        }

        public void JudgeLift( bool isLiftGood )
        {
            if( controlWindow.controlDgCollection.Count < 1 )
                MessageBox.Show( "Lyftare saknas :(, starta en tävling först!", "⚠SteelMeet varning!⚠" );

            var liftingOrder = controlWindow.liftingOrder;
            var selectedLifter = controlWindow.Lifters[ controlWindow.SelectedLifterIndex ];

            if( !liftingOrder.LiftingOrderList.Contains( selectedLifter ) )
            {
                MessageBox.Show( "Denna lyftare har redan lyft denna omgång", "⚠SteelMeet varning!⚠", MessageBoxButton.OK, MessageBoxImage.None );
                return;
            }

            if( selectedLifter.currentLiftType == Lifter.eLiftType.Done )
            {
                MessageBox.Show( "Denna lyftare är klar", "⚠SteelMeet varning!⚠", MessageBoxButton.OK, MessageBoxImage.None );
                return;
            }

            // Set colors
            System.Windows.Media.Color backColor = isLiftGood ? Colors.ForestGreen : Colors.Red;
            System.Windows.Media.Color foreColor = System.Windows.Media.Color.FromArgb( 255, 187, 225, 250);
            ColorDataGridCell( selectedLifter.currentLiftType, selectedLifter, backColor, foreColor );

            backColor = Colors.White;
            foreColor = Colors.Black;
            ColorDataGridCell( selectedLifter.currentLiftType + 1, selectedLifter, backColor, foreColor );

            //dataGridViewControlPanel.CurrentCell = dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ selectedLifter.CurrentLift ];

            if( selectedLifter.currentLiftType != Lifter.eLiftType.S3 &&
                selectedLifter.currentLiftType != Lifter.eLiftType.B3 &&
                selectedLifter.currentLiftType != Lifter.eLiftType.D3 )
            {
                if( isLiftGood )
                    selectedLifter.sbdWeightsList[ ( int )selectedLifter.currentLiftType + 1 ] = selectedLifter.sbdWeightsList[ ( int )selectedLifter.currentLiftType ] + 2.5f;
                else
                    selectedLifter.sbdWeightsList[ ( int )selectedLifter.currentLiftType + 1 ] = selectedLifter.sbdWeightsList[ ( int )selectedLifter.currentLiftType ];
            }

            //dataGridViewControlPanel.CurrentCell = dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ 1 ];

            // Markerar rad för den aktiva lyftaren
            //for( int columnIndex = 2 ; columnIndex <= 5 ; columnIndex++ )
            //      dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ columnIndex ].Selected = true;

            // Update Other
            controlWindow.timerControl.TimerController( TimerControl.TimerOptions.LAPP_ONE_MINUTE ); //Startar lapp timern på 1 minut
            controlWindow.timerControl.TimerController( TimerControl.TimerOptions.LIFT_RESET ); //Stoppar lyft timern och sätter timern på 00:00
            isRecord = false;
            //RecordUpdate();
            //InfopanelsUpdate();
            controlWindow.weightInDg.AutoScaleDataGrid();

            // Update stats
            selectedLifter.LiftRecord[ ( int )selectedLifter.currentLiftType ] = isLiftGood;
            selectedLifter.BestSBDUpdate();
            selectedLifter.total = selectedLifter.bestS + selectedLifter.bestB + selectedLifter.bestD;
            selectedLifter.pointsGL = ( double )Math.Round(selectedLifter.CalculateGLPoints( selectedLifter.total ), 2);
            selectedLifter.RankUpdate( controlWindow );
            selectedLifter.isRetrying = false;
            selectedLifter.EstimatedUpdate();

            // Increase current lift
            if( selectedLifter.isBenchOnly && selectedLifter.currentLiftType == Lifter.eLiftType.D1 )
                selectedLifter.currentLiftType = Lifter.eLiftType.Done;
            else
                selectedLifter.currentLiftType += 1;

            // Needs to increment currentLiftType before updating liftingOrder
            liftingOrder.RemoveLifter( selectedLifter, controlWindow, controlWindow.spectatorWindowList);
            controlWindow.SelectedLifterIndex = liftingOrder.LiftingOrderList[ 0 ].index;
        }

        private DataGridCell GetCell( DataGridRow row, int columnIndex )
        {
            DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);
            if( presenter == null )
            {
                row.ApplyTemplate();
                presenter = FindVisualChild<DataGridCellsPresenter>( row );
            }
            DataGridCell cell = presenter?.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
            return cell;
        }

        private T FindVisualChild<T>( DependencyObject obj ) where T : DependencyObject
        {
            for( int i = 0 ; i < VisualTreeHelper.GetChildrenCount( obj ) ; i++ )
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if( child is T childOfType )
                    return childOfType;
                T childOfChild = FindVisualChild<T>(child);
                if( childOfChild != null )
                    return childOfChild;
            }
            return null;
        }

        int GetStartingCell( string columnName ) 
        {
            var dataGrid = controlWindow.controlDg;

            int columnIndex = -1;
            for( int i = 0 ; i < dataGrid.Columns.Count ; i++ )
            {
                if( dataGrid.Columns[ i ] is DataGridBoundColumn col &&
                    col.Binding is System.Windows.Data.Binding b &&
                    b.Path.Path == columnName )
                {
                    columnIndex = i;
                    break;
                }
            }
            return columnIndex;
        }

        private void ColorDataGridCell( Lifter.eLiftType lifttype, Lifter selectedLifter, System.Windows.Media.Color backgroundColor, System.Windows.Media.Color foregroundColor ) // Gör så denna jävel funkar
        {
            // Where does the lift start in the datagrid then add lifttype and you have your correct cell
            var dataGrid = controlWindow.controlDg;
            int startingCell = GetStartingCell( "S1" );

            int withinGroupLifterIndex = 0;
            for( int i = 0 ; i < controlWindow.groupDataList[ controlWindow.currentGroupIndex ].lifters.Count ; i++ )
                if( controlWindow.groupDataList[ selectedLifter.groupNumber - 1 ].lifters[ i ] == selectedLifter )
                {
                    withinGroupLifterIndex = i;
                    break;
                }

            // Control Window
            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex( withinGroupLifterIndex );
            if( row != null )
            {
                var cell = GetCell(row, startingCell + (int)lifttype);
                if( cell != null )
                {
                    cell.Background = new SolidColorBrush( backgroundColor );
                    cell.Foreground = new SolidColorBrush( foregroundColor );
                }
            }

            // Spectator Window
            foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
            {
                dataGrid = specWindow.specDg;

                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex( withinGroupLifterIndex );
                if( row != null )
                {
                    var cell = GetCell(row, startingCell + (int)lifttype);
                    if( cell != null )
                    {
                        cell.Background = new SolidColorBrush( backgroundColor );
                        cell.Foreground = new SolidColorBrush( foregroundColor );
                    }
                }
            }
        }

        private void SelectNextLifter()
        {
            if ( controlWindow.liftingOrder.LiftingOrderList.Count > 0 )
                controlWindow.SelectedLifterIndex = controlWindow.liftingOrder.LiftingOrderList[ 0 ].index;
        }

        public void undoLift( bool _isRetrying )
        {
            if( controlWindow.controlDgCollection.Count < 1 )
                MessageBox.Show( "Lyftare saknas :(, starta en tävling först!", "⚠SteelMeet varning!⚠" );

            var liftingOrder = controlWindow.liftingOrder;
            var selectedLifter = controlWindow.Lifters[ controlWindow.SelectedLifterIndex ];

            if( selectedLifter.isBenchOnly && selectedLifter.currentLiftType == Lifter.eLiftType.B1 ||
                selectedLifter.currentLiftType == Lifter.eLiftType.S1 )
                return;

            // Set colors
            System.Windows.Media.Color backColor = Colors.White;
            System.Windows.Media.Color foreColor = Colors.Black;
            ColorDataGridCell( selectedLifter.currentLiftType - 1, selectedLifter, backColor, foreColor );

            if( selectedLifter.currentLiftType != Lifter.eLiftType.S1 &&
                selectedLifter.currentLiftType != Lifter.eLiftType.B1 &&
                selectedLifter.currentLiftType != Lifter.eLiftType.D1 )
                    selectedLifter.sbdWeightsList[ ( int )selectedLifter.currentLiftType ] = 0.0f;

            // Markerar rad för den aktiva lyftaren
            //for( int columnIndex = 2 ; columnIndex <= 5 ; columnIndex++ )
            //      dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ columnIndex ].Selected = true;

            // Update Other
            isRecord = false;
            //RecordUpdate();

            // Update stats
            selectedLifter.LiftRecord[ ( int )selectedLifter.currentLiftType ] = false;
            selectedLifter.BestSBDUpdate();
            selectedLifter.total = selectedLifter.bestS + selectedLifter.bestB + selectedLifter.bestD;
            selectedLifter.pointsGL = ( double )Math.Round( selectedLifter.CalculateGLPoints( selectedLifter.total ), 2 );
            selectedLifter.RankUpdate( controlWindow );
            selectedLifter.isRetrying = false;
            selectedLifter.EstimatedUpdate();

            // Decrement current lift
            if( selectedLifter.isBenchOnly && selectedLifter.currentLiftType == Lifter.eLiftType.Done )
                selectedLifter.currentLiftType = Lifter.eLiftType.B3;
            else
                selectedLifter.currentLiftType -= 1;

            // Needs to decrement currentLiftType before updating liftingOrder
            liftingOrder.RemoveLifter( selectedLifter, controlWindow, controlWindow.spectatorWindowList );
            if ( liftingOrder.LiftingOrderList.Count > 0 )
                controlWindow.SelectedLifterIndex = liftingOrder.LiftingOrderList[ 0 ].index;

            //        public void undoLift( bool _isRetrying )
            //{
            //    if( LifterID[ SelectedRowIndex + groupRowFixer ].isBenchOnly && LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift == 13 )
            //        return;

            //    if( LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift > firstLiftColumn )
            //    {
            //        if( _isRetrying )
            //            LifterID[ SelectedRowIndex + groupRowFixer ].isRetrying = true;

            //        LiftingOrderList.Add( LifterID[ SelectedRowIndex + groupRowFixer ] );

            //        LiftingOrderUpdate();// Updaterar lyftar ordning
            //        LifterID[ SelectedRowIndex + groupRowFixer ].isRetrying = true;
            //        // Ångarar ett lyft för lyftaren i LiftRecord
            //        // Lift record håller koll på vilka av lyften som lyftaren gjort har blivit godkända eller underkända i boolformat
            //        LifterID[ SelectedRowIndex + groupRowFixer ].LiftRecord.RemoveAt( LifterID[ SelectedRowIndex + groupRowFixer ].LiftRecord.Count - 1 );

            //        if( LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift != 13 && LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift != 16 )
            //            dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift ].Value = 0;

            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift ].Style.BackColor = Color.Empty;
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift ].Style.ForeColor = Color.FromArgb( 187, 225, 250 );
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift - 1 ].Style.BackColor = currentLiftColor;
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift - 1 ].Style.ForeColor = Color.Black;
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift - 1 ].Style.Font = new System.Drawing.Font( "Segoe UI", 10f, FontStyle.Regular );
            //        LifterID[ SelectedRowIndex + groupRowFixer ].CurrentLift -= 1;

            //        // Uppdaterar total och GLpoints
            //        LiftingOrderList[ 0 ].total = LiftingOrderList[ 0 ].bestS + LiftingOrderList[ 0 ].bestB + LiftingOrderList[ 0 ].bestD;
            //        LiftingOrderList[ 0 ].pointsGL = GLPointsCalculator( LiftingOrderList[ 0 ], LiftingOrderList[ 0 ].total );
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ 19 ].Value = LiftingOrderList[ 0 ].total;
            //        dataGridViewControlPanel.Rows[ SelectedRowIndex ].Cells[ 21 ].Value = LiftingOrderList[ 0 ].pointsGL.ToString( "0.00" );

            //    }
            //    InfopanelsUpdate();
            //}
        }

        private void BarReadyBtn_Click( object sender, RoutedEventArgs e )
        {
            //TimerController( 0 );
            if( controlWindow.liftingOrder.LiftingOrderList.Count > 0 )
            {
                controlWindow.timerControl.TimerController( TimerControl.TimerOptions.LIFT_ONE_MINUTE );
                SelectNextLifter();
            }
        }

        private void SelectNextLifterBtn_Click( object sender, RoutedEventArgs e )
        {
            SelectNextLifter();
        }

        private void GoodLiftBtn_Click( object sender, RoutedEventArgs e )
        {
            JudgeLift( true );
        }

        private void BadLiftBtn_Click( object sender, RoutedEventArgs e )
        {
            JudgeLift( false );
        }

        private void RetryBtn_Click( object sender, RoutedEventArgs e )
        {
            undoLift( true );
        }

        private void UndoBtn_Click( object sender, RoutedEventArgs e )
        {
            undoLift( false );
        }
    }
}
