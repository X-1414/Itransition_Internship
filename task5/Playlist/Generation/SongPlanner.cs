using Playlist.Lib;

namespace Playlist.Generation;

public record NoteEvent(string Note, int Octave, double StartSec, double DurSec, string Instrument, double Velocity);
public record DrumEvent(string Type, double StartSec);
public record InstrumentDef(string Waveform, double Attack, double Release);
public record SongFx(double ReverbMix, double EchoMix);
public record SongPlan(uint MusicSeed, string Key, string ScaleType, int Bpm, List<string[]> Chords, int TotalBars, double TotalDurationSec, List<NoteEvent> MelodyEvents, List<NoteEvent> PadEvents, List<NoteEvent> BassEvents, List<DrumEvent> DrumEvents, Dictionary<string, InstrumentDef> Instruments, SongFx Fx);

public static class SongPlanner
{
    private static readonly string[] Keys = { "C", "D", "E", "F", "G", "A", "B", "C#", "D#", "F#", "G#", "A#" };
    private static readonly string[] ScaleTypes = { "major", "minor", "dorian", "mixolydian" };
    private static readonly int[][] DegreeProgressions =
    {
        new[] { 0, 5, 3, 4 }, 
        new[] { 0, 3, 4, 0 }, 
        new[] { 5, 3, 0, 4 }, 
        new[] { 0, 4, 5, 3 }, 
        new[] { 1, 4, 0, 0 }, 
        new[] { 0, 3, 1, 4 }, 
    };
    
    public static readonly Dictionary<string, InstrumentDef> Instruments = new()
    {
        ["lead"] = new InstrumentDef("triangle", 0.01, 0.18),
        ["pad"] = new InstrumentDef("sine", 0.08, 0.4),
        ["bass"] = new InstrumentDef("sawtooth", 0.005, 0.12),
        ["pluck"] = new InstrumentDef("square", 0.002, 0.08),
        ["piano"] = new InstrumentDef("piano", 0.002, 1.1),
    };
    
    public static string[] DegreeTriad (string[] scaleNotes, int degree)
    {
        int n = scaleNotes.Length;
        string root = scaleNotes[((degree%n)+n)%n];
        string third = scaleNotes[((degree+2)%n+n)%n];
        string fifth = scaleNotes[((degree+4)%n+n)%n];
        return new[] {root, third, fifth};
    }

    private static readonly double[][] MotifOptions =
    {
        new[] { 1.0, 1.0, 0.5, 0.5, 1.0 },
        new[] { 0.5, 0.5, 1.0, 1.0, 1.0 },
        new[] { 1.0, 0.5, 0.5, 1.0, 1.0 },
        new[] { 0.75, 0.75, 0.5, 1.0, 1.0 },
    };

    public static SongPlan Build (uint musicSeed, double durSec = 22.0)
    {
        var rng = Rng.Mulberry32(musicSeed);
        string key = Rng.Pick(rng, Keys);
        string scaleType = Rng.Pick(rng, ScaleTypes);
        string[] scaleNotes = MusicTheory.GetScaleNotes(key, scaleType);

        int bpm = Rng.IntBetween(rng, 78, 168);
        double beatSec = 60.0/bpm;
        double barSec = beatSec*4;

        int[] progressionDegrees = Rng.Pick(rng, DegreeProgressions);
        var chords = progressionDegrees.Select(d => DegreeTriad(scaleNotes, d)).ToList();

        string leadInstrument = Rng.PickWeighted(rng, new(string, double)[]
        {
            ("piano", 3), ("lead", 1), ("pluck", 1),
        });
        const string padInstrument = "pad";
        const string bassInstrument = "bass";

        int totalBars = Math.Max(8, (int)Math.Round(durSec/barSec));
        const int barsPerChord = 1;

        var melodyEvents = new List<NoteEvent>();
        var padEvents = new List<NoteEvent>();
        var bassEvents = new List<NoteEvent>();
        var drumEvents = new List<DrumEvent>();

        for (int bar=0; bar<totalBars; bar++)
        {
            int chordIdx=(bar/barsPerChord)%chords.Count;
            string[] chordNotes = chords[chordIdx];
            double barStart=bar*barSec;

            foreach (string n in chordNotes)
            {
                padEvents.Add(new NoteEvent(n, 3, barStart, barSec*0.95, padInstrument, 0.18));
            }

            bassEvents.Add(new NoteEvent(chordNotes[0], 2, barStart, beatSec*0.9, bassInstrument, 0.5));
            bassEvents.Add(new NoteEvent(chordNotes[0], 2, barStart+beatSec*2, beatSec*0.9, bassInstrument, 0.4));

            double[] motif = Rng.Pick(rng, MotifOptions);
            double t = 0;

            int prevIdx = chordIdx;
            foreach (double dur in motif)
            {
                int step = Rng.PickWeighted(rng, new (int, double)[]
                {
                    (0, 3), (1, 4), (-1, 4), (2, 2), (-2, 2),
                });

                int idx = ((prevIdx + step) % scaleNotes.Length + scaleNotes.Length) % scaleNotes.Length;
                prevIdx = idx;
                string note = scaleNotes[idx];
                const double restProbability = 0.12;
                if (!Rng.Chance(rng, restProbability))
                {
                    melodyEvents.Add(new NoteEvent(note, 4, barStart + t, beatSec * dur * 0.85, leadInstrument, 0.32));
                }
                t += beatSec * dur;
            }

            drumEvents.Add(new DrumEvent("kick", barStart));
            drumEvents.Add(new DrumEvent("kick", barStart + beatSec * 2));
            drumEvents.Add(new DrumEvent("hat", barStart + beatSec * 0.5));
            drumEvents.Add(new DrumEvent("hat", barStart + beatSec * 1.5));
            drumEvents.Add(new DrumEvent("hat", barStart + beatSec * 2.5));
            drumEvents.Add(new DrumEvent("hat", barStart + beatSec * 3.5));

            if (Rng.Chance(rng, 0.7))
            {
                drumEvents.Add(new DrumEvent("snare", barStart + beatSec * 1));
                drumEvents.Add(new DrumEvent("snare", barStart + beatSec * 3));
            } 
        }

        double totalDurationSec = totalBars * barSec + 1.2;
        double reverbMix = 0.15 + (musicSeed % 100)/100.0 * 0.25;
        var echoRng = Rng.Mulberry32(musicSeed ^ 0x9e3779b9u);
        double echoMix = Rng.Chance(echoRng, 0.6) ? 0.18 : 0.0;

        return new SongPlan (musicSeed, key, scaleType, bpm, chords, totalBars, totalDurationSec, melodyEvents, padEvents, bassEvents, drumEvents, Instruments, new SongFx(reverbMix, echoMix));
    }
}