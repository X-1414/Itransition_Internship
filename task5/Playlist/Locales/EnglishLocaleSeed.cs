using Playlist.Data;

namespace Playlist.Locales;

public static class EnglishLocaleSeed{
    public const string Code = "en";
    public const string Label = "English (USA)";
    public const string NameLocale = "en";

    public static readonly string[] Genres = {
        "Indie Pop", "Synthwave", "Alternative Rock", "Deep House", "Trap",
        "Lo-fi Hip Hop", "Folk", "Dream Pop", "Funk", "Soul",
        "Garage Rock", "Ambient", "Drum and Bass", "Country", "R&B",
        "Punk Rock", "Jazz Fusion", "Reggaeton", "Glitch Pop", "Shoegaze",
        "Acoustic", "Electro Swing", "Hyperpop", "Post-Rock", "Disco",};

    public static readonly (string Word, Gender Gender)[] TitleNouns = {
        ("Heart", Gender.Any), ("Skyline", Gender.Any), ("Echo", Gender.Any),
        ("Horizon", Gender.Any), ("Shadow", Gender.Any), ("Rivers", Gender.Any),
        ("Static", Gender.Any), ("Flame", Gender.Any), ("Mirage", Gender.Any),
        ("Wolves", Gender.Any), ("Tide", Gender.Any), ("Avenue", Gender.Any),
        ("Signal", Gender.Any), ("Bloom", Gender.Any), ("Gravity", Gender.Any),
        ("Lighthouse", Gender.Any), ("Whisper", Gender.Any), ("Vertigo", Gender.Any),
        ("Compass", Gender.Any), ("Ashes", Gender.Any), ("Orbit", Gender.Any),
        ("Daydream", Gender.Any), ("Rebellion", Gender.Any), ("Constellations", Gender.Any),
        ("Glow", Gender.Any),};

    public static readonly (string Word, Gender Gender)[] TitleAdjectives = {
        ("Broken", Gender.Any), ("Electric", Gender.Any), ("Silent", Gender.Any),
        ("Golden", Gender.Any), ("Lost", Gender.Any), ("Endless", Gender.Any),
        ("Wild", Gender.Any), ("Velvet", Gender.Any), ("Neon", Gender.Any),
        ("Hollow", Gender.Any), ("Crimson", Gender.Any), ("Frozen", Gender.Any),
        ("Burning", Gender.Any), ("Distant", Gender.Any), ("Restless", Gender.Any),
        ("Glass", Gender.Any), ("Midnight", Gender.Any), ("Faded", Gender.Any),
        ("Eternal", Gender.Any), ("Quiet", Gender.Any), ("Reckless", Gender.Any),
        ("Radiant", Gender.Any), ("Bitter", Gender.Any), ("Tender", Gender.Any),
        ("Savage", Gender.Any),};

    public static readonly string[] AlbumWords = {
        "Sessions", "Chronicles", "Fragments", "Letters", "Diaries", "Anthems",
        "Tapes", "Notes", "Visions", "Reflections", "Departures", "Arrivals",
        "Memoirs", "Pulse", "Static", "Bloom", "Echoes", "Currents",};

    public static readonly string[] ReviewSentences = {
        "A surprisingly tight production for such a short runtime.",
        "The chorus sticks with you long after the track ends.",
        "Feels like a late-night drive through an empty city.",
        "Not groundbreaking, but emotionally honest and well arranged.",
        "The mix could use more low-end, but the melody carries it.",
        "An unexpected genre blend that mostly pays off.",
        "Catchy hook, slightly repetitive structure.",
        "This one grows on you after a few listens.",
        "Production polish over substance, but still enjoyable.",
        "A solid addition to any late-summer playlist.",
        "The vocal performance is the clear highlight here.",
        "Great energy in the bridge, weaker opening verse.",
        "Sounds like it was made for headphones, not speakers.",
        "A confident, if familiar, take on the genre.",
        "Surprisingly minimal arrangement that works in its favor.",};
}
