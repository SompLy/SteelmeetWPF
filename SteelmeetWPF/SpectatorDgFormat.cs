using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteelmeetWPF
{
    class SpectatorDgFormat
    {
        public SpectatorDgFormat( Lifter lifter ) 
        { 
            Placement = lifter.place.ToString();
            this.name = lifter.name;
            this.lotNumber = lifter.lotNumber.ToString();
            this.weightClass = lifter.weightClass;
            this.accossiation = lifter.accossiation;
            this.bodyWeight = lifter.bodyWeight.ToString();
            this.squatHeight = lifter.squatHeight.ToString();
            this.benchHeight = lifter.benchHeight.ToString();
            this.benchRack = lifter.benchRack.ToString();
            this.s1 = lifter.s1.ToString();
            this.s2 = lifter.s2.ToString();
            this.s3 = lifter.s3.ToString();
            this.d1 = lifter.d1.ToString();
            this.d2 = lifter.d2.ToString();
            this.d3 = lifter.d3.ToString();
            this.b1 = lifter.b1.ToString();
            this.b2 = lifter.b2.ToString();
            this.b3 = lifter.b3.ToString();
            this.total = lifter.total.ToString();
            this.gLPoints = lifter.pointsGL.ToString();
            this.estimatedTotal = lifter.estimatedTotal.ToString();
            this.estimatedGLPoints = lifter.estimatedGLPoints.ToString();
            this.estimatedPlacement = lifter.estimatedPlacement.ToString();
        }
        [Display( Name = "#" )]
        public string? Placement { get; set; }

        [Display( Name = "Name" )]
        public string? name { get; set; }

        [Display( Name = "Lot" )]
        public string? lotNumber { get; set; }

        [Display( Name = "Class" )]
        public string? weightClass { get; set; }

        [Display( Name = "Association" )]
        public string? accossiation { get; set; }

        [Display( Name = "BW" )]
        public string? bodyWeight { get; set; }

        [Display( Name = "SHeight" )]
        public string? squatHeight { get; set; }

        [Display( Name = "BHeight" )]
        public string? benchHeight { get; set; }

        [Display( Name = "BRack" )]
        public string? benchRack { get; set; }

        [Display( Name = "S1" )]
        public string? s1 { get; set; }

        [Display( Name = "S2" )]
        public string? s2 { get; set; }

        [Display( Name = "S3" )]
        public string? s3 { get; set; }

        [Display( Name = "D1" )]
        public string? d1 { get; set; }

        [Display( Name = "D2" )]
        public string? d2 { get; set; }

        [Display( Name = "D3" )]
        public string? d3 { get; set; }

        [Display( Name = "B1" )]
        public string? b1 { get; set; }

        [Display( Name = "B2" )]
        public string? b2 { get; set; }

        [Display( Name = "B3" )]
        public string? b3 { get; set; }

        [Display( Name = "Total" )]
        public string? total { get; set; }

        [Display( Name = "GL" )]
        public string? gLPoints { get; set; }

        [Display( Name = "Est. Tot" )]
        public string? estimatedTotal { get; set; }

        [Display( Name = "Est. GL" )]
        public string? estimatedGLPoints { get; set; }

        [Display( Name = "Est. #" )]
        public string? estimatedPlacement { get; set; }
    }
}
