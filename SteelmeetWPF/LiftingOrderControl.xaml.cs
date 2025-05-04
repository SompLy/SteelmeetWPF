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

namespace SteelmeetWPF
{
    public partial class LiftingOrderControl : UserControl
    {
        public LiftingOrderControl()
        {
            InitializeComponent();
        }

        public TextBlock[] GetTextBlocks()
        {
            return new TextBlock[]{ L0Tb, L1Tb, L2Tb, L3Tb, L4Tb, L5Tb, L6Tb, L7Tb, L8Tb, L9Tb, L10Tb, L11Tb, L12Tb };
        }
    }
}
