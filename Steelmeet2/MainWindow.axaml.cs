using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
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
    DispatcherTimer? liftTimer;
    int timerSecondsLeft = 120;

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

        var clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        clockTimer.Tick += (_, _) => Clock.Text = DateTime.Now.ToString("HH:mm:ss");
        clockTimer.Start();
        Clock.Text = DateTime.Now.ToString("HH:mm:ss");

        WeighInDg.ItemsSource = weighInRows;
        NewMeetBtn.Click += async (_, _) => await CreateOrSaveMeetAsync(true);
        PublishBtn.Click += async (_, _) => await CreateOrSaveMeetAsync(false);
        LoadMeetsBtn.Click += async (_, _) => await LoadMeetsAsync();
        OpenMeetBtn.Click += async (_, _) => await OpenSelectedMeetAsync();
        ImportBtn.Click += async (_, _) => await ImportExcelAsync();
        ExportBtn.Click += async (_, _) => await ExportExcelAsync();

        // Tävling-sidan: engine + timer + domslut
        GoodBtn.Click += (_, _) => JudgeAndRefresh(true);
        BadBtn.Click += (_, _) => JudgeAndRefresh(false);
        NextLiftBtn.Click += (_, _) => BumpCurrentWeight();
        TimerStart.Click += (_, _) =>
        {
            if (liftTimer is null)
            {
                liftTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                liftTimer.Tick += (_, _) =>
                {
                    timerSecondsLeft--;
                    if (timerSecondsLeft <= 0) { StopTimer(); TimerBig.Foreground = Avalonia.Media.Brushes.IndianRed; }
                    UpdateTimerText();
                };
                liftTimer.Start();
            }
            else
            {
                if (liftTimer.IsEnabled) StopTimer(); else liftTimer.Start();
            }
            UpdateTimerText();
        };
        TimerReset.Click += (_, _) => { StopTimer(); timerSecondsLeft = 120; TimerBig.Foreground = Avalonia.Media.Brushes.White; UpdateTimerText(); };

#if DEBUG
        try { MeetEngine.SelfCheck(); }
        catch (Exception ex) { Console.Error.WriteLine("SelfCheck FAIL: " + ex.Message); }
