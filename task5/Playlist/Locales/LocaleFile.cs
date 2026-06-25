using System.Text.Json.Serialization;
using Playlist.Data;

namespace Playlist.Locales;

public sealed class LocaleFile
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("nameLocale")]
    public string NameLocale { get; set; } = string.Empty;

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    [JsonPropertyName("titleNouns")]
    public List<LocaleWord> TitleNouns { get; set; } = new();

    [JsonPropertyName("titleAdjectives")]
    public List<LocaleWord> TitleAdjectives { get; set; } = new();

    [JsonPropertyName("albumWords")]
    public List<string> AlbumWords { get; set; } = new();

    [JsonPropertyName("reviewSentences")]
    public List<string> ReviewSentences { get; set; } = new();
}

public sealed class LocaleWord
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = "Any";
    public Gender ToGender()
    {
        return Enum.TryParse<Gender>(Gender, true, out var result) ? result : Playlist.Data.Gender.Any;
    }
}