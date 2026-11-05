using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Steelmeet2.Db;

namespace Steelmeet2;

public partial class MainWindow : Window
{
    readonly DbClient client = new();

    public MainWindow()
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
            CompList.ItemsSource = comps.Select(c => $"{c.CompetitionId}  {c.Name}  ({c.StartDate:yyyy-MM-dd})  {c.Status}");
        }
        catch (Exception ex)
        {
            DbStatus.Text = $"FAIL: {ex.Message}";
            CompList.ItemsSource = new[] { "(ingen cache) — online krävs första gången" };
        }
        FetchBtn.IsEnabled = true;
    }
}
