using DocumentFormat.OpenXml.InkML;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for SuggestedWeightControl.xaml
    /// </summary>
    public partial class SuggestedWeightControl : UserControl
    {
        private Lifter lifter = null;
        private ControlWindow controlWindow = null;

        public SuggestedWeightControl()
        {
            InitializeComponent();
        }

        public void Update( Lifter _lifter, ControlWindow _controlWindow, bool shouldUpdateLiftingOrder = false ) 
        {
            lifter = _lifter;
            controlWindow = _controlWindow;

            if( lifter.currentLiftType == Lifter.eLiftType.Done )
                return;

            float baseWeight = lifter.sbdWeightsList[ ( int )lifter.currentLiftType ];
            float[] weightIncrements = { 2.5f, 5.0f, 7.5f, 10.0f, 12.5f, 15.0f, 17.5f, 20f, 25.0f };

            _2_5Btn.Content  = ( baseWeight + weightIncrements[ 0 ] ).ToString();
            _5Btn.Content    = ( baseWeight + weightIncrements[ 1 ] ).ToString();
            _7_5Btn.Content  = ( baseWeight + weightIncrements[ 2 ] ).ToString();
            _10Btn.Content   = ( baseWeight + weightIncrements[ 3 ] ).ToString();
            _12_5Btn.Content = ( baseWeight + weightIncrements[ 4 ] ).ToString();
            _15Btn.Content   = ( baseWeight + weightIncrements[ 5 ] ).ToString();
            _17_5Btn.Content = ( baseWeight + weightIncrements[ 6 ] ).ToString();
            _20Btn.Content   = ( baseWeight + weightIncrements[ 7 ] ).ToString();
            _25Btn.Content   = ( baseWeight + weightIncrements[ 8 ] ).ToString();

            __2_5Btn.Content = ( baseWeight - weightIncrements[ 0 ] ).ToString();
            __5Btn.Content   = ( baseWeight - weightIncrements[ 1 ] ).ToString();
            __7_5Btn.Content = ( baseWeight - weightIncrements[ 2 ] ).ToString();

            if ( !shouldUpdateLiftingOrder ) // Fattar inte varför i helvette denna är reversed men det funkar
                _controlWindow.liftingOrder.UpdateAll( _controlWindow );
        }

        private void Suggested_Btn_Click(object sender, RoutedEventArgs e)
        {
            if( lifter == null )
                return;
            if( lifter.currentLiftType == Lifter.eLiftType.Done )
                return;

            if( sender is Button button ) 
            {
                float increment = float.Parse(button.Tag.ToString(), CultureInfo.InvariantCulture);

                lifter.sbdWeightsList[ ( int )lifter.currentLiftType ] += increment;
            }

            Update( lifter, controlWindow );
        }
    }
}
