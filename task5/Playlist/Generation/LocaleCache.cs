using System.Text.Json;
using Playlist.Locales;

namespace Playlist.Generation;

public class LocaleCache{
    private readonly Dictionary<string, LocaleFile> _cache;
    public LocaleCache(){
        _cache = LoadLocales();
    }

    public static Dictionary<string, LocaleFile> LoadLocales()
    {
        var result = new Dictionary<string, LocaleFile>(StringComparer.OrdinalIgnoreCase);
        var localeFolder = Path.Combine(AppContext.BaseDirectory, "Locales");

        if (!Directory.Exists(localeFolder))
        {
            throw new DirectoryNotFoundException($"Locale folder not found: {localeFolder}");
        }

        foreach (var file in Directory.GetFiles(localeFolder, "*.json"))
        {
            var json = File.ReadAllText(file);
            var locale = JsonSerializer.Deserialize<LocaleFile>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true});

            if (locale == null) continue;

            result [locale.Code] = locale;
        }

        if (result.Count == 0) throw new InvalidOperationException("No locale JSON files were loaded.");
        return result;
    }

    public Task<IReadOnlyDictionary<string, LocaleFile>> GetAllAsync(){
        return Task.FromResult<IReadOnlyDictionary<string, LocaleFile>>(_cache);
    }
    
    public  Task<LocaleFile?> GetAsync(string code){
        _cache.TryGetValue(code, out var locale);

        return Task.FromResult(locale);
    }
    public void Invalidate(){}
}