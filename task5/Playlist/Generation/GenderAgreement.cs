using Playlist.Data;
using Playlist.Locales;

namespace Playlist.Generation;

public static class GenderAgreement
{
    public static List<LocaleWord> AdjectivesForGender(LocaleFile locale, Gender gender)
    {
        var pool = locale.TitleAdjectives.Where(a=>a.ToGender() == gender).ToList();
        if (pool.Count == 0) pool = locale.TitleAdjectives.Where(a=>a.ToGender() == Gender.Any).ToList();
        if (pool.Count==0) pool = locale.TitleAdjectives;
        return pool;
    }
}