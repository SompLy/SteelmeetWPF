using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SteelmeetWPF
{
    public class DgFormat: INotifyPropertyChanged
    {
        private readonly Lifter _source;

        public DgFormat( Lifter lifter )
        {
            _source = lifter;
            UpdateFromLifter();
        }

        public void UpdateFromLifter()
        {
            // Either use :
            // foreach (var row in controlDgCollection)
            // {
            //     row.UpdateFromLifter();
            // }
            // Or just for single lifter :
            // controlDgCollection[index].UpdateFromLifter();
            //

            Placement = _source.place.ToString();
            Name = _source.name;
            LotNumber = _source.lotNumber.ToString();
            WeightClass = _source.weightClass;
            Accossiation = _source.accossiation;
            BodyWeight = _source.bodyWeight.ToString();
            SquatHeight = _source.squatHeight.ToString();
            BenchHeight = _source.benchHeight.ToString();
            BenchRack = _source.benchRack.ToString();
            S1 = _source.sbdWeightsList[ 0 ].ToString();
            S2 = _source.sbdWeightsList[ 1 ].ToString();
            S3 = _source.sbdWeightsList[ 2 ].ToString();
            B1 = _source.sbdWeightsList[ 3 ].ToString();
            B2 = _source.sbdWeightsList[ 4 ].ToString();
            B3 = _source.sbdWeightsList[ 5 ].ToString();
            D1 = _source.sbdWeightsList[ 6 ].ToString();
            D2 = _source.sbdWeightsList[ 7 ].ToString();
            D3 = _source.sbdWeightsList[ 8 ].ToString();
            Total = _source.total.ToString();
            GLPoints = _source.pointsGL.ToString();
            EstimatedTotal = _source.estimatedTotal.ToString();
            EstimatedGLPoints = _source.estimatedGLPoints.ToString();
            EstimatedPlacement = _source.estimatedPlacement.ToString();
        }

        private string? _placement;
        [Display( Name = "#" )]
        public string? Placement { get => _placement; set { _placement = value; OnPropertyChanged( nameof( Placement ) ); } }

        private string? _name;
        [Display( Name = "Name" )]
        public string? Name { get => _name; set { _name = value; OnPropertyChanged( nameof( Name ) ); } }

        private string? _lotNumber;
        [Display( Name = "Lot" )]
        public string? LotNumber { get => _lotNumber; set { _lotNumber = value; OnPropertyChanged( nameof( LotNumber ) ); } }

        private string? _weightClass;
        [Display( Name = "Class" )]
        public string? WeightClass { get => _weightClass; set { _weightClass = value; OnPropertyChanged( nameof( WeightClass ) ); } }

        private string? _accossiation;
        [Display( Name = "Association" )]
        public string? Accossiation { get => _accossiation; set { _accossiation = value; OnPropertyChanged( nameof( Accossiation ) ); } }

        private string? _bodyWeight;
        [Display( Name = "BW" )]
        public string? BodyWeight { get => _bodyWeight; set { _bodyWeight = value; OnPropertyChanged( nameof( BodyWeight ) ); } }

        private string? _squatHeight;
        [Display( Name = "Squat\nHeight" )]
        public string? SquatHeight { get => _squatHeight; set { _squatHeight = value; OnPropertyChanged( nameof( SquatHeight ) ); } }

        private string? _benchHeight;
        [Display( Name = "Bench\nHeight" )]
        public string? BenchHeight { get => _benchHeight; set { _benchHeight = value; OnPropertyChanged( nameof( BenchHeight ) ); } }

        private string? _benchRack;
        [Display( Name = "Bench\nRack" )]
        public string? BenchRack { get => _benchRack; set { _benchRack = value; OnPropertyChanged( nameof( BenchRack ) ); } }

        private string? _s1;
        [Display( Name = "S1" )]
        public string? S1 { get => _s1; set { _s1 = value; OnPropertyChanged( nameof( S1 ) ); } }

        private string? _s2;
        [Display( Name = "S2" )]
        public string? S2 { get => _s2; set { _s2 = value; OnPropertyChanged( nameof( S2 ) ); } }

        private string? _s3;
        [Display( Name = "S3" )]
        public string? S3 { get => _s3; set { _s3 = value; OnPropertyChanged( nameof( S3 ) ); } }

        private string? _b1;
        [Display( Name = "B1" )]
        public string? B1 { get => _b1; set { _b1 = value; OnPropertyChanged( nameof( B1 ) ); } }

        private string? _b2;
        [Display( Name = "B2" )]
        public string? B2 { get => _b2; set { _b2 = value; OnPropertyChanged( nameof( B2 ) ); } }

        private string? _b3;
        [Display( Name = "B3" )]
        public string? B3 { get => _b3; set { _b3 = value; OnPropertyChanged( nameof( B3 ) ); } }

        private string? _d1;
        [Display( Name = "D1" )]
        public string? D1 { get => _d1; set { _d1 = value; OnPropertyChanged( nameof( D1 ) ); } }

        private string? _d2;
        [Display( Name = "D2" )]
        public string? D2 { get => _d2; set { _d2 = value; OnPropertyChanged( nameof( D2 ) ); } }

        private string? _d3;
        [Display( Name = "D3" )]
        public string? D3 { get => _d3; set { _d3 = value; OnPropertyChanged( nameof( D3 ) ); } }

        private string? _total;
        [Display( Name = "Total" )]
        public string? Total { get => _total; set { _total = value; OnPropertyChanged( nameof( Total ) ); } }

        private string? _gLPoints;
        [Display( Name = "GL" )]
        public string? GLPoints { get => _gLPoints; set { _gLPoints = value; OnPropertyChanged( nameof( GLPoints ) ); } }

        private string? _estimatedTotal;
        [Display( Name = "Est.\n Tot" )]
        public string? EstimatedTotal { get => _estimatedTotal; set { _estimatedTotal = value; OnPropertyChanged( nameof( EstimatedTotal ) ); } }

        private string? _estimatedGLPoints;
        [Display( Name = "Est.\n GL" )]
        public string? EstimatedGLPoints { get => _estimatedGLPoints; set { _estimatedGLPoints = value; OnPropertyChanged( nameof( EstimatedGLPoints ) ); } }

        private string? _estimatedPlacement;
        [Display( Name = "Est.\n #" )]
        public string? EstimatedPlacement { get => _estimatedPlacement; set { _estimatedPlacement = value; OnPropertyChanged( nameof( EstimatedPlacement ) ); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged( string name ) =>
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );
    }
}
