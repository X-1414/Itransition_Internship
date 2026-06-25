using FFMpegCore;
using FFMpegCore.Enums;

namespace Playlist.Generation;

public static class Mp3Encoder
{
    private static bool _configured;
    private static readonly object ConfigureLock = new();
    private static void EnsureConfigured()
    {
        if(_configured) return;
        lock (ConfigureLock)
        {
            if (_configured) return;
            GlobalFFOptions.Configure(new FFOptions());
        }
    }

    public static byte[] WavToMp3(byte[] wavBytes, int bitrateKbps = 128)
    {
        EnsureConfigured();
        string tempDir = Path.GetTempPath();
        string wavPath = Path.Combine(tempDir, $"playlist_{Guid.NewGuid():N}.wav");
        string mp3Path = Path.Combine(tempDir, $"playlist_{Guid.NewGuid():N}.mp3");
        try
        {
            File.WriteAllBytes(wavPath, wavBytes);
            bool success = FFMpegArguments.FromFileInput(wavPath).OutputToFile(mp3Path, overwrite: true,options => options.WithAudioCodec("libmp3lame").WithAudioBitrate(bitrateKbps)).ProcessSynchronously();

            if (!success)
            {
                throw new InvalidOperationException("ffmpeg exited without success while converting WAV to MP3. ");
            }
            return File.ReadAllBytes(mp3Path);
        }
        finally
        {
            TryDelete(wavPath);
            TryDelete(mp3Path);
        }
    }

    private static void TryDelete(string path)
    {
        try {if (File.Exists(path)) File.Delete(path);}
        catch {}
    }
}