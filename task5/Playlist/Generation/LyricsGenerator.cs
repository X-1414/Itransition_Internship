using System.Globalization;
using Playlist.Data;
using Playlist.Lib;

namespace Playlist.Generation;

public record LyricLine(string Text, double StartSec, double EndSec);

public static class LyricsGenerator
{
    private static readonly Func<string, string, string>[] Patterns =
    {
        (adj, noun) => $"{adj} {noun}, don't let it fade",
        (adj, noun) => $"We are the {noun} in the {Lower(adj)} night",
        (adj, noun) => $"{noun}, {noun}, calling out my name",
        (adj, noun) => $"Every {Lower(noun)} feels {Lower(adj)} tonight",
        (adj, noun) => $"Hold the {Lower(noun)}, hold the light",
        (adj, noun) => $"{adj} skies above the {Lower(noun)}",
        (adj, noun) => $"Running through the {Lower(adj)} {Lower(noun)}",
        (adj, noun) => $"This is our {Lower(adj)} {Lower(noun)}",
    };

    private static string Lower(string s) => s.ToLowerInvariant();

    public static List<LyricLine> Build (Locale locale, uint musicSeed, SongPlan plan)
    {
        var rng = Rng.Mulberry32(musicSeed ^ 0x2545f491u);

        double beatSec = 60.0 / plan.Bpm;
        double barSec = beatSec * 4;
        const int linesPerSection = 2;

        var lines = new List<LyricLine>();
        for (int bar=0; bar<plan.TotalBars; bar+=linesPerSection)
        {
            var nounEntry = Rng.Pick(rng, locale.TitleNouns);
            string noun = nounEntry.Word;
            var adjPool = GenderAgreement.AdjectivesForGender(locale, nounEntry.Gender);
            string adj = Rng.Pick(rng, adjPool).Word;
            var pattern = Rng.Pick(rng, Patterns);
            string text = pattern(adj, noun);
            double startSec = bar * barSec;
            double endSec = Math.Min(plan.TotalDurationSec, (bar + linesPerSection) * barSec);
            lines.Add(new LyricLine(text, startSec, endSec));
        }
        return lines;
    }
}