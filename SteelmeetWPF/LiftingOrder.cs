using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SteelmeetWPF
{
    public class LiftingOrder
    {
        private bool controlLoaded = false;
        private bool spectatorLoaded = false;

        public List<Lifter> LiftingOrderList = new List<Lifter>();
        public List<TextBlock> ControlTbList = new List<TextBlock>();
        public List<TextBlock> SpectatorTbList = new List<TextBlock>();
        
        public void UpdateAll( ControlWindow window )
        {
            UpdateLiftingorder( window );
            UpdateControl( window );
            foreach(SpectatorWindow specWindow in window.spectatorWindowList )
                UpdateSpectator( specWindow );
        }

        public void UpdateLiftingorder( ControlWindow window )
        {
            ControlWindow controlWindow = window;
            List<Lifter> liftersInCurrentGroup = new List<Lifter>();

            int currentGroupIndex = controlWindow.currentGroupIndex;
            int nextGroupIndex = 0;
            nextGroupIndex = ( controlWindow.currentGroupIndex + 1 ) % controlWindow.groupDataList.Count;
            liftersInCurrentGroup = controlWindow.groupDataList[ currentGroupIndex ].lifters;

            Lifter.eLiftType lowestLiftType = 0;
            if( controlWindow.groupDataList.Count > 0 )
                lowestLiftType = liftersInCurrentGroup.Min( lifter => lifter.currentLiftType );

            LiftingOrderList.Clear();

            for( int i = 0 ; i < controlWindow.groupDataList[ currentGroupIndex ].count ; i++ ) 
            {
                if( liftersInCurrentGroup[ i ].currentLiftType == lowestLiftType )
                    LiftingOrderList.Add( liftersInCurrentGroup[ i ] );
            }

            // Sort 
            var comparer = new LifterComparer();
            LiftingOrderList = LiftingOrderList.OrderBy( item => item, comparer ).ToList();

            // Repeat to get the next lift type
            List<Lifter> ExtraLifters = new List<Lifter>();

            Lifter.eLiftType almostlowestLiftType = 0;
            if( controlWindow.groupDataList.Count > 0 )
                almostlowestLiftType = liftersInCurrentGroup.Min( lifter => lifter.currentLiftType + 1 );

            for( int i = 0 ; i < controlWindow.groupDataList[ currentGroupIndex ].count ; i++ )
                if( liftersInCurrentGroup[ i ].currentLiftType == almostlowestLiftType )
                    ExtraLifters.Add( liftersInCurrentGroup[ i ] );

            ExtraLifters = ExtraLifters.OrderBy( item => item, comparer ).ToList();
            LiftingOrderList.AddRange( ExtraLifters );
        }

        void UpdateControl( ControlWindow controlWindow )
        {
            if( !controlLoaded )
            {
                ControlTbList.Clear();
                ControlTbList.AddRange( controlWindow.liftingOrderControl.GetTextBlocks() );

                controlLoaded = true;
            }

            for( int i = 0 ; i < ControlTbList.Count ; i++ )
                ControlTbList[ i ].Text = "";

            for( int i = 0; i < LiftingOrderList.Count && i < ControlTbList.Count ; i++ )
            {
                string spacing = " ";
                string spacingIndex = "";
                float value = 0.0f;

                if( i < LiftingOrderList.Count() )
                    if( LiftingOrderList[ i ].currentLiftType != Lifter.eLiftType.Done )
                        value = LiftingOrderList[ i ].sbdWeightsList[ ( int )LiftingOrderList[ i ].currentLiftType ];

                string text = value.ToString();

                if( value <= 100.0f )
                    spacing += "  ";

                if( !text.Contains( ".5" ) )
                    spacing += "   ";

                if( i >= 10 )
                    spacingIndex = "| ";
                else
                    spacingIndex = "  | ";

                if( value == 0.0f )
                    ControlTbList[ i ].Text = "";
                else if( i < LiftingOrderList.Count )
                    ControlTbList[ i ].Text = i + spacingIndex + value + spacing + LiftingOrderList[ i ].name;
            }

            foreach( var row in controlWindow.controlDgCollection )
                     row.UpdateFromLifter();
        }

        void UpdateSpectator( SpectatorWindow spectatorWindow )
        {
            if( !spectatorLoaded )
            {
                SpectatorTbList.Clear();
                SpectatorTbList.AddRange( spectatorWindow.LiftingOrderSpec.GetTextBlocks() );

                spectatorLoaded = true;
            }

            for( int i = 0 ; i < SpectatorTbList.Count ; i++ )
                SpectatorTbList[ i ].Text = "";

            for( int i = 0 ; i < LiftingOrderList.Count && i < SpectatorTbList.Count ; i++ )
            {
                string spacing = " ";
                string spacingIndex = "";
                float value = 0.0f;

                if( i < LiftingOrderList.Count() )
                    if( LiftingOrderList[ i ].currentLiftType != Lifter.eLiftType.Done )
                        value = LiftingOrderList[ i ].sbdWeightsList[ ( int )LiftingOrderList[ i ].currentLiftType ];

                string text = value.ToString();

                if( value <= 100.0f )
                    spacing += "  ";

                if( !text.Contains( ".5" ) )
                    spacing += "   ";

                if( i >= 10 )
                    spacingIndex = "| ";
                else
                    spacingIndex = "  | ";

                if( value == 0.0f )
                    SpectatorTbList[ i ].Text = "";
                else if( i < LiftingOrderList.Count )
                    SpectatorTbList[ i ].Text = i + spacingIndex + value + spacing + LiftingOrderList[ i ].name;
            }
        }
        public void RemoveLifter(Lifter lifterToRemove, ControlWindow controlWindow)
        {
            LiftingOrderList.Remove( lifterToRemove );
            UpdateAll( controlWindow );
            // Play animation on lifter that is removed and move the whoe lifting order textblocks up

            // Get Textblock that corresponds to lifter

            // Animate that textblock so it goes to the side

            // Animate all blocks below to go up

            // Switch to to the positions that they were in before & Update lifting order same frame

            // Last lifter in orders textblock slides in from side

            // Done!!! :)
        }
    }
}
