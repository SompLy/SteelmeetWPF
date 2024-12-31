using System.Collections.Generic;
using System;
using System.Windows;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

using SpreadsheetLight;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Colors = System.Windows.Media.Colors;
using System.Windows.Data;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Xml.Linq;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        public Dictionary<int, Lifter> LifterID = new();

        List<SpectatorWindow> spectatorWindowList = new List<SpectatorWindow>();

        public ObservableCollection<WeighInDgFormat> WeighInDbCollection;
        //public ObservableCollection<WeighInDgFormat> ControlFormat { get; set; } will need later

        public RainbowColor rainbowColor = new RainbowColor();
        Fullscreen fullscreen = new Fullscreen();
        bool isFullscreen = false;

        bool a = true;
        bool b = true;
        public bool IsExcelFile;
        bool IsRecord = false;

        public string BrowsedFilePath;
        public string BrowsedFile;
        public string recordType;            // Klubb, Distrikt, Svenskt rekord, Europa rekord, World record!!!

        string currentLiftColor = "Black";   // Color of current lift on the datagridview

        public int SelectedRowIndex;
        public int SelectedColumnIndex;
        int secondsLapp;
        int minutesLapp;
        int secondsLyft;
        int minutesLyft;
        public int groupIndexCurrent;
        public int groupIndexCount = 1;            // Antal grupper
        public int group1Count;                    // Antal lyftare i grupp
        public int group2Count;                    // Antal lyftare i grupp
        public int group3Count;                    // Antal lyftare i grupp
        public int groupRowFixer;           // Ändars beronde på grupp så att LifterID[SelectedRowIndex + groupRowFixer] blir rätt
        int firstLiftColumn = 10;           // 130, 217 måste ändras också ????

        public List<int> usedPlatesList = new List<int>();  // Hur många plates calculatorn har använt.
        List<int> totalPlatesList = new List<int>();        // Antalet paltes som användaren anvivit
        List<float> weightsList = new List<float>();        // Vikter
        public List<int> usedPlatesList2 = new List<int>(); // Hur många plates calculatorn har använt.
        List<int> totalPlatesList2 = new List<int>();       // Antalet paltes som användaren anvivit
        List<float> weightsList2 = new List<float>();       // Vikter

        public List<TextBlock> LiftingOrderListLabels = new List<TextBlock>();    // Order med lyftare och vikt de ska ta i rätt ordning.
        public List<Lifter> LiftingOrderList = new List<Lifter>();                                                  // För att sortera

        public List<TextBlock> GroupLiftingOrderListLabels = new List<TextBlock>();   // Order med lyftare och vikt de ska ta i rätt ordning.
        List<Lifter> GroupLiftingOrderList = new List<Lifter>();                                                        // För att sortera viktera

        List<Lifter> ExtraLifters = new List<Lifter>();
        enum eGroupLiftingOrderState
        {
            group1Squat = 0,
            group1Bench = 1,
            group1Deadlift = 2,

            group2Squat = 3,
            group2Bench = 4,
            group2Deadlift = 5,

            group3Squat = 6,
            group3Bench = 7,
            group3Deadlift = 8,

            nothing = 9
        }

        // Default Plate setup 16x25kg
        public PlateInfo plateInfo = new PlateInfo(0, 16, 2, 2, 2, 2, 2, 2, 2, 2, Colors.ForestGreen, Colors.Red, Colors.Blue, Colors.Yellow, Colors.ForestGreen, Colors.WhiteSmoke, Colors.Black, Colors.Silver, Colors.Gainsboro, Colors.Gainsboro);

        public CultureInfo customCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

        public class LifterComparer : IComparer<Lifter>
        {
            public int Compare( Lifter x, Lifter y )
            {
                if( x.isRetrying && !y.isRetrying )
                {
                    return 1; // x should come after y
                }
                else if( !x.isRetrying && y.isRetrying )
                {
                    return -1; // x should come before y
                }

                int indexX = x.currentLift - 10;
                int indexY = y.currentLift - 10;

                if( indexX >= 0 && indexX < x.sbdList.Count && indexY >= 0 && indexY < y.sbdList.Count )
                {
                    float weightX = x.sbdList[indexX];
                    float weightY = y.sbdList[indexY];

                    int weightComparison = weightX.CompareTo(weightY);

                    if( weightComparison != 0 )
                    {
                        return weightComparison;
                    }

                    return x.lotNumber.CompareTo( y.lotNumber );
                }

                return 0;
            }
        }
        public class LifterComparerTotal : IComparer<Lifter>
        {
            public int Compare( Lifter x, Lifter y )
            {
                // baserad på total
                return x.total.CompareTo( y.total );
            }
        }

        public ControlWindow()
        {
            InitializeComponent();

            WeighInDbCollection = new ObservableCollection<WeighInDgFormat>();
            WeightInDg.ItemsSource = WeighInDbCollection;
        }

        private void CreateDynamicColumns( DataGrid dataGrid )
        {
            var propertiesToShowWeightIn = new List<string> // Gör en til för control panel bra :)
    {
        "GroupNumber    ",
        "Name           ",
        "WeightClass    ",
        "Category       ",
        "groupNumber    ",
        "name           ",
        "lotNumber      ",
        "weightClass    ",
        "category       ",
        "licenceNumber  ",
        "accossiation   ",
        "bodyWeight     ",
        "squatHeight    ",
        "tilted         ",
        "s1             ",
        "benchHeight    ",
        "benchRack      ",
        "liftoff        ",
        "d1             ",
        "b1             "
    };

            // Clear existing columns
            dataGrid.Columns.Clear();

            // Create a column for each property in the list
            foreach( var property in propertiesToShow )
            {
                var column = new DataGridTextColumn
                {
                    Header = property,
                    Binding = new Binding(property)  // Binding to the property in LifterData
                };

                dataGrid.Columns.Add( column );
            }
        }

        public void ExcelImportHandler()                                                                                               // Hanterar text impoteringen av excel
        {
            using SLDocument sl = new SLDocument(BrowsedFile);
            SLWorksheetStatistics stats = sl.GetWorksheetStatistics();

            int rowCount = stats.NumberOfRows;
            int realRowCount = 0;
            int columnCount = stats.NumberOfColumns;

            List<string> data = new List<string>();
            for( int i = 1; 0 < 1; i++ ) //Hittar antal rader som är ifyllda
            {
                if( string.IsNullOrWhiteSpace( sl.GetCellValueAsString( i, 1 ) ) )
                {
                    realRowCount = i;
                    break;
                }
            }

            for( int i = 1; i < realRowCount; i++ )
            {
                if( sl.GetCellValueAsString( i, 1 ) != "Grupp" )
                {
                    var collection = new WeighInDgFormat
                    {
                         groupNumber    = sl.GetCellValueAsString(i, 1),
                         name           = sl.GetCellValueAsString(i, 2),
                         lotNumber      = sl.GetCellValueAsString(i, 3),
                         weightClass    = sl.GetCellValueAsString(i, 4),
                         category       = sl.GetCellValueAsString(i, 5),
                         licenceNumber  = sl.GetCellValueAsString(i, 6),
                         accossiation   = sl.GetCellValueAsString(i, 7),
                         bodyWeight     = sl.GetCellValueAsString(i, 8),
                         squatHeight    = sl.GetCellValueAsString(i, 9),
                         tilted         = sl.GetCellValueAsString(i, 10),
                         s1             = sl.GetCellValueAsString(i, 11),
                         benchHeight    = sl.GetCellValueAsString(i, 12),
                         benchRack      = sl.GetCellValueAsString(i, 13),
                         liftoff        = sl.GetCellValueAsString(i, 14),
                         d1             = sl.GetCellValueAsString(i, 15),
                         b1             = sl.GetCellValueAsString(i, 16)
                    };

                    WeighInDbCollection.Add( collection );
                }
            }
            try
            {
                // Om man laddar en ogitig fil
                WeighInInfoUpdate();
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }

            for( int i = 0; i < WeightInDg.Columns.Count; i++ )
            {
                WeightInDg.Columns[ i ].CanUserSort = false;
            }
        }

        private void ImportBtn_Click( object sender, EventArgs e )
        {
            try
            {
                SaveFileDialog ofd = new SaveFileDialog();
                ofd.InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.Desktop );
                ofd.Title = "Steelmeet Impoertera fil :)";
                ofd.Filter = "Excel file |*.xlsx";
                ofd.FileName = "Steelmeet_lyftare_Start_XX.XX";
                bool? result = ofd.ShowDialog();

                if( result == true )
                {
                    SLDocument sl = new SLDocument();
                    for( int i = 0; i < WeighInDbCollection.Count - 1; i++ )
                    {
                        for( int o = 0; o < WeightInDg.; o++ )
                        {
                            sl.SetCellValue( i + 1, o + 1, WeightInDg.Rows[ i ].Cells[ o ].Value.ToString() );
                        }
                    }
                    sl.SaveAs( ofd.FileName );

                    MessageBox.Show( "Excel fil sparad! :)" );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }

        }
        public void ExcelExport()
        {

        }
        private void btn_Comp_Click( object sender, EventArgs e ) // Skicka till tävlings knappen lol
        {
            List<string> list = new List<string>();
            LifterID.Clear();
            dt2.Rows.Clear();

            for( int o = 0; o < WeightInDg.RowCount - 1; o++ )
            {
                for( int i = 0; i < WeightInDg.ColumnCount; i++ ) // Antal columner som inte är lyft
                {
                    list.Add( WeightInDg[ i, o ].Value.ToString() );
                }

                if( list[ 4 ].ToLower().Contains( "herr" ) )                      // Kollar om viktklassen är giltig för dam och herr
                {

                    if( list[ 3 ].ToLower().Contains( "120+" ) || list[ 3 ].ToLower().Contains( "+120" ) )
                    {
                        list[ 3 ] = "+120";
                    }
                    else if( list[ 3 ].ToLower().Contains( "120" ) )
                    {
                        list[ 3 ] = "-120";
                    }
                    else if( list[ 3 ].ToLower().Contains( "105" ) )
                    {
                        list[ 3 ] = "-105";
                    }
                    else if( list[ 3 ].ToLower().Contains( "93" ) )
                    {
                        list[ 3 ] = "-93";
                    }
                    else if( list[ 3 ].ToLower().Contains( "83" ) )
                    {
                        list[ 3 ] = "-83";
                    }
                    else if( list[ 3 ].ToLower().Contains( "74" ) )
                    {
                        list[ 3 ] = "-74";
                    }
                    else if( list[ 3 ].ToLower().Contains( "66" ) )
                    {
                        list[ 3 ] = "-66";
                    }
                    else if( list[ 3 ].ToLower().Contains( "59" ) )
                    {
                        list[ 3 ] = "-59";
                    }
                    else if( list[ 3 ].ToLower().Contains( "53" ) )
                    {
                        list[ 3 ] = "-53";
                    }
                    else if( list[ 3 ].ToLower().Contains( "koeffhk" ) )          // Herr Klassiskt
                    {
                        list[ 3 ] = "koeffHK";
                    }
                    else if( list[ 3 ].ToLower().Contains( "koeffhu" ) )          // Herr Utrustat
                    {
                        list[ 3 ] = "koeffHU";
                    }
                    else
                    {
                        MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                        list[ 3 ] = "Ange klass!!";
                    }
                }
                else if( list[ 4 ].ToLower().Contains( "dam" ) ) // Dam viktklass
                {
                    if( list[ 3 ].ToLower().Contains( "84+" ) || list[ 3 ].ToLower().Contains( "+84" ) )
                    {
                        list[ 3 ] = "+84";
                    }
                    else if( list[ 3 ].ToLower().Contains( "84" ) )
                    {
                        list[ 3 ] = "-84";
                    }
                    else if( list[ 3 ].ToLower().Contains( "76" ) )
                    {
                        list[ 3 ] = "-76";
                    }
                    else if( list[ 3 ].ToLower().Contains( "69" ) )
                    {
                        list[ 3 ] = "-69";
                    }
                    else if( list[ 3 ].ToLower().Contains( "63" ) )
                    {
                        list[ 3 ] = "-63";
                    }
                    else if( list[ 3 ].ToLower().Contains( "57" ) )
                    {
                        list[ 3 ] = "-57";
                    }
                    else if( list[ 3 ].ToLower().Contains( "52" ) )
                    {
                        list[ 3 ] = "-52";
                    }
                    else if( list[ 3 ].ToLower().Contains( "47" ) )
                    {
                        list[ 3 ] = "-47";
                    }
                    else if( list[ 3 ].ToLower().Contains( "43" ) )
                    {
                        list[ 3 ] = "-43";
                    }
                    else if( list[ 3 ].ToLower().Contains( "koeffdk" ) )      // Dam Klassiskt
                    {
                        list[ 3 ] = "koeffDK";
                    }
                    else if( list[ 3 ].ToLower().Contains( "koeffdu" ) )      // Dam Utrustat
                    {
                        list[ 3 ] = "koeffDU";
                    }
                    else
                    {
                        MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                        list[ 3 ] = "Ange klass!!";
                    }
                }
                else
                {
                    MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                    list[ 3 ] = "Ange klass!!";
                }

                WeightInDg.Rows[ o ].Cells[ 3 ].Value = list[ 3 ];

                // Lägger till lyftare adderar lyftare ny lyftare
                LifterID.Add( o, new Lifter( list[ 0 ], list[ 1 ], list[ 2 ], list[ 3 ], list[ 4 ], list[ 5 ], list[ 6 ], list[ 7 ], list[ 8 ], list[ 9 ], list[ 10 ], list[ 11 ], list[ 12 ], list[ 13 ], list[ 14 ], list[ 15 ] ) );
                LifterID[ LifterID.Count - 1 ].index = LifterID.Count - 1;
                SetCategoryEnum( list[ 4 ] );

                // Is bench only
                if( LifterID[ o ].CategoryEnum == Lifter.eCategory.MenClassicBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenClassicBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                {
                    LifterID[ o ].isBenchOnly = true;
                    LifterID[ o ].LiftRecord.AddRange( new bool[] { true, true, true } );
                    LifterID[ o ].CurrentLift = firstLiftColumn + 3;
                }

                // Is equipped lifter
                if( LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquipped ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquipped ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                    LifterID[ o ].isEquipped = true;
                else
                    LifterID[ o ].isEquipped = false;

                list.Clear();
            }

            // Stränger av sorting (gör header rutorna så feta också)
            for( int i = 0; i < dataGridCP.ColumnCount; i++ )
            {
                dataGridCP.Columns[ i ].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        void WeighInInfoUpdate()
        {
            string gindex = dataGridViewWeighIn.Rows[dataGridViewWeighIn.RowCount - 2].Cells[0].Value.ToString();                          // Tar den sista lyftarens grupp
            dataGridViewWeighIn.Rows[ 0 ].Selected = false;
            lbl_WeightInData.Text = "Antal Lyftare : " + ( dataGridViewWeighIn.RowCount - 1 ).ToString() + "\nAntal Grupper : " + gindex; // Uppdaterar data för invägning
        }

        void SetCategoryEnum( string Category )
        {
            string[] wholeThing;

            string sex;
            string yearclass;
            bool Equipped;
            bool BenchOnly;

            wholeThing = Category.Split( ' ' );
            sex = wholeThing[ 0 ].ToLower();
            yearclass = wholeThing[ 1 ].ToLower();

            if( wholeThing[ 2 ].ToLower() == "utrustat" )
            {
                Equipped = true;
            }
            else
            {
                Equipped = false;
            }
            if( wholeThing[ 3 ].ToLower() == "bänkpress" )
            {
                BenchOnly = true;
            }
            else
            {
                BenchOnly = false;
            }

            if( sex == "herr" )
            {
                if( BenchOnly )
                {
                    if( Equipped == true )
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.MenEquippedBench;
                    }
                    else
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.MenClassicBench;
                    }
                }
                else
                {
                    if( Equipped == true )
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.MenEquipped;
                    }
                    else
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.MenClassic;
                    }
                }
            }
            else
            {
                if( BenchOnly )
                {
                    if( Equipped == true )
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenEquippedBench;
                    }
                    else
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenClassicBench;
                    }
                }
                else
                {
                    if( Equipped == true )
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenEquipped;
                    }
                    else
                    {
                        LifterID[ LifterID.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenClassic;
                    }
                }
            }
        }
    }
}
