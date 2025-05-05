using Microsoft.Win32;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Colors = System.Windows.Media.Colors;

namespace SteelmeetWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        public List<Lifter> Lifters = new();

        public List<SpectatorWindow> spectatorWindowList = new List<SpectatorWindow>();

        public ObservableCollection<WeighInDgFormat> weighInDgCollection { get; set; }
        public ObservableCollection<ControlDgFormat> controlDgCollection { get; set; }

        private readonly HashSet<string> hiddenColumns = new() {};

        public LiftingOrder liftingOrder = new LiftingOrder();

        public RainbowColor rainbowColor = new RainbowColor();
        Fullscreen fullscreen = new Fullscreen();
        bool isFullscreen = false;
        public ThemeManagerWrapper themeManagerWrapper;

        private readonly double _originalWindowWidth = 1920;
        private readonly double _originalWindowHeight = 1080;

        bool isRecord = false;
        bool isWeighInDgInEditMode = false;
        bool isControlDgInEditMode = false;

        public string browsedFilePath;
        public string browsedFile;
        public string recordType;            // Klubb, Distrikt, Svenskt rekord, Europa rekord, World record!!! borde vara en enum?

        public int selectedLifterIndex;
        public int selectedColumnIndex;
        
        public class GroupData
        {
            public int count;
            public List<Lifter> lifters;
        }

        public List<GroupData> groupDataList = new List<GroupData>();
        public int currentGroupIndex = 0;

        public List<int> usedPlatesList = new List<int>();  // Hur många plates calculatorn har använt.
        List<int> totalPlatesList = new List<int>();        // Antalet paltes som användaren anvivit
        List<float> weightsList = new List<float>();        // Vikter
        public List<int> usedPlatesList2 = new List<int>(); // Hur många plates calculatorn har använt.
        List<int> totalPlatesList2 = new List<int>();       // Antalet paltes som användaren anvivit
        List<float> weightsList2 = new List<float>();       // Vikter

        // Default Plate setup 16x25kg
        public PlateInfo plateInfo = new PlateInfo(0, 16, 2, 2, 2, 2, 2, 2, 2, 2, Colors.ForestGreen, Colors.Red, Colors.Blue, Colors.Yellow, Colors.ForestGreen, Colors.WhiteSmoke, Colors.Black, Colors.Silver, Colors.Gainsboro, Colors.Gainsboro);

        public CultureInfo customCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

        public class LifterComparerTotal : IComparer<Lifter>
        {
            public int Compare( Lifter x, Lifter y )
            {
                return x.total.CompareTo( y.total );
            }
        }

        public ControlWindow()
        {
            InitializeComponent();

            weighInDgCollection = new ObservableCollection<WeighInDgFormat>();
            weightInDg.ItemsSource = weighInDgCollection;
            controlDgCollection = new ObservableCollection<ControlDgFormat>();
            controlDg.ItemsSource = controlDgCollection;
            themeManagerWrapper = new ThemeManagerWrapper( this );
            themeManagerWrapper.SetTheme( "Borlänge" );

#if DEBUG
            // Window sizing for easier debugging
            Height = 540;
            Width = 960;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 0;
            Top = 200;
#endif
        }

        public void ExcelImportHandler()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Steelmeet Importera fil :)",
                Filter = "Excel files|*.xlsx; *.xls|" + "All files (*.*)|*.*"
            };
            bool? result = ofd.ShowDialog();
            if( result == true )
            {
                if( ".xls" == Path.GetExtension( ofd.FileName ) || ".xlsx" == Path.GetExtension( ofd.FileName ) )
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

            if( browsedFile == null )
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
                        b1             = sl.GetCellValueAsString(i, 15),
                        d1             = sl.GetCellValueAsString(i, 16)
                    };

                    weighInDgCollection.Add( collection );
                }
            }
            try
            {
                WeighInInfoUpdate();
            }
            catch( Exception ex )
            {
                // If loading invalid file
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
                    SLDocument sl = new SLDocument();

                    // Swedish headers
                    string[] sweHeaders = new string[]
                    {
                    "Grupp"   ,
                    "Namn"     ,
                    "Lot"       ,
                    "Klass"    ,
                    "Kategori"  ,
                    "Licensnr.",
                    "Förening"  ,
                    "Kropps\nvikt",
                    "Höjd\nBöj" ,
                    "Infällt"   ,
                    "Böj"       ,
                    "Höjd\nBänk" ,
                    "Rack\nBänk" ,
                    "Avlyft"   ,
                    "Bänk"      ,
                    "Mark"      ,
                    };

                    for( int colIndex = 0; colIndex < weightInDg.Columns.Count; colIndex++ )
                    {
                        //var columnHeader = weightInDg.Columns[colIndex].Header?.ToString();
                        var columnHeader = sweHeaders[colIndex];
                        sl.SetCellValue( 1, colIndex + 1, columnHeader );
                    }

                    for( int rowIndex = 0; rowIndex < weighInDgCollection.Count; rowIndex++ )
                    {
                        var rowItem = weighInDgCollection[ rowIndex ];

                        for( int colIndex = 0; colIndex < weightInDg.Columns.Count; colIndex++ )
                        {
                            var columnBinding = ((Binding)((DataGridTextColumn)weightInDg.Columns[ colIndex ] ).Binding );
                            var propertyName = columnBinding.Path.Path;

                            // Use some shit called reflection to get the value from the row object
                            var cellValue = rowItem.GetType().GetProperty( propertyName )?.GetValue( rowItem )?.ToString();

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
            Lifters.Clear();
            controlDgCollection.Clear();
            WeighInInfoUpdate();

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
                        return;
                    }
                }
                else
                {
                    MessageBox.Show( "Ogiltig viktklass", "⚠SteelMeet varning!⚠" ); // Varning 
                    currentWeightclass = "Ange klass!!";
                    return;
                }

                weighInDgCollection[ o ].weightClass = currentWeightclass;

                // Lägger till lyftare adderar lyftare ny lyftare
                Lifters.Add( new Lifter( weighInDgCollection[ o ] ) );
                Lifters[ Lifters.Count - 1 ].index = Lifters.Count - 1;
                SetCategoryEnum( currentCategory );

                // Is bench only
                if( Lifters[ o ].CategoryEnum == Lifter.eCategory.MenClassicBench ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.WomenClassicBench ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                {
                    Lifters[ o ].isBenchOnly = true;
                    Lifters[ o ].LiftRecord.AddRange( new bool[] { true, true, true } );
                    Lifters[ o ].currentLiftType = Lifter.eLiftType.B1;
                }
                else
                    Lifters[ o ].currentLiftType = Lifter.eLiftType.S1;

                // Is equipped lifter
                if( Lifters[ o ].CategoryEnum == Lifter.eCategory.MenEquipped ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.MenEquippedBench ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.WomenEquipped ||
                    Lifters[ o ].CategoryEnum == Lifter.eCategory.WomenEquippedBench )
                    Lifters[ o ].isEquipped = true;
                else
                    Lifters[ o ].isEquipped = false;

                groupDataList[ Lifters[ o ].groupNumber - 1 ].count++;
                groupDataList[ Lifters[ o ].groupNumber - 1 ].lifters.Add( Lifters[ o ] );

                if( Lifters[ o ].groupNumber == 1 )
                {
                    var collection = new ControlDgFormat( Lifters[ o ] );
                    controlDgCollection.Add( collection );
                }
            }
        }

        void WeighInInfoUpdate()
        {
            int groupCount = 0;
            groupDataList.Clear();
            for( int i = 0; i < weighInDgCollection.Count; i++ )
            {
                int parsedGroupNumber = int.Parse( weighInDgCollection[ i ].groupNumber );
                if( parsedGroupNumber > groupCount )
                    groupCount = parsedGroupNumber;
            }

            for( int i = 0; i < groupCount; i++ )
            {
                groupDataList.Add( new GroupData() );
                groupDataList[ i ].lifters = new List<Lifter>();
            }

            weighInDataTb.Text = "Antal Lyftare : " + ( weighInDgCollection.Count - 1 ).ToString() + "\nAntal Grupper : " + groupCount; // Uppdaterar data för invägning
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
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.MenEquippedBench;
                    }
                    else
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.MenClassicBench;
                    }
                }
                else
                {
                    if( Equipped == true )
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.MenEquipped;
                    }
                    else
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.MenClassic;
                    }
                }
            }
            else
            {
                if( BenchOnly )
                {
                    if( Equipped == true )
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenEquippedBench;
                    }
                    else
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenClassicBench;
                    }
                }
                else
                {
                    if( Equipped == true )
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenEquipped;
                    }
                    else
                    {
                        Lifters[ Lifters.Count - 1 ].CategoryEnum = Lifter.eCategory.WomenClassic;
                    }
                }
            }
        }

        public bool ToggleFullscreen()
        {
            fullscreen.ToggleFullscreen( isFullscreen, this );
            isFullscreen = !isFullscreen;
            return isFullscreen;
        }
        public void Shutdown()
        {
            var result = MessageBox.Show("Är du säker att du vill terminera STEELMEET?", "STEELMEET Terminering", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
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
            //    e.Key == Key.G && Lifters[ SelectedRowIndex + groupRowFixer ].currentLift <= firstLiftColumn + 8 &&
            //    controlDg.Rows[ SelectedRowIndex ].Cells[ Lifters[ SelectedRowIndex + groupRowFixer ].currentLift ].Value != DBNull.Value &&
            //    !controlDg.IsCurrentCellInEditMode )            //Godkänt lyft
            //{
            //    goodLiftMarked();
            //}
            //if( MainTc.SelectedIndex == 2 &&
            //    e.Key == Key.U && Lifters[ SelectedRowIndex + groupRowFixer ].currentLift <= firstLiftColumn + 8 &&
            //    controlDg.Rows[ SelectedRowIndex ].Cells[ Lifters[ SelectedRowIndex + groupRowFixer ].currentLift ].Value != DBNull.Value &&
            //    !controlDg.IsCurrentCellInEditMode )       //Underkänt lyft
            //{
            //    badLiftMarked();
            //}
            //if( MainTc.SelectedIndex == 2 && e.Key == Key.R && Lifters[ SelectedRowIndex + groupRowFixer ].currentLift >= firstLiftColumn + 1 &&
            //    !controlDg.IsCurrentCellInEditMode )       //Ångra lyft
            //{
            //    undoLift( false );
            //}
            if( ( e.Key == Key.F || e.Key == Key.F11 ) && !isWeighInDgInEditMode && !isControlDgInEditMode )
            {
                ToggleFullscreen();
            }
            if( e.Key == Key.Escape && !isWeighInDgInEditMode && !isWeighInDgInEditMode )
            {
                Shutdown();
            }
        }

        private void MainWindow_SizeChanged( object sender, SizeChangedEventArgs e )
        {
            double scaleX = this.ActualWidth / _originalWindowWidth;
            double scaleY = this.ActualHeight / _originalWindowHeight;

            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);
            MainGrid.LayoutTransform = scaleTransform;

            MainGrid.RenderTransformOrigin = new System.Windows.Point( 0.5, 0.5 );
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

        // WeighIn Tab

        // Comp Tab

    }
}
