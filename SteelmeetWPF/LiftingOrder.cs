using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

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

            UpdateControlInfoPanels(controlWindow);
        }

        public void UpdateControlInfoPanels( ControlWindow _controlWindow )
        {
            // Animate here?
            if( LiftingOrderList.Count > 0 )
                _controlWindow.lifterInfo1.Update( LiftingOrderList[ 0 ] );
            else
                _controlWindow.lifterInfo1.Visibility = Visibility.Hidden;

            if( LiftingOrderList.Count > 1 )
                _controlWindow.lifterInfo2.Update( LiftingOrderList[ 1 ] );
            else
                _controlWindow.lifterInfo2.Visibility = Visibility.Hidden;
        }

        public void UpdateSpectator( SpectatorWindow spectatorWindow )
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

            UpdateSpectatorInfoPanels( spectatorWindow );
        }

        public void UpdateSpectatorInfoPanels( SpectatorWindow spectatorWindow )
        {
            // Animate here?
            if( LiftingOrderList.Count > 0 )
                spectatorWindow.lifterInfo1.Update( LiftingOrderList[ 0 ] );
            else
                spectatorWindow.lifterInfo1.Visibility = Visibility.Hidden;

            if( LiftingOrderList.Count > 1 )
                spectatorWindow.lifterInfo2.Update( LiftingOrderList[ 1 ] );
            else
                spectatorWindow.lifterInfo2.Visibility = Visibility.Hidden;
        }

        public void RemoveLifter(Lifter lifterToRemove, ControlWindow controlWindow, List<SpectatorWindow> spectatorWindows)
        {
            foreach( SpectatorWindow spectatorWindow in spectatorWindows )
            {
                // Play animation for spectator window

                TextBlock lifterTextBlock = null;
                TextBlock[] textBlocks = spectatorWindow.LiftingOrderSpec.GetTextBlocks();

                for( int i = 0 ; i < textBlocks.Length ; i++ )
                    if( textBlocks[ i ].Text.Contains( lifterToRemove.name ) )
                        lifterTextBlock = textBlocks[ i ];

                if( lifterTextBlock == null )
                    break;

                // Animate that textblock so it goes to the side

                if( lifterTextBlock.RenderTransform == null || !( lifterTextBlock.RenderTransform is TranslateTransform ) )
                    lifterTextBlock.RenderTransform = new TranslateTransform();

                TranslateTransform translateTransform = lifterTextBlock.RenderTransform as TranslateTransform;

                if( translateTransform != null )
                {
                    double from = translateTransform.X;
                    double to = translateTransform.X + 400;

                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = from,
                        To = to,
                        Duration = new Duration(TimeSpan.FromMilliseconds(250))
                    };

                    animation.Completed += ( s, e ) =>
                    {
                        // Animate all blocks below to go up

                        int removedIndex = Array.IndexOf(textBlocks, lifterTextBlock);
                        int blocksToMove = textBlocks.Length - removedIndex - 1;
                        int completedCount = 0;

                        for( int i = removedIndex + 1 ; i < textBlocks.Length ; i++ )
                        {
                            TextBlock blockBelow = textBlocks[i];

                            if( blockBelow.RenderTransform == null || !( blockBelow.RenderTransform is TranslateTransform ) )
                                blockBelow.RenderTransform = new TranslateTransform();

                            TranslateTransform translateTransform2 = blockBelow.RenderTransform as TranslateTransform;

                            double fromY = translateTransform2.Y;
                            double toY = translateTransform2.Y - 40;

                            DoubleAnimation moveUpAnimation = new DoubleAnimation
                            {
                                From = fromY,
                                To = toY,
                                Duration = new Duration(TimeSpan.FromMilliseconds(250))
                            };

                            moveUpAnimation.Completed += ( s, e ) =>
                            {
                                completedCount++;

                                if( completedCount == blocksToMove )
                                {
                                    foreach( TextBlock tb in textBlocks )
                                    {
                                        if( tb.RenderTransform is TranslateTransform tt )
                                        {
                                            tt.ApplyAnimationClock( TranslateTransform.XProperty, null );
                                            tt.ApplyAnimationClock( TranslateTransform.YProperty, null );
                                            tt.X = 0;
                                            tt.Y = 0;
                                        }
                                    }

                                    foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                                        UpdateSpectator( specWindow );
                                }
                            };

                            translateTransform2.BeginAnimation( TranslateTransform.YProperty, moveUpAnimation );
                        }
                    };

                    translateTransform.BeginAnimation( TranslateTransform.XProperty, animation );
                }



                // Last lifter in orders textblock slides in from side

                // Done!!! >:)

            }

            UpdateLiftingorder( controlWindow );
            UpdateControl( controlWindow );
            LiftingOrderList.Remove( lifterToRemove );
        }
    }
}