#endif

        // --meet <id>: öppna ett möte direkt vid start (t ex demo/lansering)
        if (openMeetId is not null)
            Loaded += async (_, _) => await LoadMeetIntoAppAsync(openMeetId);
    }

    void UpdateTimerText()
    {
        TimerBig.Text = $"{timerSecondsLeft / 60}:{(timerSecondsLeft % 60):00}";
        TimerBig.Foreground = liftTimer is { IsEnabled: true }
            ? Avalonia.Media.Brushes.MediumSpringGreen
            : Avalonia.Media.Brushes.White;
    }

    void StopTimer() { liftTimer?.Stop(); }

    void JudgeAndRefresh(bool good)
    {
        engine?.Judge(good);
        var n = engine?.Current;
        CompNowName.Text = n is null ? "KLART — alla försök är gjorda" : n.Name;
        CompNowAttempt.Text = n is null ? "" : $"{n.AttemptLabel}  {n.Weights[n.AttemptIdx].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} kg   ({n.GoodCount} good / {n.Name})";
        RefreshOrderList();
    }

    void RefreshOrderList()
    {
        if (engine is null) return;
        var pending = new List<string>();
        var sorted = engine.Lifters.Where(l => !l.Done).ToList();
        sorted.Sort((a, b) =>
        {
            int c = a.AttemptIdx.CompareTo(b.AttemptIdx);
            if (c != 0) return c;
            c = a.Weights[a.AttemptIdx].CompareTo(b.Weights[b.AttemptIdx]);
            return c;
        });
        foreach (var l in sorted)
            pending.Add($"{l.Name,-20} {l.AttemptLabel}  {l.Weights[l.AttemptIdx].ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)}");
        for (int i = 0; i < sorted.Count; i++)
            pending[i] = pending[i].Insert(0, $"{i + 1}.      ");
        OrderList.ItemsSource = pending;
    }

    // VIKA VIKT: flytta upp nuvarande attempt med 2.5 kg (plats-lock)? — nej, det är "next lifter up" list→ kärnan"  "  "  "  "  "  "  "  "  "
    void BumpCurrentWeight()
    {
        if (engine?.Current is null)
        {
            RefreshOrderList(); return;
        }
        var l = engine.Current;
        l.Weights[l.AttemptIdx] += 2.5f;
        RefreshOrderList();
        RefreshOrderList();
        CompStatus.Text = $"VIKA VIKT: {l.Name} → {l.CurrentLabel}";
        var n = engine.Current;
        CompNowName.Text = n?.Name ?? "KLART";
        CompNowAttempt.Text = n?.CurrentLabel ?? "";
    }

    // ---------- möten ----------
    async Task CreateOrSaveMeetAsync(bool newMeet)
    {
        string pin = MeetPin.Text?.Trim() ?? "";
        string name = MeetName.Text?.Trim() ?? "";
        string date = MeetDate.Text?.Trim() ?? "";
        if (name.Length == 0 || pin.Length == 0) { WeighStatus.Text = "mötesnamn + PIN krävs"; return; }

        bool send;
        if (newMeet)
        {
            currentMeet = new MeetBundle { id = "m" + DateTime.Now.ToString("yyyyMMddHHmmss"), name = name, date = date, category = "", organizer = "steelmeet2" };
            WeighStatus.Text = $"skapat {currentMeet.id} (lokalt)";
        }
        else if (currentMeet is null) { WeighStatus.Text = "öppna/skapa ett möte först"; return; }

        currentMeet!.name = name; currentMeet!.date = date;
        currentMeet!.rows = weighInRows.ToList();
        send = await client.SaveMeetAsync(currentMeet, pin);
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
            RefreshOrderList();
            var first = engine.Current;
            CompNowName.Text = first?.Name ?? "—";
            CompNowAttempt.Text = first?.CurrentLabel ?? "";
            TimerReset_Click();
            NavList.SelectedIndex = 1; // hoppa till Tävling-sidan
        }
        catch (Exception ex) { WeighStatus.Text = "öppna misslyckades: " + ex.Message; }
    }

    void TimerReset_Click()
    {
        StopTimer(); timerSecondsLeft = 120; UpdateTimerText();
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
            WeighStatus.Text = $"importerat {rows.Count} rader — SÄTNAMN och PIN för att publicera";
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

    void Nav_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PageTitle is null) return;
        int i = NavList.SelectedIndex;
        PageTitle.Text = ((ListBoxItem?)NavList.SelectedItem)?.Content is TextBlock tb ? tb.Text : "";
        PageWeighIn.IsVisible = i == 0;
        PageComp.IsVisible = i == 1;
        PageJudge.IsVisible = i == 2;
        PageResults.IsVisible = i == 3;
        PageDb.IsVisible = i == 4;
        PageSettings.IsVisible = i == 5;
    }

    async Task RefreshNetStatusAsync()
    {
        try
        {
            using var ping = new HttpClient { Timeout = TimeSpan.FromSeconds(6) };
            await ping.GetStringAsync("https://db.steelmeet.se/api/competitions?limit=1");
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
        DbStatus.Text = $"hämtar …";
        try
        {
            var comps = await client.GetCompetitionsAsync();
            DbStatus.Text = $"OK — {comps.Count} tävlingar (cache: {OfflineCache.CachePath("get", "competitions")})";
            CompList.ItemsSource = comps.Select(c => $"{c.Id}  {c.Name}  ({c.StartDate})  {c.State}  [{c.Location}]  anm\u00e4lda: {c.RegistrationsCount}");
        }
        catch (Exception ex)
        {
            DbStatus.Text = $"FAIL: {ex.Message}";
            CompList.ItemsSource = new[] { "(ingen cache) — online krävs första gången" };
        }
        FetchBtn.IsEnabled = true;
    }
}
