using Playlist.Data;

namespace Playlist.Locales;

public static class GermanLocaleSeed{
    public const string Code = "de";
    public const string Label = "Deutsch (Deutschland)";
    public const string NameLocale = "de";

    public static readonly string[] Genres = {
        "Indie-Pop", "Synthwave", "Alternative Rock", "Deep House", "Hip-Hop",
        "Schlager-Pop", "Volksmusik-Fusion", "Elektro", "Funk", "Soul",
        "Garage-Rock", "Ambient", "Drum and Bass", "Krautrock", "Liedermacher",
        "Punkrock", "Jazz-Fusion", "Techno", "Neue Deutsche Welle", "Shoegaze",
        "Akustik-Pop", "Electro-Swing", "Hyperpop", "Post-Rock", "Disco",};

    public static readonly (string Word, Gender Gender)[] TitleNouns = {
        ("Herz", Gender.Neuter), ("Skyline", Gender.Feminine), ("Echo", Gender.Neuter),
        ("Horizont", Gender.Masculine), ("Schatten", Gender.Masculine), ("Flüsse", Gender.Masculine),
        ("Rauschen", Gender.Neuter), ("Flamme", Gender.Feminine), ("Fata-Morgana", Gender.Feminine),
        ("Wölfe", Gender.Masculine), ("Gezeiten", Gender.Feminine), ("Allee", Gender.Feminine),
        ("Signal", Gender.Neuter), ("Blüte", Gender.Feminine), ("Schwerkraft", Gender.Feminine),
        ("Leuchtturm", Gender.Masculine), ("Flüstern", Gender.Neuter), ("Schwindel", Gender.Masculine),
        ("Kompass", Gender.Masculine), ("Asche", Gender.Feminine), ("Umlaufbahn", Gender.Feminine),
        ("Tagtraum", Gender.Masculine), ("Aufstand", Gender.Masculine), ("Sternbilder", Gender.Neuter),
        ("Glanz", Gender.Masculine),};

    public static readonly (string Word, Gender Gender)[] TitleAdjectives = {
        ("Zerbrochener", Gender.Masculine), ("Zerbrochene", Gender.Feminine), ("Zerbrochenes", Gender.Neuter),
        ("Elektrischer", Gender.Masculine), ("Elektrische", Gender.Feminine), ("Elektrisches", Gender.Neuter),
        ("Stummer", Gender.Masculine), ("Stumme", Gender.Feminine), ("Stummes", Gender.Neuter),
        ("Goldener", Gender.Masculine), ("Goldene", Gender.Feminine), ("Goldenes", Gender.Neuter),
        ("Verlorener", Gender.Masculine), ("Verlorene", Gender.Feminine), ("Verlorenes", Gender.Neuter),
        ("Endloser", Gender.Masculine), ("Endlose", Gender.Feminine), ("Endloses", Gender.Neuter),
        ("Wilder", Gender.Masculine), ("Wilde", Gender.Feminine), ("Wildes", Gender.Neuter),
        ("Samtiger", Gender.Masculine), ("Samtige", Gender.Feminine), ("Samtiges", Gender.Neuter),
        ("Neonfarbener", Gender.Masculine), ("Neonfarbene", Gender.Feminine), ("Neonfarbenes", Gender.Neuter),
        ("Hohler", Gender.Masculine), ("Hohle", Gender.Feminine), ("Hohles", Gender.Neuter),
        ("Purpurner", Gender.Masculine), ("Purpurne", Gender.Feminine), ("Purpurnes", Gender.Neuter),
        ("Gefrorener", Gender.Masculine), ("Gefrorene", Gender.Feminine), ("Gefrorenes", Gender.Neuter),
        ("Brennender", Gender.Masculine), ("Brennende", Gender.Feminine), ("Brennendes", Gender.Neuter),
        ("Ferner", Gender.Masculine), ("Ferne", Gender.Feminine), ("Fernes", Gender.Neuter),
        ("Ruheloser", Gender.Masculine), ("Ruhelose", Gender.Feminine), ("Ruheloses", Gender.Neuter),
        ("Glasklarer", Gender.Masculine), ("Glasklare", Gender.Feminine), ("Glasklares", Gender.Neuter),
        ("Mitternächtlicher", Gender.Masculine), ("Mitternächtliche", Gender.Feminine), ("Mitternächtliches", Gender.Neuter),
        ("Verblasster", Gender.Masculine), ("Verblasste", Gender.Feminine), ("Verblasstes", Gender.Neuter),
        ("Ewiger", Gender.Masculine), ("Ewige", Gender.Feminine), ("Ewiges", Gender.Neuter),
        ("Leiser", Gender.Masculine), ("Leise", Gender.Feminine), ("Leises", Gender.Neuter),
        ("Rücksichtsloser", Gender.Masculine), ("Rücksichtslose", Gender.Feminine), ("Rücksichtsloses", Gender.Neuter),
        ("Strahlender", Gender.Masculine), ("Strahlende", Gender.Feminine), ("Strahlendes", Gender.Neuter),
        ("Bitterer", Gender.Masculine), ("Bittere", Gender.Feminine), ("Bitteres", Gender.Neuter),
        ("Zärtlicher", Gender.Masculine), ("Zärtliche", Gender.Feminine), ("Zärtliches", Gender.Neuter),};

    public static readonly string[] AlbumWords = {
        "Sitzungen", "Chroniken", "Fragmente", "Briefe", "Tagebücher", "Hymnen",
        "Aufnahmen", "Notizen", "Visionen", "Reflexionen", "Abreisen", "Ankünfte",
        "Memoiren", "Pulsschlag", "Rauschen", "Blüte", "Echos", "Strömungen",};

    public static readonly string[] ReviewSentences = {
        "Eine überraschend straffe Produktion für so kurze Spielzeit.",
        "Der Refrain bleibt lange nach dem Song im Ohr.",
        "Klingt wie eine nächtliche Fahrt durch eine leere Stadt.",
        "Nicht bahnbrechend, aber emotional ehrlich und gut arrangiert.",
        "Der Mix könnte mehr Bass vertragen, die Melodie trägt es trotzdem.",
        "Eine ungewöhnliche Genre-Mischung, die meistens funktioniert.",
        "Eingängiger Hook, etwas repetitive Struktur.",
        "Dieser Song wächst nach ein paar Durchläufen.",
        "Mehr Produktionsglanz als Substanz, aber dennoch unterhaltsam.",
        "Eine solide Ergänzung für jede Spätsommer-Playlist.",
        "Die Gesangsleistung ist hier klar der Höhepunkt.",
        "Viel Energie in der Bridge, schwächere Eröffnungsstrophe.",
        "Klingt, als wäre es für Kopfhörer gemacht, nicht für Lautsprecher.",
        "Eine selbstbewusste, wenn auch vertraute, Genre-Interpretation.",
        "Überraschend minimalistisches Arrangement, das hier gut funktioniert.",};
}