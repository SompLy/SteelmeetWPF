using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static SteelmeetWPF.NextGroupOrderSpectator;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for LiftingOrderSpectator.xaml
    /// </summary>
    public partial class LiftingOrderSpectator : UserControl
    {
        private bool loaded = false;

        bool viewNothing = false;
        public List<TextBlock> LiftersInGroupTextBoxes = new List<TextBlock>();

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(LiftingOrderSpectator),
                new PropertyMetadata("Lyftar Ordning"));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public LiftingOrderSpectator()
        {
            InitializeComponent();
        }

        public void GroupLiftOrderUpdate( ControlWindow window )
        {
            //if( !loaded )
            //{
            //    LiftersInGroupTextBoxes.Clear();

            //    LiftersInGroupTextBoxes.AddRange( new TextBlock[] {
            //    L0Tb, L1Tb, L2Tb, L3Tb, L4Tb, L5Tb, L6Tb, L7Tb, L8Tb, L9Tb, L10Tb,
            //    L11Tb, L12Tb, L13Tb, L14Tb, L15Tb, L16Tb, L17Tb, L18Tb, L19Tb } );

            //    loaded = true;
            //}

            //ControlWindow controlWindow = window;

            //for( int i = 0 ; i < LiftersInGroupTextBoxes.Count ; i++ )
            //    LiftersInGroupTextBoxes[ i ].Text = "";

            //List<Lifter> liftersInGroup = new List<Lifter>();

            //liftersInGroup.Clear();

            //int totalGroupCount = controlWindow.groupDataList.Count;
            //int firstLifterInNextGroupIndex = 0;

            //liftersInGroup = controlWindow.groupDataList[ controlWindow.currentGroupIndex ].lifters;
            //firstLifterInNextGroupIndex = liftersInGroup[ 0 ].index;

            //// For each lifter in the next group, determine the lowest current lift
            //Lifter.eLiftType nextGroupLiftType = 0;
            //if( liftersInGroup.Count > 0 )
            //    nextGroupLiftType = liftersInGroup.Min( lifter => lifter.currentLiftType );

            //// Sort and Remove
            //var comparer = new LifterComparer();
            //liftersInGroup = liftersInGroup.OrderBy( item => item, comparer ).ToList();
            //for( int i = 0 ; i < liftersInGroup.Count ; i++ )
            //{
            //    if( liftersInGroup[ i ].currentLiftType > nextGroupLiftType )
            //    {
            //        liftersInGroup.RemoveAt( i );
            //    }
            //}

            //viewNothing = ( liftersInGroup.Count > 0 && ( liftersInGroup[ 0 ].currentLiftType == Lifter.eLiftType.None || liftersInGroup[ 0 ].currentLiftType == Lifter.eLiftType.Done ) );

            //if( !viewNothing )
            //{
            //    for( int i = 0 ; i < liftersInGroup.Count ; i++ )
            //    {
            //        string Spacing = " ";
            //        string SpacingIndex = " ";
            //        int currentLiftIndex = ( int )liftersInGroup[ i ].currentLiftType;
            //        float value = liftersInGroup[i].sbdListWeight[currentLiftIndex];
            //        string text = liftersInGroup[i].sbdListWeight[currentLiftIndex].ToString();

            //        if( value <= 100.0f )
            //            Spacing += "  ";

            //        if( !text.Contains( ".5" ) )
            //            Spacing += "   ";

            //        if( i >= 9 )
            //            SpacingIndex = "| ";
            //        else
            //            SpacingIndex = "  | ";

            //        LiftersInGroupTextBoxes[ i ].Text = ( i + 1 ) + SpacingIndex + value + Spacing + liftersInGroup[ i ].name;
            //    }
            //}
            //else
            //{
            //    for( int i = 0 ; i < liftersInGroup.Count ; i++ )
            //        LiftersInGroupTextBoxes[ i ].Text = "";
            //}
        }
    }
}
