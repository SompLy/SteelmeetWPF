// db.steelmeet.se-klient: SM2:s länk till databasen.
// Offline-först: GET går viaWriter cache (disk), upload köar och försöker igen automatiskt.
using System.Net.Http.Json;
using System.Text.Json;

namespace Steelmeet2.Db;

public sealed class DbClient
{
    const string Base = "https://db.steelmeet.se";
    static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    internal static readonly JsonSerializerOptions Opts = JsonOpts;

    readonly HttpClient http = new() { BaseAddress = new Uri(Base + "/"), Timeout = TimeSpan.FromSeconds(15) };

    // ---------- GET med disk-cache (offline) ----------
    public async Task<T> GetAsync<T>(string path, string cacheKey)
    {
        string cacheFile = OfflineCache.CachePath("get", cacheKey);
        try
        {
            var data = await http.GetFromJsonAsync<T>(path, JsonOpts) ?? throw new Exception("tomt svar");
            OfflineCache.Save(cacheFile, JsonSerializer.Serialize(data, JsonOpts));
            OfflineCache.MarkOnline();
            return data;
        }
        catch // offline eller upstream-fel: fallback till cache
        {
            OfflineCache.MarkOffline();
            string raw = OfflineCache.Load(cacheFile);
            if (raw.Length == 0) throw;
            return JsonSerializer.Deserialize<T>(raw, JsonOpts)
                ?? throw new Exception("cache corrupt: " + cacheKey);
        }
    }

    // ---------- POST med kö (offline-first) ----------
    public async Task<bool> PostDefaultAsync(string path, object payload, string queueKey)
    {
        try
        {
            var res = await http.PostAsJsonAsync(path, payload, JsonOpts);
            OfflineCache.MarkOnline();
            return res.IsSuccessStatusCode;
        }
        catch
        {
            OfflineCache.MarkOffline();
            OfflineCache.Enqueue(queueKey, path, payload);
            return false;
        }
    }

    public static async Task<int> FlushQueueAsync()
    {
        int sent = 0;
        foreach (var (key, path, json) in OfflineCache.ReadAllQueue())
        {
            try
            {
                using var http = new HttpClient { BaseAddress = new Uri(Base + "/"), Timeout = TimeSpan.FromSeconds(15) };
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var res = await http.PostAsync(path, content);
                if (res.IsSuccessStatusCode)
                {
                    OfflineCache.Dequeue(key);
                    sent++;
                }
            }
            catch { break; } // fortfarande offline — kör igen nästa flush
        }
        return sent;
    }

    // ---------- Tillåtna SM-endpoints (ligger på db.steelmeet.se) ----------
    public Task<List<ApiCompetition>> GetCompetitionsAsync() =>
        GetAsync<List<ApiCompetition>>("api/competitions?limit=50", "competitions");

    public Task<ApiCompetition> GetCompetitionAsync(string id) =>
        GetAsync<ApiCompetition>($"api/competitions/{id}", "comp_" + id);
}

public record ApiCompetition(string CompetitionId, string Name, DateTime StartDate, string Status);

public static class OfflineCache
{
    static readonly string Root =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steelmeet2");

    public static void MarkOnline() => Online = true;
    public static void MarkOffline() => Online = false;
    public static bool Online = true;

    public static string CachePath(string kind, string key)
    {
        string safe = new string(key.Select(c => char.IsLetterOrDigit(c) || c is '_' or '-' ? c : '_').ToArray());
        return Path.Combine(Root, kind + "_" + safe + ".json");
    }

    public static string Load(string file) { try { return File.Exists(file) ? File.ReadAllText(file) : ""; } catch { return ""; } }
    public static void Save(string file, string json)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, json);
    }

    public static void Enqueue(string key, string path, object payload)
    {
        string entry = JsonSerializer.Serialize(new { path, payload }, DbClient.Opts);
        string baseName = Path.GetFileNameWithoutExtension(CachePath("q", key));
        Save(Path.Combine(Root, "queue", $"{DateTime.UtcNow:yyyyMMddHHmmss}_{baseName}.json"), entry);
    }

    public static List<(string Key, string Path, string Json)> ReadAllQueue()
    {
        var list = new List<(string, string, string)>();
        string dir = Path.Combine(Root, "queue");
        if (!Directory.Exists(dir)) return list;
        foreach (var f in Directory.EnumerateFiles(dir, "*.json"))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(f));
                list.Add((f, doc.RootElement.GetProperty("path").GetString()!, doc.RootElement.GetProperty("payload").GetRawText()));
            }
            catch { /* korrupt rad: hoppa över */ }
        }
        return list;
    }

    public static void Dequeue(string file) { try { File.Delete(file); } catch { } }
}
