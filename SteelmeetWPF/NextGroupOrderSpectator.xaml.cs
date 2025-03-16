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
using static SteelmeetWPF.ControlWindow;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for NextGroupOrderSpectator.xaml
    /// </summary>
    public partial class NextGroupOrderSpectator : UserControl
    {
        public List<TextBlock> groupLiftingOrderListLabels = new List<TextBlock>();     // Order med lyftare och vikt de ska ta i rätt ordning.
        List<Lifter> groupLiftingOrderList = new List<Lifter>();                        // För att sortera viktera

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(string),
                typeof(LiftingOrderSpectator),
                new PropertyMetadata("Lyftar Ordning"));

        public string Title
        {
            get { return ( string )GetValue( TitleProperty ); }
            set { SetValue( TitleProperty, value ); }
        }

        public NextGroupOrderSpectator()
        {
            InitializeComponent();
        }

        public void GroupLiftOrderUpdate() // Updates the next group's lifts
        {
            ControlWindow controlWindow = Window.GetWindow( this ) as ControlWindow;
            // Sets labels if they are not initialized
            if( groupLiftingOrderListLabels.Count < 1 )
                groupLiftingOrderListLabels.AddRange( new TextBlock[] {
                    L0Tb, L1Tb, L2Tb, L3Tb, L4Tb, L5Tb, L6Tb, L7Tb, L8Tb, L9Tb, L10Tb,
                    L11Tb, L12Tb, L13Tb, L14Tb, L15Tb, L16Tb, L17Tb, L18Tb, L19Tb } );

            for( int i = 0 ; i < groupLiftingOrderListLabels.Count ; i++ )
                groupLiftingOrderListLabels[ i ].Text = "";

            // List to determine the lowest current lift in the active group
            List<int> lowestCurrentLiftInGroup = new List<int>();
            lowestCurrentLiftInGroup.Clear();

            // Initialize the start and end indices based on the current group
            int startIndex = 0;
            int endIndex = 0;

            // Update the group count and determine the current group indices
            GroupCountUpdater();

            // Calculate startIndex and endIndex based on currentGroupIndex
            int totalGroupCount = controlWindow.groupDataList.Count; // Get the total number of groups
            int groupStartIndex = 0;

            for( int i = 0 ; i < totalGroupCount ; i++ )
            {
                if( i == controlWindow.currentGroupIndex )
                {
                    groupStartIndex = i;
                    break;
                }
            }

            startIndex = groupStartIndex * 10;  // Assuming 10 lifters per group (adjust based on your scenario)
            endIndex = startIndex + 10;

            // For each lifter in the active group, determine the lowest current lift
            for( int i = startIndex ; i < endIndex ; i++ )
            {
                var lifter = controlWindow.Lifters[i];
                if( ( lifter.isBenchOnly && lifter.currentLift< 16 ) || !lifter.isBenchOnly )
                    lowestCurrentLiftInGroup.Add( lifter.currentLift);
            }

            int relevantCurrentLift = 0;
            if( lowestCurrentLiftInGroup.Count > 0 )
                relevantCurrentLift = lowestCurrentLiftInGroup.Min() - firstLiftColumn;

            // Set the initial group lifting order state
            eGroupLiftingOrderState groupLiftingOrderState = eGroupLiftingOrderState.group2Squat;

            // Determine which lifting state should be displayed based on relevantCurrentLift
            if( currentGroupIndex == 0 ) // Group 1
            {
                if( relevantCurrentLift < 3 ) groupLiftingOrderState = eGroupLiftingOrderState.group1Squat;
                else if( relevantCurrentLift < 6 ) groupLiftingOrderState = eGroupLiftingOrderState.group1Bench;
                else if( relevantCurrentLift < 9 ) groupLiftingOrderState = eGroupLiftingOrderState.group1Deadlift;
            }
            else if( currentGroupIndex == 1 ) // Group 2
            {
                if( relevantCurrentLift < 3 ) groupLiftingOrderState = eGroupLiftingOrderState.group2Squat;
                else if( relevantCurrentLift < 6 ) groupLiftingOrderState = eGroupLiftingOrderState.group2Bench;
                else if( relevantCurrentLift < 9 ) groupLiftingOrderState = eGroupLiftingOrderState.group2Deadlift;
            }
            else // Additional groups can be handled dynamically
            {
                if( relevantCurrentLift < 3 ) groupLiftingOrderState = eGroupLiftingOrderState.group3Squat;
                else if( relevantCurrentLift < 6 ) groupLiftingOrderState = eGroupLiftingOrderState.group3Bench;
                else if( relevantCurrentLift < 9 ) groupLiftingOrderState = eGroupLiftingOrderState.group3Deadlift;
            }

            // Variables for looping and displaying text
            int loopLeft = 0;
            int loopMiddle = 0;
            int textCurrentLift = 0;
            string lblText = "";
            bool ViewNothing = false;

            // Set the loop parameters and label text based on the lifting order state
            switch( groupLiftingOrderState )
            {
                case eGroupLiftingOrderState.group1Squat:
                    loopLeft = 0;
                    loopMiddle = group1Count;
                    textCurrentLift = 0;
                    lblText = "Ingångar : Grupp 1 Böj";
                    break;
                case eGroupLiftingOrderState.group1Bench:
                    loopLeft = 0;
                    loopMiddle = group1Count;
                    textCurrentLift = 3;
                    lblText = "Ingångar : Grupp 1 Bänk";
                    break;
                case eGroupLiftingOrderState.group1Deadlift:
                    loopLeft = 0;
                    loopMiddle = group1Count;
                    textCurrentLift = 6;
                    lblText = "Ingångar : Grupp 1 Mark";
                    break;
                case eGroupLiftingOrderState.group2Squat:
                    loopLeft = group1Count;
                    loopMiddle = group1Count + group2Count;
                    textCurrentLift = 0;
                    lblText = "Ingångar : Grupp 2 Böj";
                    break;
                case eGroupLiftingOrderState.group2Bench:
                    loopLeft = group1Count;
                    loopMiddle = group1Count + group2Count;
                    textCurrentLift = 3;
                    lblText = "Ingångar : Grupp 2 Bänk";
                    break;
                case eGroupLiftingOrderState.group2Deadlift:
                    loopLeft = group1Count;
                    loopMiddle = group1Count + group2Count;
                    textCurrentLift = 6;
                    lblText = "Ingångar : Grupp 2 Mark";
                    break;
                default:
                    break;
            }

            groupLiftingOrderList.Clear();
            for( int i = loopLeft ; i < loopMiddle ; i++ )
            {
                groupLiftingOrderList.Add( LifterID[ i ] );
            }

            // Sort the list based on the custom comparer
            var comparer = new LifterComparer();
            groupLiftingOrderList = groupLiftingOrderList.OrderBy( item => item, comparer ).ToList();

            // Remove all lifters who have a higher current lift than the lowest one
            var tempLowestCurrentLift = groupLiftingOrderList.Select(x => x.CurrentLift);
            if( tempLowestCurrentLift.Count() > 0 )
            {
                int low = tempLowestCurrentLift.Min();
                for( int i = 0 ; i < groupLiftingOrderList.Count ; )
                {
                    if( groupLiftingOrderList[ i ].CurrentLift > low )
                        groupLiftingOrderList.RemoveAt( i );
                    else
                        i++;
                }
            }

            // Display the lifting order text
            if( !ViewNothing )
                lbl_OpeningLift.Text = lblText;
            else
                lbl_OpeningLift.Text = "";

            // Display the order for all lifters
            if( !ViewNothing )
            {
                for( int i = 0 ; i < groupLiftingOrderList.Count ; i++ )
                {
                    string Spacing = " ";
                    string SpacingIndex = " ";
                    float value = groupLiftingOrderList[i].sbdList[textCurrentLift];
                    string text = groupLiftingOrderList[i].sbdList[textCurrentLift].ToString();

                    if( value <= 100.0f )
                        Spacing += "  ";

                    if( !text.Contains( ".5" ) )
                        Spacing += "   ";

                    if( i >= 9 )
                        SpacingIndex = "| ";
                    else
                        SpacingIndex = "  | ";

                    groupLiftingOrderListLabels[ i ].Text = ( i + 1 ) + SpacingIndex + groupLiftingOrderList[ i ].sbdList[ textCurrentLift ] + Spacing + groupLiftingOrderList[ i ].name;
                }
            }
            else
            {
                // If nothing is displayed, clear the labels
                for( int i = 0 ; i < groupLiftingOrderList.Count ; i++ )
                    groupLiftingOrderListLabels[ i ].Text = "";
            }
        }
    }
}
