using System.ComponentModel.DataAnnotations;

namespace SteelmeetWPF
{
    public class WeighInDgFormat
    {
        [Display( Name = "Group" )]
        public string? groupNumber { get; set; }

        [Display( Name = "Name" )]
        public string? name { get; set; }

        [Display( Name = "Lot" )]
        public string? lotNumber { get; set; }

        [Display( Name = "Weight\nClass" )]
        public string? weightClass { get; set; }

        [Display( Name = "Category" )]
        public string? category { get; set; }

        [Display( Name = "Licence\nNumber" )]
        public string? licenceNumber { get; set; }

        [Display( Name = "Association" )]
        public string? accossiation { get; set; }

        [Display( Name = "Body\nWeight" )]
        public string? bodyWeight { get; set; }

        [Display( Name = "Squat\nHeight" )]
        public string? squatHeight { get; set; }

        [Display( Name = "Tilted" )]
        public string? tilted { get; set; }

        [Display( Name = "Squat" )]
        public string? s1 { get; set; }

        [Display( Name = "Bench\nHeight" )]
        public string? benchHeight { get; set; }

        [Display( Name = "Bench\nRack" )]
        public string? benchRack { get; set; }

        [Display( Name = "Liftoff" )]
        public string? liftoff { get; set; }

        [Display( Name = "Bench" )]
        public string? b1 { get; set; }

        [Display( Name = "Deadlift" )]
        public string? d1 { get; set; }
    }
}
