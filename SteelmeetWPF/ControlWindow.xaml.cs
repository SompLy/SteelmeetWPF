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
using DocumentFormat.OpenXml.Presentation;
using System.IO;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        public Dictionary<int, Lifter> LifterID = new();

        List<SpectatorWindow> spectatorWindowList = new List<SpectatorWindow>();

        public ObservableCollection<WeighInDgFormat> weighInDgCollection;
        public ObservableCollection<WeighInDgFormat> controlDgCollection;

        public RainbowColor rainbowColor = new RainbowColor();
        Fullscreen fullscreen = new Fullscreen();
        bool isFullscreen = false;

        bool a = true;
        bool b = true;
        bool isRecord = false;
        bool isWeighInDgInEditMode = false;
        bool isControlDgInEditMode = false;

        public string browsedFilePath;
        public string browsedFile;
        public string recordType;            // Klubb, Distrikt, Svenskt rekord, Europa rekord, World record!!!

        string currentLiftColor = "Black";   // Color of current lift on the datagridview

        public int selectedRowIndex;
        public int selectedColumnIndex;
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

        public List<TextBlock> groupLiftingOrderListLabels = new List<TextBlock>();   // Order med lyftare och vikt de ska ta i rätt ordning.
        List<Lifter> groupLiftingOrderList = new List<Lifter>();                                                        // För att sortera viktera

        List<Lifter> extraLifters = new List<Lifter>();
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

            weighInDgCollection = new ObservableCollection<WeighInDgFormat>();
            weightInDg.ItemsSource = weighInDgCollection;
        }

        private void CreateDynamicColumns( DataGrid dataGrid )
        {
            var propertiesToShowWeightIn = new List<string> // Gör en til för control panel bra :)
    {
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
            foreach( var property in propertiesToShowWeightIn )
            {
                var column = new DataGridTextColumn
                {
                    Header = property,
                    Binding = new Binding(property)  // Binding to the property in LifterData
                };

                dataGrid.Columns.Add( column );
            }
        }

        public void ExcelImportHandler()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Steelmeet Importera fil :)",
                Filter = "Excel och txt files|*.txt; *.xlsx; *.xls|" + "All files (*.*)|*.*"
            };
            bool? result = ofd.ShowDialog();
            if( result == true )
            {
                if( ".xls" == Path.GetExtension( ofd.FileName ) || ".xlsx" == Path.GetExtension( ofd.FileName ) )   // Om man väljer en excel fil
                {
                    browsedFile = ofd.FileName;
                    try
                    {
                        FileInfo finfo = new FileInfo(browsedFile);
                        browsedFilePath = finfo.DirectoryName + "\\" + finfo.Name;
                        filePathTb.Text = "Filsökväg: " + browsedFilePath;

                        weighInDgCollection.Clear();
                    }
                    catch( IOException )
                    {
                    }
                }
            }
            else
                return;

            using SLDocument sl = new SLDocument(browsedFile);
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

                    weighInDgCollection.Add( collection );
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

            for( int i = 0; i < weightInDg.Columns.Count; i++ )
            {
                weightInDg.Columns[ i ].CanUserSort = false;
            }
        }

        private void ExcelImportUpdate()
        {
            weighInDgCollection.Clear();

            ExcelImportHandler();
        }

        public void ExcelExportHandler() 
        {
            try
            {
                // Configure SaveFileDialog
                SaveFileDialog ofd = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = "Steelmeet Exportera fil :)",
                    Filter = "Excel file |*.xlsx",
                    FileName = "Steelmeet_lyftare_Start_XX.XX"
                };
                bool? result = ofd.ShowDialog();

                if( result == true )
                {
                    // Create a new SL document
                    SLDocument sl = new SLDocument();

                    // Add headers
                    for( int colIndex = 0; colIndex < weightInDg.Columns.Count; colIndex++ )
                    {
                        var columnHeader = weightInDg.Columns[colIndex].Header?.ToString();
                        sl.SetCellValue( 1, colIndex + 1, columnHeader );
                    }

                    // Add data
                    for( int rowIndex = 0; rowIndex < weighInDgCollection.Count; rowIndex++ )
                    {
                        var rowItem = weighInDgCollection[rowIndex];

                        for( int colIndex = 0; colIndex < weightInDg.Columns.Count; colIndex++ )
                        {
                            var columnBinding = ((Binding)((DataGridTextColumn)weightInDg.Columns[colIndex]).Binding);
                            var propertyName = columnBinding.Path.Path;

                            // Use some shit called reflection to get the value from the row object
                            var cellValue = rowItem.GetType().GetProperty(propertyName)?.GetValue(rowItem)?.ToString();

                            sl.SetCellValue( rowIndex + 2, colIndex + 1, cellValue );
                        }
                    }
                    sl.SaveAs( ofd.FileName );

                    MessageBox.Show( "Excel fil exporterad utan besvär! :)" );
                }
            }
            catch( Exception ex )
            {
                MessageBox.Show( ex.Message );
            }
        }

        public void SendToCompetitionTab() 
        {
            LifterID.Clear();
            controlDgCollection.Clear();

            for( int o = 0; o < weighInDgCollection.Count - 1; o++ )
            {
                string currentWeightclass = weighInDgCollection[ o ].weightClass.ToLower();
                string currentCategory = weighInDgCollection[ o ].category.ToLower();

                if( currentCategory.Contains( "herr" ) )                      // Kollar om viktklassen är giltig för dam och herr
                {

                    if( currentWeightclass.Contains( "120+" ) || currentWeightclass.Contains( "+120" ) )
                    {
                        currentWeightclass = "+120";
                    }
                    else if( currentWeightclass.Contains( "120" ) )
                    {
                        currentWeightclass = "-120";
                    }
                    else if( currentWeightclass.Contains( "105" ) )
                    {
                        currentWeightclass = "-105";
                    }
                    else if( currentWeightclass.Contains( "93" ) )
                    {
                        currentWeightclass = "-93";
                    }
                    else if( currentWeightclass.Contains( "83" ) )
                    {
                        currentWeightclass = "-83";
                    }
                    else if( currentWeightclass.Contains( "74" ) )
                    {
                        currentWeightclass = "-74";
                    }
                    else if( currentWeightclass.Contains( "66" ) )
                    {
                        currentWeightclass = "-66";
                    }
                    else if( currentWeightclass.Contains( "59" ) )
                    {
                        currentWeightclass = "-59";
                    }
                    else if( currentWeightclass.Contains( "53" ) )
                    {
                        currentWeightclass = "-53";
                    }
                    else if( currentWeightclass.Contains( "koeffhk" ) )          // Herr Klassiskt
                    {
                        currentWeightclass = "koeffHK";
                    }
                    else if( currentWeightclass.Contains( "koeffhu" ) )          // Herr Utrustat
                    {
                        currentWeightclass = "koeffHU";
                    }
                    else
                    {
                        MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                        currentWeightclass = "Ange klass!!";
                    }
                }
                else if( currentCategory.Contains( "dam" ) ) // Dam viktklass
                {
                    if( currentWeightclass.Contains( "84+" ) || currentWeightclass.Contains( "+84" ) )
                    {
                        currentWeightclass = "+84";
                    }
                    else if( currentWeightclass.Contains( "84" ) )
                    {
                        currentWeightclass = "-84";
                    }
                    else if( currentWeightclass.Contains( "76" ) )
                    {
                        currentWeightclass = "-76";
                    }
                    else if( currentWeightclass.Contains( "69" ) )
                    {
                        currentWeightclass = "-69";
                    }
                    else if( currentWeightclass.Contains( "63" ) )
                    {
                        currentWeightclass = "-63";
                    }
                    else if( currentWeightclass.Contains( "57" ) )
                    {
                        currentWeightclass = "-57";
                    }
                    else if( currentWeightclass.Contains( "52" ) )
                    {
                        currentWeightclass = "-52";
                    }
                    else if( currentWeightclass.Contains( "47" ) )
                    {
                        currentWeightclass = "-47";
                    }
                    else if( currentWeightclass.Contains( "43" ) )
                    {
                        currentWeightclass = "-43";
                    }
                    else if( currentWeightclass.Contains( "koeffdk" ) )      // Dam Klassiskt
                    {
                        currentWeightclass = "koeffDK";
                    }
                    else if( currentWeightclass.Contains( "koeffdu" ) )      // Dam Utrustat
                    {
                        currentWeightclass = "koeffDU";
                    }
                    else
                    {
                        MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                        currentWeightclass = "Ange klass!!";
                    }
                }
                else
                {
                    MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                    currentWeightclass = "Ange klass!!";
                }

                weighInDgCollection[ o ].weightClass = currentWeightclass;

                // Lägger till lyftare adderar lyftare ny lyftare
                LifterID.Add( o, new Lifter( weighInDgCollection[ o ] ) );
                LifterID[ LifterID.Count - 1 ].index = LifterID.Count - 1;
                SetCategoryEnum( currentCategory );

                // Is bench only
                if( LifterID[ o ].CategoryEnum == Lifter.eCategory.MenClassicBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenClassicBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                {
                    LifterID[ o ].isBenchOnly = true;
                    LifterID[ o ].LiftRecord.AddRange( new bool[] { true, true, true } );
                    LifterID[ o ].currentLift = firstLiftColumn + 3;
                }

                // Is equipped lifter
                if( LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquipped ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquipped ||
                    LifterID[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                    LifterID[ o ].isEquipped = true;
                else
                    LifterID[ o ].isEquipped = false;
            }
        }

        void WeighInInfoUpdate()
        {
            int gIndex = 0;
            for( int i = 0; i < weighInDgCollection.Count; i++ )
            {
                int parsedGroupNumber = int.Parse( weighInDgCollection[ i ].groupNumber );
                if( parsedGroupNumber > gIndex )
                    gIndex = parsedGroupNumber;
            }
            //dataGridViewWeighIn.Rows[ 0 ].Selected = false;
            weighInDataTb.Text = "Antal Lyftare : " + ( weighInDgCollection.Count - 1 ).ToString() + "\nAntal Grupper : " + gIndex; // Uppdaterar data för invägning
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

        void UpdateTheme( string themeName )
        {
            ThemeManager.SetTheme( themeName );

            Application.Current.Resources[ "background"   ] = ThemeManager.background;
            Application.Current.Resources[ "background2"  ] = ThemeManager.background2;
            Application.Current.Resources[ "middleGround" ] = ThemeManager.middleGround;
            Application.Current.Resources[ "accent"       ] = ThemeManager.accent;
            Application.Current.Resources[ "fontColor"    ] = ThemeManager.fontColor;
        }

        // Events
        private void HandleInput_KeyDown( object sender, KeyEventArgs e )
        {
            //if( MainTc.SelectedIndex == 2 &&
            //   e.Key == Key.Enter ) //om man är på sista raden
            //{
            //    controlDg.Rows[ SelectedRowIndex ].Cells[ SelectedColumnIndex - 1 ].Selected = true;
            //}
            //if( MainTc.SelectedIndex == 2 &&
            //    e.Key == Key.G && LifterID[ SelectedRowIndex + groupRowFixer ].currentLift <= firstLiftColumn + 8 &&
            //    controlDg.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].currentLift ].Value != DBNull.Value &&
            //    !controlDg.IsCurrentCellInEditMode )            //Godkänt lyft
            //{
            //    goodLiftMarked();
            //}
            //if( MainTc.SelectedIndex == 2 &&
            //    e.Key == Key.U && LifterID[ SelectedRowIndex + groupRowFixer ].currentLift <= firstLiftColumn + 8 &&
            //    controlDg.Rows[ SelectedRowIndex ].Cells[ LifterID[ SelectedRowIndex + groupRowFixer ].currentLift ].Value != DBNull.Value &&
            //    !controlDg.IsCurrentCellInEditMode )       //Underkänt lyft
            //{
            //    badLiftMarked();
            //}
            //if( MainTc.SelectedIndex == 2 && e.Key == Key.R && LifterID[ SelectedRowIndex + groupRowFixer ].currentLift >= firstLiftColumn + 1 &&
            //    !controlDg.IsCurrentCellInEditMode )       //Ångra lyft
            //{
            //    undoLift( false );
            //}
            if( e.Key == Key.F && !isWeighInDgInEditMode && !isControlDgInEditMode )
            {
                fullscreen.ToggleFullscreen( isFullscreen, this );
                isFullscreen = !isFullscreen;
            }
            if( e.Key == Key.Escape && /*!controlDg.IsCurrentCellInEditMode &&*/ !isWeighInDgInEditMode )
            {
                var result = MessageBox.Show("Är du säker att du vill terminera STEELMEET?", "STEELMEET Terminering", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if( result == MessageBoxResult.Yes )
                {
                    Application.Current.Shutdown();
                }
            }
        }

        private void WeighInDg_BeginningEdit( object sender, DataGridBeginningEditEventArgs e )
        {
            isWeighInDgInEditMode = true;
        }

        private void WeighInDg_CellEditEnding( object sender, DataGridCellEditEndingEventArgs e )
        {
            isWeighInDgInEditMode = false;
        }

        private void ImportBtn_Click( object sender, RoutedEventArgs e )
        {
            ExcelImportHandler();
        }

        private void ExportBtn_Click( object sender, RoutedEventArgs e )
        {
            ExcelExportHandler();
        }

        private void UpdateImportBtn_Click( object sender, RoutedEventArgs e )
        {
            ExcelImportUpdate();
        }

        private void SendToCompetitionBtn_Click( object sender, RoutedEventArgs e )
        {
            SendToCompetitionTab();
        }
    }
}
