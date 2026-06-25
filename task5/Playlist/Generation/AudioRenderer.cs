using Playlist.Lib;

namespace Playlist.Generation;

public static class AudioRenderer
{
    public const int SampleRate = 22050;

    private static readonly (double Mult, double Amp, double DecayMul)[] HarmonicRecipe =
    {
        (1.00, 1.00, 1.0),   
        (2.00, 0.55, 1.4),
        (3.00, 0.30, 1.9),
        (4.01, 0.16, 2.4),   
        (5.02, 0.09, 3.0),
        (6.03, 0.05, 3.6),
    };

    private static double NoteFrequencyOrDefault(string noteName, int octave)
    {
        return MusicTheory.NoteFrequency(noteName, octave);
    }

    private static double OscSample(string waveform, double phase)
    {
        double p = phase - Math.Floor(phase);
        switch (waveform)
        {
            case "sine":
                return Math.Sin(2 * Math.PI * p);
            case "square":
                return p < 0.5 ? 1.0 : -1.0;
            case "sawtooth":
                return 2 * (p - Math.Floor(p + 0.5));
            case "triangle":
                {
                    double t = p < 0.5 ? p * 2 : 2 - p * 2;
                    return t * 2 - 1;
                }
            default:
                return Math.Sin(2 * Math.PI * p);
        }
    }

    private static double Envelope(double tSec, double durSec, double attack, double release)
    {
        if (tSec < 0 || tSec > durSec + release) return 0;
        if (tSec < attack) return tSec / attack;
        if (tSec > durSec)
        {
            double rt = (tSec - durSec) / release;
            return Math.Max(0, 1 - rt);
        }
        return 1;
    }

    private static double PianoSample(double fundamentalFreq, double tSec)
    {
        var harmonics = HarmonicRecipe;
        double sum = 0;
        foreach (var (mult, amp, decayMul) in harmonics)
        {
            double harmonicFreq = fundamentalFreq * mult;
            double phase = harmonicFreq * tSec;
            double p = phase - Math.Floor(phase);
            double wave = Math.Sin(2 * Math.PI * p);
            double harmonicDecay = Math.Exp(-tSec * 2.2 * decayMul);
            sum += amp * wave * harmonicDecay;
        }
        if (tSec < 0.012)
        {
            double strikeEnv = (1-tSec/0.012);
            double strikeWave = Math.Sin(2 * Math.PI * fundamentalFreq * 7.3 * tSec);
            sum += 0.12 * strikeEnv * strikeWave;
        }
        return sum * 0.5;
    }

    private static void RenderPianoEvents(float[] buffer, IReadOnlyList<NoteEvent> events, InstrumentDef instrument)
    {
        foreach (var ev in events)
        {
            double freq = NoteFrequencyOrDefault(ev.Note, ev.Octave);
            int startSample = (int)Math.Floor(ev.StartSec * SampleRate);
            int totalSamples = (int)Math.Floor((ev.DurSec + instrument.Release + 0.02)*SampleRate);
            
            for (int i=0; i<totalSamples; i++)
            {
                int idx=startSample+i;
                if (idx<0 || idx>=buffer.Length) continue;
                double tSec = (double)i/SampleRate;
                double env = Envelope(tSec, ev.DurSec, instrument.Attack, instrument.Release);
                if (env <= 0) continue;
                buffer[idx] += (float)(env * ev.Velocity * PianoSample(freq, tSec));
            }
        }
    }

    private static void RenderNoteEvents(float[] buffer, IReadOnlyList<NoteEvent> events, InstrumentDef instrument)
    {
        if (instrument.Waveform == "piano")
        {
            RenderPianoEvents(buffer, events, instrument);
            return;
        }

        foreach (var ev in events)
        {
            double freq = NoteFrequencyOrDefault(ev.Note, ev.Octave);
            int startSample = (int)Math.Floor(ev.StartSec * SampleRate);
            int totalSamples = (int)Math.Floor((ev.DurSec + instrument.Release + 0.02) * SampleRate);
            for (int i=0; i < totalSamples; i++)
            {
                int idx = startSample + i;
                if (idx < 0 || idx >= buffer.Length) continue;
                double tSec = (double)i/SampleRate;
                double env = Envelope(tSec, ev.DurSec, instrument.Attack, instrument.Release);
                if (env <= 0) continue;
                double baseWave = OscSample(instrument.Waveform, freq * tSec);
                double harmonic = OscSample(instrument.Waveform, freq * 2 * tSec) * 0.15;
                buffer[idx] += (float)(env * ev.Velocity * (baseWave + harmonic));
            }
        }
    }

