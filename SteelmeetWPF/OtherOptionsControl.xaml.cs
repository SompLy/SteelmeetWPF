using System;
using System.Windows;
using System.Windows.Controls;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for OtherOptionsControl.xaml
    /// </summary>
    public partial class OtherOptionsControl : UserControl
    {
        private ControlWindow controlWindow = null;

        public OtherOptionsControl()
        {
            InitializeComponent();

            Loaded += OtherOptionsControlLoaded;
        }

        private void OtherOptionsControlLoaded(object sender, RoutedEventArgs e)
        {
            controlWindow = Window.GetWindow(this) as ControlWindow;
            ActiveGroupCob.Items.Clear();

            if (controlWindow == null)
                return;

            for (int i = 0; i < controlWindow.groupDataList.Count; i++)
                ActiveGroupCob.Items.Add(i + 1); // To not start at index 0
            
            ActiveGroupCob.SelectedIndex = 0;
        }

        private void OpenSpectatorWindowBtn_OnClick(object sender, RoutedEventArgs e)
        {
            controlWindow = Window.GetWindow( this ) as ControlWindow;
            if (controlWindow != null)
            {
                var specList = controlWindow.spectatorWindowList;
                specList.Add( new SpectatorWindow( controlWindow ) );
                specList[ specList.Count - 1 ].Show();
                controlWindow.liftingOrder.UpdateSpectator( specList[ specList.Count - 1 ] );
            }
        }

        private void ActiveGroupCob_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            controlWindow.controlDgCollection.Clear();

            if (ActiveGroupCob.SelectedIndex < 0)
                controlWindow.currentGroupIndex = 0;
            else
                controlWindow.currentGroupIndex = ActiveGroupCob.SelectedIndex;

            for( int i = 0 ; i < controlWindow.Lifters.Count ; i++ )
            {
                if( controlWindow.Lifters[ i ].groupNumber - 1 == ActiveGroupCob.SelectedIndex) // To not start at index 0
                {
                    var collection = new ControlDgFormat(controlWindow.Lifters[i]);
                    controlWindow.controlDgCollection.Add( collection );
                }
            }

            controlWindow.liftingOrder.LiftingOrderList.Clear();
            controlWindow.liftingOrder.UpdateAll( controlWindow );
            controlWindow.selectedLifterIndex = controlWindow.liftingOrder.LiftingOrderList[ 0 ].index;
            foreach( SpectatorWindow window in controlWindow.spectatorWindowList )
                window.nextGroupOrderSpec.GroupLiftOrderUpdate(controlWindow);
        }
    }
}
