using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;
using Playlist.Generation;
using Playlist.Lib;
using Playlist.Locales;

const double MaxLikesAvg = 10.0;
const double MinLikesAvg = 0.0;

var builder = WebApplication.CreateBuilder(args);

string connectionString = Environment.GetEnvironmentVariable("PLAYLIST_DB_CONNECTION") ?? builder.Configuration.GetConnectionString("Default") ?? "Host=localhost;Port=5432;Database=playlist;Username=postgres;Password=postgres";

builder.Services.AddDbContext<PlaylistDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton<LocaleCache>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

var app = builder.Build();
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlaylistDbContext>();
    await db.Database.EnsureCreatedAsync();
    await LocaleSeeder.SeedAsync(db); 
}

app.UseDefaultFiles();
app.UseStaticFiles();

async Task<(string? Error, string LocaleCode, Locale? Locale, string SeedInput, double LikesAvg)>
    ParseCommonParamsAsync(HttpRequest request, LocaleCache localeCache)
{
    string localeCode = request.Query["locale"].FirstOrDefault() ?? "en";
    string? seedRaw = request.Query["seed"].FirstOrDefault();
    string seedInput = string.IsNullOrEmpty(seedRaw) ? "0" : seedRaw;

    double likesAvg = 0;
    string? likesRaw = request.Query["likes"].FirstOrDefault();
    if(likesRaw != null && double.TryParse(likesRaw, System.Globalization.CultureInfo.InvariantCulture, out double parsed))
    {
        likesAvg = parsed;
    }
    likesAvg = Math.Min(MaxLikesAvg, Math.Max(MinLikesAvg, likesAvg));

    var locale = await localeCache.GetAsync(localeCode);
    if (locale == null)
    {
        return ($"Unknown locale \"{localeCode}\"", localeCode, null, seedInput, likesAvg);
    }
    return (null, localeCode, locale, seedInput, likesAvg);
}   

uint NumericSeed(string seedInput) => Rng.HashSeed("user-seed", seedInput);

SongRecord GetFullRecord(int index, Locale locale, string seedInput, double likesAvg)
{
    uint seed = NumericSeed(seedInput);
    var generator = new RecordGenerator();
    return generator.Generate(index, locale, seed, likesAvg);
}

string BuildCoverUrl(int index, string localeCode, string seedInput) => $"/api/songs/{index}/cover.png?locale={localeCode}&seed={Uri.EscapeDataString(seedInput)}";
string BuildAudioUrl(int index, string localeCode, string seedInput) => $"/api/songs/{index}/audio.wav?locale={localeCode}&seed={Uri.EscapeDataString(seedInput)}";
string BuildLyricsUrl(int index, string localeCode, string seedInput) => $"/api/songs/{index}/lyrics?locale={localeCode}&seed={Uri.EscapeDataString(seedInput)}";


// GET api/locales
app.MapGet("/api/locales", async (LocaleCache localeCache)=>
{
    var all = await localeCache.GetAllAsync();
    var locales = all.Values.Select(l=>new{code=l.Code, label=l.Label}).OrderBy(l=>l.code).ToList();
    return Results.Json(new{locales});
});

// GET api/songs
app.MapGet("/api/songs", async (HttpRequest request, LocaleCache localeCache) =>
{
    var (error, localeCode, locale, seedInput, likesAvg) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400);

    int page = Math.Max(1, ParseIntOrDefault(request.Query["page"], 1));
    int pageSize = Math.Min(100, Math.Max(1, ParseIntOrDefault(request.Query["pageSize"], 20)));

    int startIndex = (page-1) * pageSize + 1;
    var records = new List<object>();
    for (int i=0; i<pageSize; i++){
        int index = startIndex + i;
        var record = GetFullRecord(index, locale!, seedInput, likesAvg);
        records.Add(new{index=record.Index, title=record.Title, artist=record.Artist, albumTitle=record.AlbumTitle, isSingle=record.IsSingle, genre=record.Genre, likes=record.Likes, reviewText=record.ReviewText, coverUrl=BuildCoverUrl(record.Index, localeCode, seedInput), audioUrl=BuildAudioUrl(record.Index, localeCode, seedInput), lyricsUrl=BuildLyricsUrl(record.Index, localeCode, seedInput),});
    }
    return Results.Json(new {page, pageSize, records });
});