    private static void RenderDrumEvents(float[] buffer, IReadOnlyList<DrumEvent> events, Func<double> noiseRng)
        {
            foreach(var ev in events)
            {
                int startSample = (int)Math.Floor(ev.StartSec*SampleRate);
                if (ev.Type == "kick")
                {
                    int durSamples = (int)Math.Floor(0.18 * SampleRate);
                    for (int i=0; i<durSamples; i++)
                    {
                        int idx = startSample + i;
                        if (idx < 0 || idx >= buffer.Length) continue;
                        double t = (double) i / SampleRate;
                        double freq = 120 * Math.Exp(-t*18);
                        double env = Math.Exp(-t*14);
                        buffer[idx] += (float)(0.55 * env * Math.Sin(2 * Math.PI * freq * t));
                    }
                }
                else if (ev.Type == "snare")
                {
                    int durSamples = (int)Math.Floor(0.12 * SampleRate);
                    for (int i = 0; i<durSamples; i++)
                    {
                        int idx = startSample + i;
                        if (idx < 0 || idx >= buffer.Length) continue;
                        double t = (double)i / SampleRate;
                        double env = Math.Exp(-t * 22);
                        double noise = noiseRng() * 2 - 1;
                        buffer[idx] += (float)(0.3 * env * noise);
                    }
                }
                else if (ev.Type == "hat")
                {
                    int durSamples = (int)Math.Floor(0.05 * SampleRate);
                    for (int i = 0; i < durSamples; i++)
                    {
                        int idx = startSample + i;
                        if (idx < 0 || idx >= buffer.Length) continue;
                        double t = (double)i/SampleRate;
                        double env = Math.Exp(-t * 45);
                        double noise = noiseRng() * 2 - 1;
                        buffer[idx] += (float)(0.14 * env * noise);
                    }
                }
            }
        }

        private static float[] ApplyEcho(float[] buffer, double mix, double delaySec = 0.28)
        {
            if (mix <= 0) return buffer;
            int delaySamples = (int)Math.Floor(delaySec * SampleRate);
            float[] outBuf = (float[])buffer.Clone();
            double decay = mix;
            for (int tap = 1; tap <= 3; tap++)
            {
                int offset = delaySamples * tap;
                decay *= 0.55;
                for (int i=offset; i<outBuf.Length; i++)
                {
                    outBuf[i] += (float)(buffer[i-offset] * decay);
                } 
            }
            return outBuf;
        }

    private static float[] ApplyReverb(float[] buffer, double mix)
    {
        if (mix <= 0) return buffer;
        double[] taps = { 0.011, 0.017, 0.023, 0.031, 0.041 }; 
        float[] outBuf = (float[])buffer.Clone();
        foreach (double tapSec in taps)
        {
            int offset = (int)Math.Floor(tapSec * SampleRate);
            double tapGain = mix * 0.35;
            for (int i = offset; i < outBuf.Length; i++)
            {
                outBuf[i] += (float)(buffer[i-offset] * tapGain);
            }
        }
        return outBuf;
    }

    private static void Normalize(float[] buffer, double targetPeak = 0.9)
    {
        float peak = 0;
        for (int i=0; i<buffer.Length; i++) peak = Math.Max(peak, Math.Abs(buffer[i]));
        if (peak==0) return;
        double gain=targetPeak/peak;
        for (int i=0; i<buffer.Length; i++) buffer[i] = (float)(buffer[i]*gain);
    }

    private static short[] FloatTo16BitPcm(float[] floatBuffer)
    {
        var outBuf = new short[floatBuffer.Length];
        for(int i=0; i<floatBuffer.Length; i++)
        {
            double s = Math.Max(-1.0, Math.Min(1.0, floatBuffer[i]));
            outBuf[i] = (short)(s < 0 ? s * 0x8000 : s * 0x7fff);
        }
        return outBuf;
    }

    private static byte[] EncodeWav(short[] pcm16, int sampleRate)
    {
        const int numChannels = 1;
        const int bytesPerSample = 2;
        int blockAlign = numChannels * bytesPerSample;
        int byteRate = sampleRate * blockAlign;
        int dataSize = pcm16.Length * bytesPerSample;

        using var ms = new MemoryStream(44 + dataSize);
        using var w = new BinaryWriter(ms);

        w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        w.Write(36 + dataSize);
        w.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        w.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        w.Write(16);
        w.Write((short)1);
        w.Write((short)numChannels);
        w.Write(sampleRate);
        w.Write(byteRate);
        w.Write((short)blockAlign);
        w.Write((short)16);
        w.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        w.Write(dataSize);

        foreach(short sample in pcm16)
        {
            w.Write(sample);
        }
        w.Flush();
        return ms.ToArray();
    }

    public static byte[] RenderToWav(SongPlan plan)
    {
        int totalSamples = (int)Math.Ceiling(plan.TotalDurationSec * SampleRate);
        var buffer = new float[totalSamples];

        RenderNoteEvents(buffer, plan.PadEvents, plan.Instruments["pad"]);
        RenderNoteEvents(buffer, plan.BassEvents, plan.Instruments["bass"]);

        string melodyInstrumentName = plan.MelodyEvents.Count > 0 ? plan.MelodyEvents[0].Instrument : "lead";
        RenderNoteEvents(buffer, plan.MelodyEvents, plan.Instruments[melodyInstrumentName]);

        var noiseRng = Rng.Mulberry32(plan.MusicSeed ^ 0x5bd1e995u);
        RenderDrumEvents(buffer, plan.DrumEvents, noiseRng);

        float[] processed = ApplyReverb(buffer, plan.Fx.ReverbMix);
        processed = ApplyEcho(processed, plan.Fx.EchoMix);
        Normalize(processed, 0.85);

        short[] pcm16 = FloatTo16BitPcm(processed);
        return EncodeWav(pcm16, SampleRate);
    }
}