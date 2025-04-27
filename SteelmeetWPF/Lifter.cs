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
            LiftRecord = new List<bool>();
            sbdListWeight = new List<float>() { this.s1, s2, s3, this.b1, b2, b3, this.d1, d2, d3 };
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
            LiftRecord = new List<bool>();
            sbdListWeight = new List<float>() { this.s1, s2, s3, this.b1, b2, b3, this.d1, d2, d3 };
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
        public List<bool> LiftRecord { get; set; } //en lista med true eller false beroende på om lyftaren fick godkänt eller inte
        public List<float> sbdListWeight { get; set; }
        public int index { get; set; }
        public float estimatedTotal { get; set; }
        public float estimatedGLPoints { get; set; }
        public float estimatedPlacement { get; set; }

    }

}
