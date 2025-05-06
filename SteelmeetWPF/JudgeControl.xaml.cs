using System;
using System.Collections.Generic;
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
// InfopanelsUpdate, ska nog inte behövas om det bindas rätt
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
            var selectedLifter = controlWindow.Lifters[ controlWindow.selectedLifterIndex ];

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
            Color backColor = isLiftGood ? Colors.ForestGreen : Colors.Red;
            Color foreColor = Color.FromArgb( 255, 187, 225, 250);
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
            //TimerController( 2 ); //Startar lapp timern på 1 minut
            //TimerController( 3 ); //Stoppar lyft timern och sätter timern på 00:00
            isRecord = false;
            //RecordUpdate();
            //InfopanelsUpdate();

            // Update stats
            selectedLifter.LiftRecord[ ( int )selectedLifter.currentLiftType ] = isLiftGood;
            selectedLifter.BestSBDUpdate();
            selectedLifter.total = selectedLifter.bestS + selectedLifter.bestB + selectedLifter.bestD;
            selectedLifter.CalculateGLPoints( selectedLifter.total );
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
            controlWindow.selectedLifterIndex = liftingOrder.LiftingOrderList[ 0 ].index;
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

        private void ColorDataGridCell( Lifter.eLiftType lifttype, Lifter selectedLifter, Color backgroundColor, Color foregroundColor ) // Gör så denna jävel funkar
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
            //if( liftingOrder.Count > 0 )
            //{
            //    dataGridViewControlPanel.CurrentCell = dataGridViewControlPanel.Rows[ liftingOrder[ 0 ].index - groupRowFixer ].Cells[ 1 ];
            //    // Markerar rad för den aktiva lyftaren
            //    for( int columnIndex = 2 ; columnIndex <= 5 ; columnIndex++ )
            //        dataGridViewControlPanel.Rows[ liftingOrder[ 0 ].index - groupRowFixer ].Cells[ columnIndex ].Selected = true;
            //
            //    // Uppdaterar platcalculatorn för den buggar ibland
            //    // Om gruppen är klar
            //    if( liftingOrder[ 0 ].CurrentLift - firstLiftColumn <= 8 )
            //        PlateCalculator( liftingOrder[ 0 ].sbdList[ liftingOrder[ 0 ].CurrentLift - firstLiftColumn ], plateInfo );
            //    // Om gruppen är klar
            //    if( liftingOrder.Count > 1 && liftingOrder[ 1 ].CurrentLift - firstLiftColumn <= 8 )
            //        PlateCalculator2( liftingOrder[ 1 ].sbdList[ liftingOrder[ 1 ].CurrentLift - firstLiftColumn ], plateInfo );
            //
            //    InfopanelsUpdate();
            //}
        }
        public void undoLift( bool _isRetrying )
        { 
        }

        private void BarReadyBtn_Click( object sender, RoutedEventArgs e )
        {
            //TimerController( 0 );
            //if( liftingOrder.Count > 0 )
                SelectNextLifter();
        }

        private void SelectNextLifterBtn_Click( object sender, RoutedEventArgs e )
        {
            SelectNextLifter();
        }

        private void RetryBtn_Click( object sender, RoutedEventArgs e )
        {
            undoLift( true );
            //LiftingOrderUpdate();
        }

        private void GoodLiftBtn_Click( object sender, RoutedEventArgs e )
        {
            JudgeLift( true );
        }

        private void BadLiftBtn_Click( object sender, RoutedEventArgs e )
        {
            JudgeLift( false );
        }

        private void UndoBtn_Click( object sender, RoutedEventArgs e )
        {
            undoLift( false );
        }
    }
}
