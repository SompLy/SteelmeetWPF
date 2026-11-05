// Excel I/O för 2.0 — portad FROM v1 ControlWindow (ExcelImport/ExcelExportHandler).
// 3-vägs-kontraktet: KOLUMNNUMMER 1-16 ↔ WeighRow-fält i ordning ↔ Svenska headers.
using SpreadsheetLight;

namespace Steelmeet2.Db;

public static class ExcelIO
{
    // Samma Svenska headers som v1 ExcelExportHandler — ORDNINGEN är kontraktet.
    static readonly string[] SweHeaders = new[]
    {
        "Grupp", "Namn", "Lot", "Klass", "Kategori", "Licensnr.", "Förening", "Kropps\nvikt",
        "Höjd\nBöj", "Infällt", "Böj", "Höjd\nBänk", "Rack\nBänk", "Avlyft", "Bänk", "Mark",
    };

    public static List<WeighInRow> Import(string path)
    {
        var rows = new List<WeighInRow>();
        using SLDocument sl = new(path);
        var stats = sl.GetWorksheetStatistics();
        int rowC = stats.NumberOfRows;

        for (int i = 1; i <= rowC; i++)
        {
            string col1 = sl.GetCellValueAsString(i, 1);
            if (string.IsNullOrWhiteSpace(col1)) break;
            if (col1 == "Grupp") continue; // rubrikrad
            rows.Add(new WeighInRow
            {
                groupNumber = sl.GetCellValueAsString(i, 1),
                name = sl.GetCellValueAsString(i, 2),
                lotNumber = sl.GetCellValueAsString(i, 3),
                weightClass = sl.GetCellValueAsString(i, 4),
                category = sl.GetCellValueAsString(i, 5),
                licenceNumber = sl.GetCellValueAsString(i, 6),
                accossiation = sl.GetCellValueAsString(i, 7),
                bodyWeight = sl.GetCellValueAsString(i, 8),
                squatHeight = sl.GetCellValueAsString(i, 9),
                tilted = sl.GetCellValueAsString(i, 10),
                s1 = sl.GetCellValueAsString(i, 11),
                benchHeight = sl.GetCellValueAsString(i, 12),
                benchRack = sl.GetCellValueAsString(i, 13),
                liftoff = sl.GetCellValueAsString(i, 14),
                b1 = sl.GetCellValueAsString(i, 15),
                d1 = sl.GetCellValueAsString(i, 16),
            });
        }
        return rows;
    }

    public static void Export(string path, IReadOnlyList<WeighInRow> rows)
    {
        using SLDocument sl = new();
        for (int c = 0; c < SweHeaders.Length; c++)
            sl.SetCellValue(1, c + 1, SweHeaders[c]);

        for (int r = 0; r < rows.Count; r++)
        {
            WeighInRow it = rows[r];
            string?[] vals = new string?[]
            {
                it.groupNumber, it.name, it.lotNumber, it.weightClass, it.category, it.licenceNumber,
                it.accossiation, it.bodyWeight, it.squatHeight, it.tilted, it.s1, it.benchHeight,
                it.benchRack, it.liftoff, it.b1, it.d1,
            };
            for (int c = 0; c < vals.Length; c++)
                sl.SetCellValue(r + 2, c + 1, vals[c]);
        }
        sl.SaveAs(path);
    }
}
