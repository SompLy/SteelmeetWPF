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

        public TextBlock[] GetTextBlocks()
        {
            return new TextBlock[] { L0Tb, L1Tb, L2Tb, L3Tb, L4Tb, L5Tb, L6Tb, L7Tb, L8Tb, L9Tb, L10Tb, L11Tb, L12Tb, L13Tb, L14Tb, L15Tb, L16Tb, L17Tb, L18Tb, L19Tb };
        }
    }
}
