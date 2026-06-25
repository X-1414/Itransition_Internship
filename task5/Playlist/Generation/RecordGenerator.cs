using System.Text.RegularExpressions;
using Bogus;
using Playlist.Data;
using Playlist.Lib;

namespace Playlist.Generation;

public class SongRecord {
    public int Index { get; set; }
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string AlbumTitle { get; set; } = "";
    public bool IsSingle { get; set; }
    public string Genre { get; set; } = "";
    public int Likes { get; set; }
    public string ReviewText { get; set; } = "";
    public uint CoverSeed { get; set; }
    public uint MusicSeed { get; set; }
}

public class RecordGenerator {
    private static readonly HashSet<string> LegalFormTokens = new(StringComparer.OrdinalIgnoreCase){
        "LLC", "INC", "INC.", "GMBH", "AG", "LTD", "LTD.", "SARL", "SAS", "SA",
        "ТОВ", "ТДВ", "ПП", "ФОП", "EI", "SEM", "GIE",
    };

    private static string StripLegalForm(string name){
        var words = name.Split(' ').Where(w => !LegalFormTokens.Contains(w.Trim(','))).ToArray();
        var joined = string.Join(' ', words).Trim();
        return joined.Length > 0 ? joined : name;
    }

    private static string FirstMeaningfulWord(string name){
        var words = name.Split(' ').Where(w => !LegalFormTokens.Contains(w.Trim(','))).ToArray();
        var first = words.Length > 0 ? words[0] : name;
        return Regex.Replace(first, "[,.;:]+$", "");
    }

    public SongRecord Generate (int index, Locale locale, object userSeed, double likesAvg){
        var contentRng = Rng.StreamRng(userSeed, index, "content");
        var likesRng = Rng.StreamRng(userSeed, index, "likes", likesAvg);
        int fakerSeed = (int)Math.Floor(contentRng() * Math.Pow(2, 31));
        var faker = new Faker(locale.NameLocale);
        faker.Random = new Bogus.Randomizer(fakerSeed);

        bool isBand = Rng.Chance(contentRng, 0.5);
        string rawArtist = isBand ? faker.Company.CompanyName() : faker.Name.FullName();
        string artist = isBand ? StripLegalForm(rawArtist) : rawArtist;
        var nounEntry = Rng.Pick(contentRng, locale.TitleNouns);
        string noun = nounEntry.Word;
        var adjPool = GenderAgreement.AdjectivesForGender(locale, nounEntry.Gender);
        string adjective = Rng.Pick(contentRng, adjPool).Word;
    
        int titlePattern = Rng.IntBetween(contentRng, 0, 2);
        string title = titlePattern switch {
            0 => $"{adjective} {noun}",
            1 => noun,
            _ => $"{noun} ({adjective})",
        };

        bool isSingle = Rng.Chance(contentRng, 0.35);
        string albumWord = Rng.Pick(contentRng, locale.AlbumWords).Word;
        string albumTitle = isSingle ? "Single" : $"{FirstMeaningfulWord(artist)} {albumWord}";
        string genre = Rng.Pick(contentRng, locale.Genres).Name;
        string reviewText = Rng.Pick(contentRng, locale.ReviewSentences).Text;
        int likes = Rng.LikesFromAverage(likesRng, likesAvg);

        uint coverSeed = (uint)Math.Floor(contentRng() * Math.Pow(2, 31));
        uint musicSeed = (uint)Math.Floor(contentRng() * Math.Pow(2, 31));

        return new SongRecord{ Index=index, Title=title, Artist=artist, AlbumTitle=albumTitle, IsSingle=isSingle, Genre=genre, Likes=likes, ReviewText=reviewText, CoverSeed=coverSeed, MusicSeed=musicSeed,};
    }
}
    
    