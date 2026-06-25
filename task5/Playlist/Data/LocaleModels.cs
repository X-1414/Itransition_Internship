namespace Playlist.Data;

public class Locale {
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public string NameLocale { get; set; } = "en";
    public List<Genre> Genres { get; set; } = new();
    public List<TitleNoun> TitleNouns { get; set; } = new();
    public List<TitleAdjective> TitleAdjectives { get; set; } = new();
    public List<AlbumWord> AlbumWords { get; set; } = new();
    public List<ReviewSentence> ReviewSentences { get; set; } = new();
}

public class Genre{
    public int Id { get; set; }
    public string LocaleCode { get; set; } = "";
    public Locale? Locale { get; set; }
    public string Name { get; set; } = "";
}

public enum Gender{
    Any = 0, Masculine = 1, Feminine = 2, Neuter = 3, Plural = 4,
}

public class TitleNoun{
    public int Id { get; set; }
    public string LocaleCode { get; set; } = "";
    public Locale? Locale { get; set; }
    public string Word { get; set; } = "";
    public Gender Gender { get; set; }
}

public class TitleAdjective{
    public int Id { get; set; }
    public string LocaleCode { get; set; } = "";
    public Locale? Locale { get; set; }
    public string Word { get; set; } = "";
    public Gender Gender { get; set; }
}

public class AlbumWord{
    public int Id { get; set; }
    public string LocaleCode { get; set; } = "";
    public Locale? Locale { get; set; }
    public string Word { get; set; } = "";
}

public class ReviewSentence{
    public int Id { get; set; }
    public string LocaleCode { get; set; } = "";
    public Locale? Locale { get; set; }
    public string Text { get; set; } = "";
}