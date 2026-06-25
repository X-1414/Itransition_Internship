namespace Playlist.Generation;

public static class MusicTheory
{
    private static readonly string[] ChromaticSharps =
        { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    private static readonly Dictionary<string, int> PitchClassIndex = ChromaticSharps.Select((name, i) => (name, i)).ToDictionary(x=>x.name, x=>x.i);
       
    public static readonly Dictionary<string, int[]> ScaleIntervals = new()
    {
        ["major"] = new[] { 0, 2, 4, 5, 7, 9, 11 },
        ["minor"] = new[] { 0, 2, 3, 5, 7, 8, 10 },      
        ["dorian"] = new[] { 0, 2, 3, 5, 7, 9, 10 },
        ["mixolydian"] = new[] { 0, 2, 4, 5, 7, 9, 10 },
    };

    public static string[] GetScaleNotes(string key, string scaleType)
    { 
        if (!PitchClassIndex.TryGetValue(key, out int rootIndex))
            throw new ArgumentException($"Unknown key: {key}");
        if (!ScaleIntervals.TryGetValue(scaleType, out int[]? intervals))
            throw new ArgumentException($"Unknown scale type: {scaleType}");

        return intervals.Select(interval => ChromaticSharps[(rootIndex + interval) % 12]).ToArray();
    }

    public static double NoteFrequency(string noteName, int octave)
    {
        if (!PitchClassIndex.TryGetValue(noteName, out int pitchClass))
            return 440.0;
        int midiNote = (octave + 1) * 12 + pitchClass;
        double semitonesFromA4 = midiNote - 69;
        return 440.0 * Math.Pow(2.0, semitonesFromA4 / 12.0);
    }
}