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
    /// Interaction logic for RecordControl.xaml
    /// </summary>
    public partial class RecordControl : UserControl
    {
        public enum ERecordRange
        {
            None,
            Club,
            District,
            National
        }
        public enum ERecordAge
        {
            None,
            SubJunior,
            Junior,
            Senior
        }
        public enum ERecordEvent
        {
            None,
            Squat,
            Bench,
            Deadlift,
            Total
        }

        bool isRecordActive = false;

        public ERecordRange recordRange = ERecordRange.None;
        public ERecordAge recordAge = ERecordAge.None;
        public ERecordEvent recordEvent = ERecordEvent.None;

        public RecordControl()
        {
            InitializeComponent();
        }

        private void RecordBtn_Click( object sender, RoutedEventArgs e )
        {
            isRecordActive = !isRecordActive;
        }

        private void ClubCb_Click( object sender, RoutedEventArgs e )
        {
            recordRange = ERecordRange.Club;
        }

        private void DistrictCb_Click( object sender, RoutedEventArgs e )
        {
            recordRange = ERecordRange.District;
        }

        private void SwedishCb_Click( object sender, RoutedEventArgs e )
        {
            recordRange = ERecordRange.National;
        }

        private void SubJuniorCb_Click( object sender, RoutedEventArgs e )
        {
            recordAge = ERecordAge.SubJunior;
        }

        private void JuinorCb_Click( object sender, RoutedEventArgs e )
        {
            recordAge = ERecordAge.Junior;
        }

        private void SeniorCb_Click( object sender, RoutedEventArgs e )
        {
            recordAge = ERecordAge.Senior;
        }

        private void SquatCb_Click( object sender, RoutedEventArgs e )
        {
            recordEvent = ERecordEvent.Squat;
        }

        private void BenchCb_Click( object sender, RoutedEventArgs e )
        {
            recordEvent = ERecordEvent.Bench;
        }

        private void DeadliftCb_Click( object sender, RoutedEventArgs e )
        {
            recordEvent = ERecordEvent.Deadlift;
        }

        private void TotalCb_Click( object sender, RoutedEventArgs e )
        {
            recordEvent = ERecordEvent.Total;
        }
    }
}
