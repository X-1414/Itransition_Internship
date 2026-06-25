using Microsoft.EntityFrameworkCore;
using Playlist.Data;

namespace Playlist.Generation;

public class LocaleCache{
    private readonly IServiceScopeFactory _scopeFactory;
    private Dictionary<string, Locale>? _cache;
    private readonly SemaphoreSlim _lock = new(1,1);

    public LocaleCache(IServiceScopeFactory scopeFactory){
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyDictionary<string, Locale>> GetAllAsync(){
        if(_cache != null) return _cache;

        await _lock.WaitAsync();
        try{
            if (_cache != null) return _cache;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();
            var locales = await db.Locales.Include(l=>l.Genres).Include(l=>l.TitleNouns).Include(l=>l.TitleAdjectives).Include(l=>l.AlbumWords).Include(l=>l.ReviewSentences).AsSplitQuery().ToListAsync();
            _cache = locales.ToDictionary(l=>l.Code, StringComparer.OrdinalIgnoreCase);
            return _cache;
        }
        finally{
            _lock.Release();
        }
    }

    public async Task<Locale?> GetAsync(string code){
        var all = await GetAllAsync();
        return all.TryGetValue(code, out var locale) ? locale : null;
    }
    public void Invalidate() => _cache = null;
}