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
                AttemptWeights = [r.s1, r.s1, r.s1, r.b1, r.b1, r.b1, r.d1, r.d1, r.d1],
                Weights = [Parse(r.s1), Parse(r.s1), Parse(r.s1), Parse(r.b1), Parse(r.b1), Parse(r.b1), Parse(r.d1), Parse(r.d1), Parse(r.d1)],
            });
        }
        engine.PickNext();
        return engine;
    }

    // Choke_NEAREST lifter with pending attempt: minst index först, sedan lättast vikt, sedan bw.
    public MeetLifter? PickNext()
    {
        MeetLifter? best = null;
        foreach (var l in Lifters)
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
