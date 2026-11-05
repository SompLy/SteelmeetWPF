using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Steelmeet2.Db;
using System.Collections.ObjectModel;

namespace Steelmeet2;

public partial class MainWindow : Window
{
    readonly DbClient client = new();
    MeetBundle? currentMeet;
    readonly ObservableCollection<WeighInRow> weighInRows = new();
    MeetEngine? engine;

    // timrar
    DispatcherTimer? liftTimer, lappTimer;
    int liftSec, lappSec;

    public MainWindow(string? openMeetId = null)
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshNetStatusAsync();
        FetchBtn.Click += async (_, _) => await FetchCompetitionsAsync();
        FlushBtn.Click += async (_, _) =>
        {
            int n = await DbClient.FlushQueueAsync();
            DbStatus.Text = $"kö flushad: {n} item(s) skickade";
        };

        WeighInDg.ItemsSource = weighInRows;
        var clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clockTimer.Tick += ClockTextCallback;
        clockTimer.Start();
        Clock.Text = DateTime.Now.ToString("HH:mm:ss");
        NewMeetBtn.Click += async (_, _) => await CreateOrSaveMeetAsync(true);
        PublishBtn.Click += async (_, _) => await CreateOrSaveMeetAsync(false);
        LoadMeetsBtn.Click += async (_, _) => await LoadMeetsAsync();
        OpenMeetBtn.Click += async (_, _) => await OpenSelectedMeetAsync();
        ImportBtn.Click += async (_, _) => await ImportExcelAsync();
        ExportBtn.Click += async (_, _) => await ExportExcelAsync();

        // domarpanel
        GoodBtn.Click += (_, _) => JudgeAndRefresh(true);
        BadBtn.Click += (_, _) => JudgeAndRefresh(false);
        UndoBtn.Click += (_, _) => UndoAndRefresh(false);
        RetryBtn.Click += (_, _) => UndoAndRefresh(true);
        NextLifterBtn.Click += (_, _) => { engine?.PickNext(); UpdateTavlingUI("nästa vald"); };
        BarReadyBtn.Click += (_, _) => { engine?.PickNext(); TimerController(TimerOpt.LIFT_ONE_MINUTE); UpdateTavlingUI("bar ready — 1:00 startad"); };

        // lyftklocka
        LiftMinutesBtn.Click += (_, _) =>
        {
            if (LiftMinutesCob.SelectedItem is ComboBoxItem it && int.TryParse(it.Content?.ToString(), out int min))
                TimerController(TimerOpt.CUSTOM_TIME, min, 0);
        };
        LiftResetBtn.Click += (_, _) => TimerController(TimerOpt.LIFT_RESET);
        // lappklocka
        StampSetBtn.Click += (_, _) => SetLappFromTimestamp();
        StampResetBtn.Click += (_, _) => TimerController(TimerOpt.LAPP_RESET);
        for (int m = 1; m <= 60; m++) LiftMinutesCob.Items.Add(new ComboBoxItem { Content = m.ToString() });

        // viktkorrectioner: current attempt ± inc
        foreach (var (btn, delta) in new (Button, float)[]
        {
            (Plus2_5, 2.5f), (Plus5, 5f), (Plus7_5, 7.5f), (Plus10, 10f), (Plus12_5, 12.5f),
            (Plus15, 15f), (Plus17_5, 17.5f), (Plus20, 20f), (Plus25, 25f),
            (Minus2_5, -2.5f), (Minus5, -5f), (Minus7_5, -7.5f),
        })
        {
            float d = delta;
            btn.Click += (_, _) => { engine?.AdjustWeight(d); UpdateTavlingUI($"viktjustering {d:+0.0;-0.0}"); };
        }
        RefreshComboFilled = false;

