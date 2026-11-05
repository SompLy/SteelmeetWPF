// WeighInDgFormat-kontraktet i 2.0: SAMMA fältnamn i SAMMA ordning som v1
// (WeighInDgFormat 16 kolumner) — Excel 3-vägs-kontraktet (import/export/DataGrid) är heligt.
using System.ComponentModel.DataAnnotations;

namespace Steelmeet2.Db;

public class WeighInRow
{
    [Display(Name = "Grupp")]
    public string? groupNumber { get; set; }

    [Display(Name = "Namn")]
    public string? name { get; set; }

    [Display(Name = "Lot")]
    public string? lotNumber { get; set; }

    [Display(Name = "Klass")]
    public string? weightClass { get; set; }

    [Display(Name = "Kategori")]
    public string? category { get; set; }

    [Display(Name = "Licensnr.")]
    public string? licenceNumber { get; set; }

    [Display(Name = "Förening")]
    public string? accossiation { get; set; }

    [Display(Name = "Kroppsvikt")]
    public string? bodyWeight { get; set; }

    [Display(Name = "Höjd Böj")]
    public string? squatHeight { get; set; }

    [Display(Name = "Infällt")]
    public string? tilted { get; set; }

    [Display(Name = "Böj")]
    public string? s1 { get; set; }

    [Display(Name = "Höjd Bänk")]
    public string? benchHeight { get; set; }

    [Display(Name = "Rack Bänk")]
    public string? benchRack { get; set; }

    [Display(Name = "Avlyft")]
    public string? liftoff { get; set; }

    [Display(Name = "Bänk")]
    public string? b1 { get; set; }

    [Display(Name = "Mark")]
    public string? d1 { get; set; }
}

public sealed class MeetBundle
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string date { get; set; } = "";
    public string category { get; set; } = "";
    public string organizer { get; set; } = "";
    public string updatedAt { get; set; } = "";
    public List<WeighInRow> rows { get; set; } = new();
}

public sealed class MeetMeta
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string date { get; set; } = "";
    public string category { get; set; } = "";
    public string organizer { get; set; } = "";
    public string updated_at { get; set; } = "";
}

public sealed class MeetsPage
{
    public List<MeetMeta> Results { get; set; } = new();
}
