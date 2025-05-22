using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelmeetWPF
{
    public class Lifter
    {
        public Lifter(
            string groupNumber,
            string name,
            string lotNumber,
            string weightClass,
            string category,
            string licenceNumber,
            string accossiation,
            string bodyWeight,
            string squatHeight,
            string tilted,
            string s1,
            string benchHeight,
            string benchRack,
            string liftoff,
            string b1,
            string d1 )
        {
            this.groupNumber = Int16.Parse( groupNumber );
            this.name = name;
            this.lotNumber = Int16.Parse( lotNumber );
            this.weightClass = weightClass;
            this.category = category;
            this.licenceNumber = licenceNumber;
            this.accossiation = accossiation;
            this.bodyWeight = float.Parse( bodyWeight );
            this.squatHeight = Int16.Parse( squatHeight );
            this.tilted = tilted;
            this.s1 = float.Parse( s1 );
            this.benchHeight = Int16.Parse( benchHeight );
            this.benchRack = Int16.Parse( benchRack );
            this.liftoff = liftoff;
            this.b1 = float.Parse( b1 );
            this.d1 = float.Parse( d1 );

            currentLiftType = 0;
            LiftRecord = new List<eLiftJudge>();
            sbdWeightsList = new List<float>() { this.s1, s2, s3, this.b1, b2, b3, this.d1, d2, d3 };
        }

        public Lifter(WeighInDgFormat weighInCollection)
        {
            this.groupNumber = Int16.Parse( weighInCollection.groupNumber );
            this.name = weighInCollection.name;
            this.lotNumber = Int16.Parse( weighInCollection.lotNumber );
            this.weightClass = weighInCollection.weightClass;
            this.category = weighInCollection.category;
            this.licenceNumber = weighInCollection.licenceNumber;
            this.accossiation = weighInCollection.accossiation;
            this.bodyWeight = float.Parse( weighInCollection.bodyWeight, CultureInfo.InvariantCulture );
            this.squatHeight = Int16.Parse( weighInCollection.squatHeight );
            this.tilted = weighInCollection.tilted;
            this.s1 = float.Parse( weighInCollection.s1, CultureInfo.InvariantCulture );
            this.benchHeight = Int16.Parse( weighInCollection.benchHeight );
            this.benchRack = Int16.Parse( weighInCollection.benchRack );
            this.liftoff = weighInCollection.liftoff;
            this.b1 = float.Parse( weighInCollection.b1, CultureInfo.InvariantCulture );
            this.d1 = float.Parse( weighInCollection.d1, CultureInfo.InvariantCulture );

            currentLiftType = 0;
            LiftRecord = new List<eLiftJudge>() { eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE, eLiftJudge.NONE };
            sbdWeightsList = new List<float>() { this.s1, s2, s3, this.b1, b2, b3, this.d1, d2, d3 };
        }

        public void BestSBDUpdate() 
        {
            float[] sbdValues = new float[ 9 ];

            for( int i = 0 ; i < 9 ; i++ )
                sbdValues[ i ] = sbdWeightsList[ i ];

            for( int i = 0 ; i < 9 ; i++ )
                if( LiftRecord[ i ] != eLiftJudge.GOOD )
                    sbdValues[ i ] = 0.0f;

            bestS = Math.Max( sbdValues[ 0 ], Math.Max( sbdValues[ 1 ], sbdValues[ 2 ] ) );
            bestB = Math.Max( sbdValues[ 3 ], Math.Max( sbdValues[ 4 ], sbdValues[ 5 ] ) );
            bestD = Math.Max( sbdValues[ 6 ], Math.Max( sbdValues[ 7 ], sbdValues[ 8 ] ) );
        }

        public void EstimatedUpdate() 
        {
            float estimatedSquat    = Math.Max( sbdWeightsList[ 0 ], Math.Max( sbdWeightsList[ 1 ], sbdWeightsList[ 2 ] ) );
            float estimatedBench    = Math.Max( sbdWeightsList[ 3 ], Math.Max( sbdWeightsList[ 4 ], sbdWeightsList[ 5 ] ) );
            float estimatedDeadlift = Math.Max( sbdWeightsList[ 6 ], Math.Max( sbdWeightsList[ 7 ], sbdWeightsList[ 8 ] ) );

            estimatedTotal = estimatedSquat + estimatedBench + estimatedDeadlift;
            estimatedGLPoints = ( float )Math.Round(CalculateGLPoints( estimatedTotal ), 2);
        }

        public float CalculateGLPoints( float calcTotal )
        {
            //Men
            double MenEquippedA      = 1236.25115;
            double MenEquippedB      = 1449.21864;
            double MenEquippedC      = 0.01644;
            double MenClassicA       = 1199.72839;
            double MenClassicB       = 1025.18162;
            double MenClassicC       = 0.00921;
            double MenEquippedBenchA = 381.22073;
            double MenEquippedBenchB = 733.79378;
            double MenEquippedBenchC = 0.02398;
            double MenClassicBenchA  = 320.98041;
            double MenClassicBenchB  = 281.40258;
            double MenClassicBenchC  = 0.01008;
            //Women
            double WomenEquippedA      = 758.63878;
            double WomenEquippedB      = 949.31382;
            double WomenEquippedC      = 0.02435;
            double WomenClassicA       = 610.32796;
            double WomenClassicB       = 1045.59282;
            double WomenClassicC       = 0.03048;
            double WomenEquippedBenchA = 221.82209;
            double WomenEquippedBenchB = 357.00377;
            double WomenEquippedBenchC = 0.02937;
            double WomenClassicBenchA  = 142.40398;
            double WomenClassicBenchB  = 442.52671;
            double WomenClassicBenchC  = 0.04724;

            double A = 1;
            double B = 1;
            double C = 1;

            double GLPointsCoeff = 0;
            double GLPoints = 0;

            switch( CategoryEnum )
            {
                case Lifter.eCategory.MenEquipped:
                    A = MenEquippedA;
                    B = MenEquippedB;
                    C = MenEquippedC;
                    break;
                case Lifter.eCategory.MenClassic:
                    A = MenClassicA;
                    B = MenClassicB;
                    C = MenClassicC;
                    break;
                case Lifter.eCategory.MenEquippedBench:
                    A = MenEquippedBenchA;
                    B = MenEquippedBenchB;
                    C = MenEquippedBenchC;
                    break;
                case Lifter.eCategory.MenClassicBench:
                    A = MenClassicBenchA;
                    B = MenClassicBenchB;
                    C = MenClassicBenchC;
                    break;
                case Lifter.eCategory.WomenEquipped:
                    A = WomenEquippedA;
                    B = WomenEquippedB;
                    C = WomenEquippedC;
                    break;
                case Lifter.eCategory.WomenClassic:
                    A = WomenClassicA;
                    B = WomenClassicB;
                    C = WomenClassicC;
                    break;
                case Lifter.eCategory.WomenEquippedBench:
                    A = WomenEquippedBenchA;
                    B = WomenEquippedBenchB;
                    C = WomenEquippedBenchC;
                    break;
                case Lifter.eCategory.WomenClassicBench:
                    A = WomenClassicBenchA;
                    B = WomenClassicBenchB;
                    C = WomenClassicBenchC;
                    break;
                default:
                    break;
            }
            GLPointsCoeff = 100 / ( A - B * Math.Pow( Math.E, -C * bodyWeight ) );
            GLPoints = calcTotal * GLPointsCoeff;

            return ( float )GLPoints;
        }

        public void RankUpdate( ControlWindow controlWindow )
        {
            var lifters = controlWindow.Lifters;

            var groupedLifters = lifters.GroupBy(l => new { l.weightClass, l.CategoryEnum });
            List<Lifter> sortedLifters;
            string[] koeffWeightClasses = { "koeffdk", "koeffdu", "koeffhk", "koeffhu" };

            foreach( var group in groupedLifters )
            {
                // Sort the lifters within the group based on their total then by bodyweight in descending order
                if( koeffWeightClasses.Contains( group.Key.weightClass ) )
                    sortedLifters = group.OrderByDescending( l => l.pointsGL ).ToList(); // Tror jag har fixat för koeff klasserna nu. svar : ja det har du
                else
                    sortedLifters = group.OrderByDescending( l => l.total ).ThenBy( l => l.bodyWeight ).ToList();

                for( int i = 0 ; i < sortedLifters.Count ; i++ )
                {
                    var lifterToUpdate = lifters.FirstOrDefault(l => l.weightClass == group.Key.weightClass && l.CategoryEnum == group.Key.CategoryEnum && l.name == sortedLifters[i].name);

                    if( lifterToUpdate != null )
                        lifterToUpdate.place = i + 1;
                }
            }
        }

        public int place { get; set; }
        public int groupNumber { get; set; }
        public string name { get; set; }
        public int lotNumber { get; set; }
        public string weightClass { get; set; }
        public string category { get; set; }
        public enum eCategory
        {
            MenEquipped,
            MenClassic,
            MenEquippedBench,
            MenClassicBench,
            WomenEquipped,
            WomenClassic,
            WomenEquippedBench,
            WomenClassicBench

        }
        public eCategory CategoryEnum { get; set; }
        public bool isBenchOnly { get; set; }
        public bool isEquipped { get; set; }
        public bool isRetrying { get; set; }
        public string licenceNumber { get; set; }
        public string accossiation { get; set; }

        public float bodyWeight { get; set; }
        public int squatHeight { get; set; }
        public string tilted { get; set; }
        public float s1 { get; set; }
        public float s2 { get; set; }
        public float s3 { get; set; }
        public int benchHeight { get; set; }
        public int benchRack { get; set; }
        public string liftoff { get; set; }
        public float b1 { get; set; }
        public float b2 { get; set; }
        public float b3 { get; set; }
        public float d1 { get; set; }
        public float d2 { get; set; }
        public float d3 { get; set; }
        public float total { get; set; }
        public int pointsWilks { get; set; }
        public double pointsGL { get; set; }

        public enum eLiftType 
        {
            S1,
            S2,
            S3,
            B1,
            B2,
            B3,
            D1,
            D2,
            D3,
            Done,
        }
        public eLiftType currentLiftType { get; set; }
        public float bestS { get; set; }
        public float bestB { get; set; }
        public float bestD { get; set; }
        public enum eLiftJudge 
        {
            GOOD,
            BAD,
            NONE,
        }
        public List<eLiftJudge> LiftRecord { get; set; }
        public List<float> sbdWeightsList { get; set; }
        public int index { get; set; }
        public float estimatedTotal { get; set; }
        public float estimatedGLPoints { get; set; }
        public float estimatedPlacement { get; set; }

    }

}