        // --meet <id>: öppna ett möte direkt vid start (t ex demo/lansering)
        if (openMeetId is not null)
            Loaded += async (_, _) => await LoadMeetIntoAppAsync(openMeetId);
    }

    bool RefreshComboFilled;

    void Nav_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PageTitle is null) return;
        PageTitle.Text = ((ListBoxItem?)NavList.SelectedItem)?.Content is TextBlock tb ? tb.Text : "";
        PageWeighIn.IsVisible = NavList.SelectedIndex == 0;
        PageComp.IsVisible = NavList.SelectedIndex == 1;
        PageJudge.IsVisible = NavList.SelectedIndex == 2;
        PageResults.IsVisible = NavList.SelectedIndex == 3;
        PageDb.IsVisible = NavList.SelectedIndex == 4;
        PageSettings.IsVisible = NavList.SelectedIndex == 5;
    }

    // ==============================  TIMERS  ==============================
    enum TimerOpt { LIFT_ONE_MINUTE, CUSTOM_TIME, LAPP_ONE_MINUTE, LIFT_RESET, LAPP_RESET }

    void TimerController(TimerOpt opt, int customMin = 0, int customSec = 0)
    {
        switch (opt)
        {
            case TimerOpt.LIFT_ONE_MINUTE: liftSec = 60; EnsureLiftTimer().Start(); break;
            case TimerOpt.CUSTOM_TIME: liftSec = customMin * 60 + customSec; EnsureLiftTimer().Start(); break;
            case TimerOpt.LAPP_ONE_MINUTE: lappSec = 60; EnsureLappTimer().Start(); break;
            case TimerOpt.LIFT_RESET: liftSec = 0; liftTimer?.Stop(); break;
            case TimerOpt.LAPP_RESET: lappSec = 0; lappTimer?.Stop(); break;
        }
        PaintClocks();
    }

    DispatcherTimer EnsureLiftTimer()
    {
        if (liftTimer is not null) return liftTimer;
        liftTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        liftTimer.Tick += (_, _) =>
        {
            liftSec--;
            if (liftSec <= 0) { liftTimer!.Stop(); liftSec = 0; }
            PaintClocks();
        };
        return liftTimer;
    }

    DispatcherTimer EnsureLappTimer()
    {
        if (lappTimer is not null) return lappTimer;
        lappTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        lappTimer.Tick += (_, _) =>
        {
            lappSec--;
            if (lappSec <= 0) { lappTimer!.Stop(); lappSec = 0; }
            PaintClocks();
        };
        return lappTimer;
    }

    void PaintClocks()
    {
        TimerBig.Text = $"{liftSec / 60:0}:{liftSec % 60:00}";
        TimerBig.Foreground = liftTimer is { IsEnabled: true }
            ? (liftSec <= 10 ? Brushes.Orange : Brushes.MediumSpringGreen)
            : Brushes.White;

        LappTimerBig.Text = $"{lappSec / 60:0}:{lappSec % 60:00}";
        LappTimerBig.Foreground = lappTimer is { IsEnabled: true } ? Brushes.IndianRed : Brushes.Gray;
    }

    void SetLappFromTimestamp()
    {
        // LAPP = tid tills vald klockslag (hour:min) = som v1 TimestampBtn
        int.TryParse((StampHourCob.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int hour);
        int.TryParse((StampMinCob.SelectedItem as ComboBoxItem)?.Content?.ToString(), out int min);
        var now = DateTime.Now;
        int diffMin = (hour * 60) - now.Hour * 60;
        diffMin += min - now.Minute - 1;
        int diffSec = 60 - now.Second;
        if (diffMin < 0) diffMin += 24 * 60; // nästa dag-rulle
        lappSec = diffMin * 60 + diffSec;
        EnsureLappTimer().Start();
        PaintClocks();
    }

    // ==============================  TÄVLING  ==============================
    void JudgeAndRefresh(bool good)
    {
        if (engine is null) { CompStatus.Text = "öppna ett möte först"; return; }
        engine.Judge(good);
        TimerController(TimerOpt.LAPP_ONE_MINUTE); // som v1: 1-min lapp efter domslut
        TimerController(TimerOpt.LIFT_RESET);
        UpdateTavlingUI(good ? "godkänd ✓" : "misslyckad ✗");
    }

    void UndoAndRefresh(bool retry)
    {
        if (engine is null) { CompStatus.Text = "öppna ett möte först"; return; }
        int before = engine.Current?.AttemptIdx ?? 0;
        engine.Undo(retry);
        UpdateTavlingUI(retry ? "RETRY — försöket returneras" : "UNDO — försöket återställs");
    }

    void UpdateTavlingUI(string? statusText = null)
    {
        if (engine is null) return;

        var pending = engine.ActiveLifters.Where(l => !l.Done).ToList();
        pending.Sort((a, b) =>
        {
            int c = a.AttemptIdx.CompareTo(b.AttemptIdx);
            if (c != 0) return c;
            c = a.Weights[a.AttemptIdx].CompareTo(b.Weights[b.AttemptIdx]);
            if (c != 0) return c;
            return a.BodyWeightKg.CompareTo(b.BodyWeightKg);
        });

        var now = engine.Current;
        if (now is null)
        {
            CompNowName.Text = "KLART";
            CompNowAttempt.Text = "alla försök gjorda i gruppen";
        }
        else
        {
            CompNowName.Text = now.Name + (now.IsRetrying ? "  (RETRY)" : "");
            CompNowAttempt.Text = $"{now.AttemptLabel}  {now.Weights[now.AttemptIdx].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} kg";

            // Höjdrader som v1 LifterInfoControl: böj-Höjd under knäböj, höjd/rack under bänk
            if (now.AttemptIdx <= 2)
            {
                CompNowHeights.Text = "Höjd " + now.SquatHeightStr;
                CompNowExtras.Text = Tag(now.Tilted);
            }
            else if (now.AttemptIdx <= 5)
            {
                CompNowHeights.Text = "Höjd " + now.BenchHeightStr + "/" + now.BenchRackStr;
                CompNowExtras.Text = Tag(now.Liftoff);
            }
            else { CompNowHeights.Text = ""; CompNowExtras.Text = ""; }
        }

        var next = pending.Count > 1 ? pending[1] : null;
        CompNextName.Text = next?.Name ?? "—";
        CompNextAttempt.Text = next is null ? "" : $"{next.AttemptLabel}  {next.Weights[next.AttemptIdx].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} kg";
        CompNextHeights.Text = next is null ? "" : (next.AttemptIdx <= 2
            ? "Höjd " + next.SquatHeightStr : "Höjd " + next.BenchHeightStr + "/" + next.BenchRackStr);

        // ordning
        var items = new List<string>();
        int i = 1;
        foreach (var l in pending)
            items.Add($"{i++}. " + l.Name.PadRight(18) + " " + l.AttemptLabel + "  " + l.Weights[l.AttemptIdx].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " kg" + (l.IsRetrying ? "  ⟲" : ""));
        OrderList.ItemsSource = items;

        // föreslagna vikter = current ± inc
        float baseW = now is null || now.Done ? 0f : now.Weights[now.AttemptIdx];
        string F(float v) => v.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        Plus2_5.Content = F(baseW + 2.5f); Plus5.Content = F(baseW + 5f); Plus7_5.Content = F(baseW + 7.5f);
        Plus10.Content = F(baseW + 10f); Plus12_5.Content = F(baseW + 12.5f); Plus15.Content = F(baseW + 15f);
        Plus17_5.Content = F(baseW + 17.5f); Plus20.Content = F(baseW + 20f); Plus25.Content = F(baseW + 25f);
        Minus2_5.Content = F(baseW - 2.5f); Minus5.Content = F(baseW - 5f); Minus7_5.Content = F(baseW - 7.5f);
        SuggHeader.Text = $"VIKT-KORREKTERING ({now?.Name ?? "—"})";
        RetryTag.Text = now?.IsRetrying == true ? "RETRY AKTIV" : "";

        if (statusText is not null) CompStatus.Text = statusText;
        NextMiniOrder.Text = string.Join("  →  ", pending.Take(3).Select(l => l.Name));
    }

    static string Tag(string v) =>
        new[] { "Nej", "No", "" }.Any(s => v.Contains(s)) ? "" : v;

    void ActiveGroupCob_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (engine is null || !RefreshComboFilled) return;
        if (ActiveGroupCob.SelectedItem is ComboBoxItem it)
        {
            string g = it.Content?.ToString() ?? "";
            engine.SetGroup(g.Length == 0 ? null : g);
            UpdateTavlingUI($"grupp {g} aktiv");
        }
    }

    void HeightCob_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (engine?.Current is not { Done: false } l) return;
        if (HeightCob.SelectedItem is ComboBoxItem it && int.TryParse(it.Content?.ToString(), out int hV))
        {
            if (l.AttemptIdx <= 2) l.SquatHeightStr = hV.ToString();
            else l.BenchHeightStr = hV.ToString();
            UpdateTavlingUI();
        }
    }

    void RackCob_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (engine?.Current is not { Done: false } l) return;
        if (RackCob.SelectedItem is ComboBoxItem it && int.TryParse(it.Content?.ToString(), out int rV))
        {
            l.BenchRackStr = rV.ToString();
            UpdateTavlingUI();
        }
    }

    void RecordToggled(object? sender, RoutedEventArgs e) =>
        RecordInfo.Text = RecordOn.IsChecked == true ? "aktiv — rekor-drag kopplas när SSF-rekor importerats" : "";

    // ==============================  DATABAS/INVÄGNING  ==============================
    async Task RefreshNetStatusAsync()
    {
        try
        {
            using var ping = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            await ping.GetStringAsync("https://db.steelmeet.se/db/meets");
            NetStatus.Text = "● ONLINE"; NetStatus.Foreground = Brushes.MediumSpringGreen;
        }
        catch
        {
            NetStatus.Text = "● OFFLINE (cache)"; NetStatus.Foreground = Brushes.Orange;
        }
    }

    async Task FetchCompetitionsAsync()
    {
        FetchBtn.IsEnabled = false;
        DbStatus.Text = "hämtar …";
        try
        {
            var comps = await client.GetCompetitionsAsync();
            DbStatus.Text = $"OK — {comps.Count} tävlingar (cache: {OfflineCache.CachePath("get", "competitions")})";
            CompList.ItemsSource = comps.Select(c => $"{c.Id}  {c.Name}  ({c.StartDate})  {c.State}");
        }
        catch (Exception ex)
        {
            DbStatus.Text = "FAIL: " + ex.Message;
            CompList.ItemsSource = new[] { "(ingen cache) — online krävs första gången" };
        }
        FetchBtn.IsEnabled = true;
    }

    // ---------- möten ----------
    async Task CreateOrSaveMeetAsync(bool newMeet)
    {
        string pin = MeetPin.Text?.Trim() ?? "";
        string name = MeetName.Text?.Trim() ?? "";
        string date = MeetDate.Text?.Trim() ?? "";
        if (name.Length == 0 || pin.Length == 0) { WeighStatus.Text = "mötesnamn + PIN krävs"; return; }

        if (newMeet)
        {
            currentMeet = new MeetBundle { id = "m" + DateTime.Now.ToString("yyyyMMddHHmmss"), name = name, date = date, category = "", organizer = "steelmeet2" };
            WeighStatus.Text = $"skapat {currentMeet.id} (lokalt)";
        }
        else if (currentMeet is null) { WeighStatus.Text = "öppna/skapa ett möte först"; return; }

        currentMeet!.name = name; currentMeet!.date = date;
        currentMeet!.rows = weighInRows.ToList();
        bool send = await client.SaveMeetAsync(currentMeet, pin);
        WeighStatus.Text = send
            ? $"PUBLICERAT: {currentMeet.id} ({weighInRows.Count} rader)"
            : $"OFFLINE eller fel — payloaden ligger i kön, FLUSH KÖ på Databas-sidan skickar om den";
    }

    async Task LoadMeetsAsync()
    {
        try
        {
            var meets = (await client.GetMeetsAsync()).Results;
            MeetsCombo.ItemsSource = meets.Select(m => $"{m.id} — {m.name} ({m.date})").ToList();
            WeighStatus.Text = $"{meets.Count} möten i databasen";
            if (meets.Count > 0) MeetsCombo.SelectedIndex = 0;
        }
        catch (Exception ex) { WeighStatus.Text = "lista misslyckades: " + ex.Message; }
    }

    async Task OpenSelectedMeetAsync()
    {
        var meta = MeetsCombo.SelectedItem as string;
        if (meta is null) { WeighStatus.Text = "välj möte i listan"; return; }
        string id = meta.Split(" — ")[0];
        MeetPin.Text = "";
        await LoadMeetIntoAppAsync(id);
    }

    async Task LoadMeetIntoAppAsync(string id)
    {
        try
        {
            var bundle = await client.GetMeetAsync(id);
            currentMeet = bundle;
            WeighInRowCollectionReplace(bundle.rows);
            MeetName.Text = bundle.name; MeetDate.Text = bundle.date;
            WeighStatus.Text = $"möte {id}: {bundle.rows.Count} rader (PIN behövs vid publicering)";

            // seed Tävling-motorn från invägningen
            engine = MeetEngine.FromBundle(bundle);
            FillOptionCombos();
            UpdateTavlingUI($"möte {id} laddat — grupp {(engine.ActiveGroup ?? "alla")}");
            TimerController(TimerOpt.LIFT_RESET);
            NavList.SelectedIndex = 1; // hoppa till Tävling-sidan
        }
        catch (Exception ex) { WeighStatus.Text = "öppna misslyckades: " + ex.Message; }
    }

    void FillOptionCombos()
    {
        RefreshComboFilled = false;
        var groups = engine?.Groups ?? new List<string>();
        ActiveGroupCob.ItemsSource = (new[] { "alla" }).Concat(groups).Select(g => new ComboBoxItem { Content = g }).ToList();
        ActiveGroupCob.SelectedIndex = 0;
        HeightCob.ItemsSource = Enumerable.Range(0, 30).Select(i => new ComboBoxItem { Content = i.ToString() }).ToList();
        HeightCob.SelectedIndex = 12;
        RackCob.ItemsSource = Enumerable.Range(0, 15).Select(i => new ComboBoxItem { Content = i.ToString() }).ToList();
        RackCob.SelectedIndex = 12;
        RefreshComboFilled = true;
        UpdateTavlingUI();
    }

    void WeighInRowCollectionReplace(List<WeighInRow> rows)
    {
        weighInRows.Clear();
        foreach (var r in rows) weighInRows.Add(r);
    }

    // ---------- excel ----------
    async Task ImportExcelAsync()
    {
        var top = TopLevel.GetTopLevel(this) ?? throw new Exception("inget fönster");
        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Importera invägning (xlsx)",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { new("Excel") { Patterns = new[] { "*.xlsx", "*.xls" } } },
        });
        if (files.Count == 0) return;
        try
        {
            var rows = ExcelIO.Import(files[0].Path.LocalPath);
            WeighInRowCollectionReplace(rows);
            currentMeet ??= new MeetBundle { id = "m" + DateTime.Now.ToString("yyyyMMddHHmmss") };
            WeighStatus.Text = $"importerat {rows.Count} rader — SÄRNAMN och PIN för att publicera";
        }
        catch (Exception ex) { WeighStatus.Text = "importfel: " + ex.Message; }
    }

    async Task ExportExcelAsync()
    {
        var top = TopLevel.GetTopLevel(this) ?? throw new Exception("inget fönster");
        var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exportera invägning",
            SuggestedFileName = "Steelmeet_2_Start",
            FileTypeChoices = new List<FilePickerFileType> { new("Excel fil") { Patterns = new[] { "*.xlsx" } } },
        });
        if (file is null) return;
        ExcelIO.Export(file.Path.LocalPath, weighInRows.ToList());
        WeighStatus.Text = "Excel fil exporterad utan besvär! :)";
    }

    // ---------- klock ----------
    void ClockTextCallback(object? sender, EventArgs e) => Clock.Text = DateTime.Now.ToString("HH:mm:ss");

    // klock varje sekund:
    //   DispatcherTimer clockTimer sattes i ctor — används via method ClockTextCallback
}
