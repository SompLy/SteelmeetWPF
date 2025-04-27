using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for NextGroupOrderSpectator.xaml
    /// </summary>
    public partial class NextGroupOrderSpectator : UserControl
    {
        private bool loaded = false;

        public const float NextGroupMarginOffsetPerLifter = -39.65f;

        bool viewNothing = false;
        public List<TextBlock> LiftersInNextGroupTextBoxes = new List<TextBlock>();

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "TitleGroup",
                typeof(string),
                typeof(NextGroupOrderSpectator),
                new PropertyMetadata("Lyftar Ordning"));

        public string TitleGroup
        {
            get { return ( string )GetValue( TitleProperty ); }
            set { SetValue( TitleProperty, value ); }
        }

        public NextGroupOrderSpectator()
        {
            InitializeComponent();
        }

        public void GroupLiftOrderUpdate( ControlWindow window )
        {
            if( !loaded )
            {
                LiftersInNextGroupTextBoxes.Clear();

                LiftersInNextGroupTextBoxes.AddRange( new TextBlock[] {
                L0Tb, L1Tb, L2Tb, L3Tb, L4Tb, L5Tb, L6Tb, L7Tb, L8Tb, L9Tb, L10Tb,
                L11Tb, L12Tb, L13Tb, L14Tb, L15Tb, L16Tb, L17Tb, L18Tb, L19Tb } );

                loaded = true;
            }

            ControlWindow controlWindow = window;

            for( int i = 0; i < LiftersInNextGroupTextBoxes.Count; i++ )
                LiftersInNextGroupTextBoxes[ i ].Text = "";

            List<Lifter> liftersInNextGroup = new List<Lifter>();

            liftersInNextGroup.Clear();

            int totalGroupCount = controlWindow.groupDataList.Count;
            int firstLifterInNextGroupIndex = 0;

            int nextGroupIndex = 0;
            nextGroupIndex = ( controlWindow.currentGroupIndex + 1 ) % totalGroupCount;

            liftersInNextGroup = controlWindow.groupDataList[ nextGroupIndex ].lifters;
            firstLifterInNextGroupIndex = liftersInNextGroup[0].index;

            // For each lifter in the next group, determine the lowest current lift
            Lifter.eLiftType nextGroupLiftType = 0;
            if( liftersInNextGroup.Count > 0 )
                nextGroupLiftType = liftersInNextGroup.Min( lifter => lifter.currentLiftType );

            foreach( SpectatorWindow specWindow in controlWindow.spectatorWindowList )
                specWindow.nextGroupOrderSpec.TitleGroup = "Grupp " + ( nextGroupIndex + 1 ) + " " + nextGroupLiftType.ToString();

            // Sort and Remove
            var comparer = new LifterComparer();
            liftersInNextGroup = liftersInNextGroup.OrderBy( item => item, comparer ).ToList();
            for( int i = 0 ; i < liftersInNextGroup.Count ; i++ )
            {
                if( liftersInNextGroup[ i ].currentLiftType > nextGroupLiftType )
                {
                    liftersInNextGroup.RemoveAt( i );
                }
            }

            viewNothing = liftersInNextGroup.Count > 0 && ( liftersInNextGroup[ 0 ].currentLiftType == Lifter.eLiftType.Done );

            if( !viewNothing )
            {
                for( int i = 0 ; i < liftersInNextGroup.Count ; i++ )
                {
                    string Spacing = " ";
                    string SpacingIndex = " ";
                    int currentLiftIndex = ( int )liftersInNextGroup[ i ].currentLiftType;
                    float value = liftersInNextGroup[i].sbdListWeight[currentLiftIndex];
                    string text = liftersInNextGroup[i].sbdListWeight[currentLiftIndex].ToString();

                    if( value <= 100.0f )
                        Spacing += "  ";

                    if( !text.Contains( ".5" ) )
                        Spacing += "   ";

                    if( i >= 9 )
                        SpacingIndex = "| ";
                    else
                        SpacingIndex = "  | ";

                    LiftersInNextGroupTextBoxes[ i ].Text = ( i + 1 ) + SpacingIndex + value + Spacing + liftersInNextGroup[ i ].name;
                }
            }
            else
            {
                for( int i = 0 ; i < liftersInNextGroup.Count ; i++ )
                    LiftersInNextGroupTextBoxes[ i ].Text = "";
            }

            SetNextGroupMargin( liftersInNextGroup.Count );
        }

        void SetNextGroupMargin( int liftersInNextGroupCount )
        {
            if( RenderTransform == null || !( RenderTransform is TranslateTransform ) )
            {
                RenderTransform = new TranslateTransform();
            }

            TranslateTransform translateTransform = RenderTransform as TranslateTransform;

            if( translateTransform != null )
            {
                double from = Margin.Top;
                double to = NextGroupMarginOffsetPerLifter * liftersInNextGroupCount;

                DoubleAnimation animation = new DoubleAnimation
                {
                    From = from,
                    To = to,
                    Duration = new Duration(TimeSpan.FromMilliseconds(1000))
                };

                translateTransform.BeginAnimation( TranslateTransform.YProperty, animation );
            }

        }
    }
}