// GET api/songs/{index}
app.MapGet("/api/songs/{index:int}", async (int index, HttpRequest request, LocaleCache localeCache) =>
{
    var (error, localeCode, locale, seedInput, likesAvg) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400);
    if (index<1) return Results.Json(new {error="index must be a positive integer"}, statusCode:400);

    var record = GetFullRecord(index, locale!, seedInput, likesAvg);
    return Results.Json(new {index=record.Index, title=record.Title, artist=record.Artist, albumTitle=record.AlbumTitle, isSingle=record.IsSingle, genre=record.Genre, likes=record.Likes, reviewText=record.ReviewText, coverUrl=BuildCoverUrl(index, localeCode, seedInput), audioUrl=BuildAudioUrl(index, localeCode, seedInput), lyricsUrl=BuildLyricsUrl(index, localeCode, seedInput),});
});

// GET /api/songs/{index}/cover.png
app.MapGet("/api/songs/{index:int}/cover.png", async (int index, HttpRequest request, LocaleCache localeCache)=>
{
    var (error, _, locale, seedInput, _) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400);
    if (index < 1) return Results.Json(new {error = "index must be a positive integer"}, statusCode: 400);

    var record = GetFullRecord(index, locale!, seedInput, 0);
    byte[] png = CoverGenerator.RenderPng(record.Title, record.Artist, record.CoverSeed);
    request.HttpContext.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    return Results.Bytes(png, "image/png");
});

// GET /api/songs/{index}/audio.wav
app.MapGet("/api/songs/{index:int}/audio.wav", async (int index, HttpRequest request, LocaleCache localeCache)=>
{
    var (error, _, locale, seedInput, _) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400);
    if (index < 1) return Results.Json(new {error = "index must be a positive integer"}, statusCode: 400);

    var record = GetFullRecord(index, locale!, seedInput, 0);
    var plan = SongPlanner.Build(record.MusicSeed);
    byte[] wav = AudioRenderer.RenderToWav(plan);
    request.HttpContext.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
    return Results.Bytes(wav, "audio/wav");
});

// GET /api/songs/{index}/lyrics
app.MapGet("/api/songs/{index:int}/lyrics", async (int index, HttpRequest request, LocaleCache localeCache)=>
{
    var (error, _, locale, seedInput, _) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400);
    if (index < 1) return Results.Json(new {error = "index must be a positive integer"}, statusCode: 400);

    var record = GetFullRecord(index, locale!, seedInput, 0);
    var plan = SongPlanner.Build(record.MusicSeed);
    var lyrics = LyricsGenerator.Build(locale!, record.MusicSeed, plan);

    return Results.Json(new{
        lines = lyrics.Select(l => new { text = l.Text, startSec = l.StartSec, endSec = l.EndSec }),
        durationSec = plan.TotalDurationSec,
    });
});


// GET /api/export.zip
app.MapGet("/api/export.zip", async (HttpRequest request, LocaleCache localeCache)=>
{
    var (error, _, locale, seedInput, likesAvg) = await ParseCommonParamsAsync(request, localeCache); 
    if (error!=null) return Results.Json(new {error}, statusCode: 400); 

    List<int>? indexes = null;
    string? indexesRaw = request.Query["indexes"].FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(indexesRaw))
    {
        indexes = indexesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x.Trim(), out var n) ? n : -1)
            .Where(n => n > 0)
            .Distinct()
            .ToList();
    }

    byte[] zipBytes;
    if (indexes != null && indexes.Count > 0)
    {
        zipBytes = ExportService.BuildZip(locale!, seedInput, likesAvg, indexes);
    }
    else
    {
        int count = Math.Min(200, Math.Max(1, ParseIntOrDefault(request.Query["count"], 20)));
        int startIndex = Math.Max(1, ParseIntOrDefault(request.Query["startIndex"], 1));
        zipBytes = ExportService.BuildZip(locale!, seedInput, likesAvg, startIndex, count);
    }

    return Results.Bytes(zipBytes, "application/zip", fileDownloadName: "songs-export.zip");
});

app.Run();

static int ParseIntOrDefault(Microsoft.Extensions.Primitives.StringValues raw, int fallback){
    string? s = raw.FirstOrDefault();
    return int.TryParse(s, out int value) ? value : fallback;
}

