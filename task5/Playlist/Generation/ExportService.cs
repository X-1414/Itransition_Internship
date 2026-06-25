using System.IO.Compression;
using System.Text.RegularExpressions;
using Playlist.Data;
using Playlist.Lib;
using Playlist.Locales;

namespace Playlist.Generation;

public static class ExportService
{
    private static readonly Regex IllegalChars = new(@"[\\/:*?""<>|]", RegexOptions.Compiled);
    private static string SanitizeFileName(string name)
    {
        string cleaned = IllegalChars.Replace(name, "_");
        return cleaned.Length > 80 ? cleaned[..80] : cleaned;
    }

    public static byte[] BuildZip(LocaleFile locale, string seedInput, double likesAvg, int startIndex, int count)
    {
        return BuildZip(locale, seedInput, likesAvg, GenerateIndexes(startIndex, count));
    }

    public static byte[] BuildZip(LocaleFile locale, string seedInput, double likesAvg, IReadOnlyList<int> indexes)
    {
        if (indexes == null || indexes.Count == 0)
        {
            return Array.Empty<byte>();
        }

        uint seed = Rng.HashSeed("user-seed", seedInput);
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var generator = new RecordGenerator();
            foreach (var index in indexes)
            {
                var record = generator.Generate(index, locale, seed, likesAvg);
                var plan = SongPlanner.Build(record.MusicSeed);
                byte[] wav = AudioRenderer.RenderToWav(plan);
                byte[] mp3 = Mp3Encoder.WavToMp3(wav);

                string filename = SanitizeFileName($"{record.Index}. {record.Title} - {record.Artist} - {record.AlbumTitle}.mp3");
                var entry = archive.CreateEntry(filename, CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(mp3, 0, mp3.Length);
            }
        }
        return memoryStream.ToArray();
    }

    private static List<int> GenerateIndexes(int startIndex, int count)
    {
        var indexes = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            indexes.Add(startIndex + i);
        }
        return indexes;
    }
}