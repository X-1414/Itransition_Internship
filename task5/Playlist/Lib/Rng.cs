namespace Playlist.Lib;

public static class Rng {
    public static uint HashSeed(params object[] parts){
        string s = string.Join(":", parts.Select(FormatInvariant));
        uint h = 2166136261u; // FNV offset basis
        unchecked
        {
            for (int i=0; i<s.Length; i++){
                h^=s[i];
                h*=16777619u; 
            }
        }
        return h;
    }
    private static string FormatInvariant(object value) => value switch{
        double d => FormatDouble(d),
        float f => FormatDouble(f),
        IFormattable f => f.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };

    private static string FormatDouble(double d){
        string s = d.ToString("F9", System.Globalization.CultureInfo.InvariantCulture);
        s=s.TrimEnd('0');
        s=s.TrimEnd('.');
        return s.Length == 0 || s == "-" ? "0" : s;
    }

    public static Func<double> Mulberry32(uint seed){
        uint a = seed;
        return () => { unchecked{
            a += 0x6d2b79f5u;
            uint t = (a ^ (a >> 15)) * (1u | a);
            t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
            return (t ^ (t >> 14)) / 4294967296.0;
        }};
    }

    public static Func<double> StreamRng(object userSeed, int recordIndex, string streamTag, params object[] extra){
        var parts = new object[2 + extra.Length+1];
        parts[0] = userSeed;
        parts[1] = recordIndex;
        parts[2] = streamTag;
        Array.Copy(extra, 0, parts, 3, extra.Length);
        return Mulberry32(HashSeed(parts));
    }

    public static T Pick<T>(Func <double> rng, IReadOnlyList<T> arr){
        int idx = (int)Math.Floor(rng()*arr.Count);
        if (idx >= arr.Count) idx = arr.Count - 1;
        return arr[idx];
    }

    public static T PickWeighted<T>(Func<double> rng, IReadOnlyList<(T Value, double Weight)> items){
        double total = items.Sum(i=>i.Weight);
        double r = rng() * total;
        foreach (var item in items){
            if (r < item.Weight) return item.Value;
            r -= item.Weight;
        }
        return items[^1].Value;
    }

    public static int IntBetween(Func<double> rng, int min, int max){
        return (int)Math.Floor(rng()*(max - min + 1)) + min;
    }

    public static double FloatBetween(Func<double> rng, double min, double max){
        return rng() * (max-min) + min;
    }

    public static bool Chance(Func<double> rng, double probability){
        return rng() < probability;
    }

    public static int LikesFromAverage(Func<double> rng, double avg){
        int baseLikes = (int)Math.Floor(avg);
        double frac = avg - baseLikes;
        return baseLikes + (rng() < frac ? 1 : 0);
    }
}

