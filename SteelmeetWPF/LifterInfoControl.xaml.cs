using DocumentFormat.OpenXml.Drawing.Diagrams;
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
    /// <summary>
    /// Interaction logic for LiftingOrder.xaml
    /// </summary>
    public partial class LifterInfoControl : UserControl
    {
        public LifterInfoControl()
        {
            InitializeComponent();
        }

        public void Update( Lifter lifter ) 
        {
            NameTb.Text = lifter.name;
            WeightTb.Text = lifter.sbdWeightsList[ ( int )lifter.currentLiftType ].ToString();

            if( lifter.currentLiftType < Lifter.eLiftType.B1 )
            {
                HeightsTb.Text = "Höjd " + lifter.squatHeight;

                if( new[] { "Nej", "No", "" }.Any( s => lifter.tilted.Contains( s ) ) )
                    TiltedLiftoffTb.Text = "";
                else
                    TiltedLiftoffTb.Text = lifter.tilted;
            }
            else if( lifter.currentLiftType < Lifter.eLiftType.D1 )
            {
                HeightsTb.Text = "Höjd " + lifter.benchHeight + "/" + lifter.benchRack;

                if( new[] { "Nej", "No", "" }.Any( s => lifter.tilted.Contains( s ) ) )
                    TiltedLiftoffTb.Text = "";
                else
                    TiltedLiftoffTb.Text = lifter.liftoff;
            }
            else 
            {
                TiltedLiftoffTb.Text = "";
                // View total -> est total
            }

        }
    }
}
