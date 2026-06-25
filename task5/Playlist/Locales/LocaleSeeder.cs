using Microsoft.EntityFrameworkCore;
using Playlist.Data;

namespace Playlist.Locales;

public static class LocaleSeeder{
    public static async Task SeedAsync(PlaylistDbContext db){
        if (await db.Locales.AnyAsync()) return;
        SeedLocale(db, EnglishLocaleSeed.Code, EnglishLocaleSeed.Label, EnglishLocaleSeed.NameLocale, EnglishLocaleSeed.Genres, EnglishLocaleSeed.TitleNouns, EnglishLocaleSeed.TitleAdjectives, EnglishLocaleSeed.AlbumWords, EnglishLocaleSeed.ReviewSentences);
        SeedLocale(db, GermanLocaleSeed.Code, GermanLocaleSeed.Label, GermanLocaleSeed.NameLocale, GermanLocaleSeed.Genres, GermanLocaleSeed.TitleNouns, GermanLocaleSeed.TitleAdjectives, GermanLocaleSeed.AlbumWords, GermanLocaleSeed.ReviewSentences);
        await db.SaveChangesAsync();
    }

    private static void SeedLocale(PlaylistDbContext db, string code, string label, string nameLocale, string[] genres, (string Word, Gender Gender)[] nouns, (string Word, Gender Gender)[] adjectives, string[] albumWords, string[] reviewSentences){
        var locale = new Locale {Code=code, Label=label, NameLocale=nameLocale};
        db.Locales.Add(locale);

        foreach(var g in genres)
            db.Genres.Add(new Genre { LocaleCode = code, Name = g});

        foreach(var (word, gender) in nouns)
            db.TitleNouns.Add(new TitleNoun { LocaleCode = code,  Word=word, Gender=gender});

        foreach(var (word, gender) in adjectives)
            db.TitleAdjectives.Add(new TitleAdjective { LocaleCode = code, Word=word, Gender=gender});

        foreach(var w in albumWords)
            db.AlbumWords.Add(new AlbumWord { LocaleCode = code, Word = w});

        foreach(var r in reviewSentences)
            db.ReviewSentences.Add(new ReviewSentence { LocaleCode = code, Text = r});
    }
}