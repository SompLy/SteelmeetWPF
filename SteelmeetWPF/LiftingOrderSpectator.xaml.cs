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

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for LiftingOrderSpectator.xaml
    /// </summary>
    public partial class LiftingOrderSpectator : UserControl
    {
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
    }
}
