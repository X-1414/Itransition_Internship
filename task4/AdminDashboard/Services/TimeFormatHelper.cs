namespace AdminDashboard.Services;

public static class TimeFormatHelper
{
    public static string ToRelativeTime(DateTime utcTime)
    {
        var span = DateTime.UtcNow - utcTime;

        if (span.TotalSeconds < 60)
            return "less than a minute ago";
        
        if (span.TotalMinutes < 60)
        {
            var m = (int)span.TotalMinutes;
            return m==1 ? "1 minute ago" : $"{m} minutes ago";
        }
        if (span.TotalHours < 24)
        {
            var h = (int)span.TotalHours;
            return h == 1 ? "1 hour ago" : $"{h} hours ago";
        }
        if (span.TotalDays < 7)
        {
            var d = (int)span.TotalDays;
            return d == 1 ? "1 day ago" : $"{d} days ago";
        }
        if (span.TotalDays < 30)
        {
            var w = (int)(span.TotalDays/7);
            return w == 1 ? "1 week ago" : $"{w} weeks ago";
        }
        if (span.TotalDays < 365)
        {
            var mo = (int)(span.TotalDays/30);
            return mo == 1 ? "1 month ago" : $"{mo} months ago";
        }
        var y = (int)(span.TotalDays/365);
        return y == 1 ? "1 year ago" : $"{y} years ago";

    }
}