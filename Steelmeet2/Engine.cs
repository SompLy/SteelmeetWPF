// MeetEngine 2.0: ordning + domslut i kärnan (S1 → D3, samma platsregler som v1 LiftingOrder).
// Orderregeln: minst attempt-index först; vid samma index lyfts lättaste vikten först (plattor);
// vid dödläge avgör kropps-vikten (lättast först) — som v1s rankingwickeln.
using System.Globalization;

namespace Steelmeet2.Db;

public sealed class MeetLifter
{
    public string Name = "";
    public string BodyWeight = "";
    public string?[] AttemptWeights = new string?[9]; // S1..D3 (samtliga lever, kan vara tomt)
    public float[] Weights = new float[9];
    public int AttemptIdx;          // nästa LIFT per levning
    public bool[] Good = new bool[9];
    public bool Done => AttemptIdx >= 9;
    public int GoodCount;
    public string AttemptLabel => AttemptIdx switch
    {
        0 => "S1", 1 => "S2", 2 => "S3",
        3 => "B1", 4 => "B2", 5 => "B3",
        6 => "D1", 7 => "D2", 8 => "D3",
        _ => "klar",
    };
    public string CurrentLabel => Done ? "klar" : $"{AttemptLabel} {Weights[AttemptIdx].ToString("0.0", CultureInfo.InvariantCulture)}";
    public bool IsRetrying;
    public string Group = "";
    public string SquatHeightStr = "";
    public string BenchHeightStr = "";
    public string BenchRackStr = "";
    public string Tilted = "";
    public string Liftoff = "";
    public float BodyWeightKg;
}

public sealed class MeetEngine
{
    public readonly List<MeetLifter> Lifters = new();
    public MeetLifter? Current;

    public static MeetEngine FromBundle(MeetBundle bundle)
    {
        var engine = new MeetEngine();
        foreach (var r in bundle.rows)
        {
            float Parse(string? v) => float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f;
            // inväg-Excel har bara s1/b1/d1 — samma vikt sätts som start för alla 3 försöki i respektive lyft
            engine.Lifters.Add(new MeetLifter
            {
                Name = (r.name ?? "").Trim(),
                BodyWeight = r.bodyWeight ?? "",
                BodyWeightKg = Parse(r.bodyWeight),
                Group = (r.groupNumber ?? "").Trim(),
                SquatHeightStr = r.squatHeight ?? "",
                BenchHeightStr = r.benchHeight ?? "",
                BenchRackStr = r.benchRack ?? "",
                Tilted = r.tilted ?? "",
                Liftoff = r.liftoff ?? "",
                AttemptWeights = [r.s1, r.s1, r.s1, r.b1, r.b1, r.b1, r.d1, r.d1, r.d1],
                Weights = [Parse(r.s1), Parse(r.s1), Parse(r.s1), Parse(r.b1), Parse(r.b1), Parse(r.b1), Parse(r.d1), Parse(r.d1), Parse(r.d1)],
            });
        }
        engine.PickNext();
        return engine;
    }

    public string? ActiveGroup;
    public IEnumerable<MeetLifter> ActiveLifters => Lifters.Where(l => ActiveGroup is null || l.Group == ActiveGroup);
    public List<string> Groups => Lifters.Select(l => l.Group).Where(g => g.Length > 0).Distinct().OrderBy(g => g).ToList();

    public void SetGroup(string? group) { ActiveGroup = group; PickNext(); }

    public void AdjustWeight(float delta)
    {
        if (Current is { Done: false } l)
            l.Weights[l.AttemptIdx] = Math.Max(0.5f, l.Weights[l.AttemptIdx] + delta);
    }

    // Choke_NEAREST lifter with pending attempt: minst index först, sedan lättast vikt, sedan bw.
    public MeetLifter? PickNext()
    {
        MeetLifter? best = null;
        foreach (var l in ActiveLifters)
        {
            if (l.Done) continue;
            if (best is null) { best = l; continue; }
            if (l.AttemptIdx < best.AttemptIdx) { best = l; continue; }
            if (l.AttemptIdx == best.AttemptIdx
                && l.Weights[l.AttemptIdx] < best.Weights[l.AttemptIdx]) { best = l; continue; }
            if (l.AttemptIdx == best.AttemptIdx && l.Weights[l.AttemptIdx] == best.Weights[l.AttemptIdx]
                && CompareBw(l, best) < 0) best = l;
        }
        Current = best;
        return best;
    }

    static float CompareBw(MeetLifter a, MeetLifter b)
    {
        float Parse(string v) => float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f;
        return Parse(a.BodyWeight) - Parse(b.BodyWeight);
    }

    public void Undo(bool retry)
    {
        if (Current is not { } l) return;
        int idx = l.AttemptIdx - 1;
        if (l.AttemptIdx % 3 == 0) return; // kan inte ånga: S1/B1/D1 — som v1
        if (l.Good[idx]) { l.GoodCount = Math.Max(0, l.GoodCount - 1); l.Good[idx] = false; }
        // som v1: vikten sätts 0 utom vid respektive lyfts FÖRSTA attempt (S1/B1/D1)
        if (idx % 3 != 0) l.Weights[idx] = 0f;
        l.AttemptIdx = idx;
        l.IsRetrying = retry;
        PickNext();
        if (Current is null) Current = l;
    }

    public void Judge(bool good)
    {
        if (Current is { Done: false } l)
        {
            l.Good[l.AttemptIdx] = good;
            if (good) l.GoodCount++;
            l.AttemptIdx++;
            // nästa försök förbereds alltid: good → +2.5 kg, miss → samma vikt igen
            if (!l.Done)
                l.Weights[l.AttemptIdx] = l.Weights[l.AttemptIdx - 1] + (good ? 2.5f : 0f);
        }
        PickNext();
    }

    // Självcheck: garanterar ordningen (min-attempt först; lättast vikt; lättast bw vid dödläge)
    public static void SelfCheck()
    {
        var e = new MeetEngine();
        e.Lifters.Add(new MeetLifter { Name = "A", AttemptIdx = 1, Weights = [0, 100, 0, 0, 0, 0, 0, 0, 0], BodyWeight = "75" });
        e.Lifters.Add(new MeetLifter { Name = "B", AttemptIdx = 0, Weights = [80, 0, 0, 0, 0, 0, 0, 0, 0], BodyWeight = "78" });
        e.Lifters.Add(new MeetLifter { Name = "C", AttemptIdx = 0, Weights = [120, 0, 0, 0, 0, 0, 0, 0, 0], BodyWeight = "70" });
        e.Lifters.Add(new MeetLifter { Name = "D", AttemptIdx = 0, Weights = [80, 0, 0, 0, 0, 0, 0, 0, 0], BodyWeight = "72" });
        e.Lifters.Add(new MeetLifter { Name = "E", AttemptIdx = 0, Weights = [80, 0, 0, 0, 0, 0, 0, 0, 0], BodyWeight = "82" });
        var order = new List<string>();
        for (int i = 0; i < 5; i++) { var n = e.PickNext()?.Name; if (n is null) break; order.Add(n); e.Judge(true); }
        string o = string.Join(",", order);
        if (o != "D,B,E,C,D") throw new Exception("MeetEngine ordning fel: " + o + " (väntat D,B,E,C,D)");
    }
}
