using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Playlist.Data;

namespace Playlist.Locales;

public static class LocaleSeeder{
    public static async Task SeedAsync(PlaylistDbContext db){
        if (await db.Locales.AnyAsync()) return;
        string localeFolder = Path.Combine(AppContext.BaseDirectory, "Locales");

        if(!Directory.Exists(localeFolder))
            throw new DirectoryNotFoundException($"Locale folder not found: {localeFolder}");
        
        var jsonFiles = Directory.GetFiles(localeFolder, "*.json");

        foreach (var file in jsonFiles)
        {
            string json = await File.ReadAllTextAsync(file);
            var locale = JsonSerializer.Deserialize<LocaleFile>( json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (locale==null) continue;
            SeedLocale(db, locale);
        }
        await db.SaveChangesAsync();
    }

    private static void SeedLocale(PlaylistDbContext db, LocaleFile localeFile){
        db.Locales.Add(new Locale
        {
            Code = localeFile.Code,
            Label = localeFile.Label,
            NameLocale = localeFile.NameLocale
        });

        foreach(var g in localeFile.Genres)
            db.Genres.Add(new Genre { LocaleCode = localeFile.Code, Name = g});

        foreach(var noun in localeFile.TitleNouns)
            db.TitleNouns.Add(new TitleNoun { LocaleCode = localeFile.Code,  Word=noun.Word, Gender=noun.ToGender()});

        foreach(var adjective in localeFile.TitleAdjectives)
            db.TitleAdjectives.Add(new TitleAdjective { LocaleCode = localeFile.Code, Word=adjective.Word, Gender=adjective.ToGender()});

        foreach(var w in localeFile.AlbumWords)
            db.AlbumWords.Add(new AlbumWord { LocaleCode = localeFile.Code, Word = w});

        foreach(var r in localeFile.ReviewSentences)
            db.ReviewSentences.Add(new ReviewSentence { LocaleCode = localeFile.Code, Text = r});
    }
}