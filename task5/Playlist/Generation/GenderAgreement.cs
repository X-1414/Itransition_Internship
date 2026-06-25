using Playlist.Data;

namespace Playlist.Generation;

public static class GenderAgreement
{
    public static List<TitleAdjective> AdjectivesForGender(Locale locale, Gender gender)
    {
        var pool = locale.TitleAdjectives.Where(a=>a.Gender == gender).ToList();
        if (pool.Count == 0) pool = locale.TitleAdjectives.Where(a=>a.Gender == Gender.Any).ToList();
        if (pool.Count==0) pool = locale.TitleAdjectives;
        return pool;
    }
}