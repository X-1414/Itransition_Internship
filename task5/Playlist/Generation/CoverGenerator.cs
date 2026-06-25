using SkiaSharp;
using Playlist.Lib;

namespace Playlist.Generation;

public static class CoverGenerator{
    private const int Size = 500;

    private static readonly SKTypeface RegularTypeface = LoadBundledFont("DejaVuSans.ttf");
    private static readonly SKTypeface BoldTypeface = LoadBundledFont("DejaVuSans-Bold.ttf");

    private static SKTypeface LoadBundledFont(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fonts", fileName);
        var typeface = SKTypeface.FromFile(path);
        if(typeface == null)
        {
            throw new FileNotFoundException($"Could not load bundled font '{fileName}' from '{path}'. ");
        }
        return typeface;
    }

    private record Palette(SKColor Bg1, SKColor Bg2, SKColor Accent, SKColor TextColor, bool Dark);

    private static Palette MakePalette(Func<double> rng){
        float hue = (float)(rng() * 360);
        float hue2 = (float)((hue + 40 + rng() * 80) % 360);
        bool dark = Rng.Chance(rng, 0.5);
 
        float bg1Sat = (float)Rng.FloatBetween(rng, 55, 80);
        float bg1Light = dark ? (float)Rng.FloatBetween(rng, 14, 22) : (float)Rng.FloatBetween(rng, 75, 88);
        float bg2Sat = (float)Rng.FloatBetween(rng, 55, 80);
        float bg2Light = dark ? (float)Rng.FloatBetween(rng, 22, 32) : (float)Rng.FloatBetween(rng, 60, 75);
 
        var bg1 = SKColor.FromHsl(hue, bg1Sat, bg1Light);
        var bg2 = SKColor.FromHsl(hue2, bg2Sat, bg2Light);
        var accent = SKColor.FromHsl((hue + 180) % 360, 70, dark ? 70 : 30);
        var textColor = dark ? new SKColor(0xf5, 0xf5, 0xf5) : new SKColor(0x1a, 0x1a, 0x1a);
 
        return new Palette(bg1, bg2, accent, textColor, dark);
    }

    private static void DrawConcentric(SKCanvas canvas, Func<double> rng, Palette p)
    {
        float cx = Rng.IntBetween(rng, 150, 350);
        float cy = Rng.IntBetween(rng, 150, 350);
        int rings = Rng.IntBetween(rng, 5, 9);
        for (int i=rings; i>=1; i--){
            float r = (i/(float)rings)*340f;
            byte opacity = (byte)((0.08+(i/(double)rings)*0.12)*255);
            using var paint = new SKPaint{
                Color = p.Accent.WithAlpha(opacity),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 6 + i,
                IsAntialias = true,
            };
            canvas.DrawCircle(cx, cy, r, paint);
        }
    }

    private static void DrawGrid(SKCanvas canvas, Func<double> rng, Palette p){
        int cells = Rng.IntBetween(rng, 4, 7);
        float size = Size/(float)cells;
        for(int row = 0; row < cells; row++){
            for (int col = 0; col < cells; col++){
                if (Rng.Chance(rng, 0.45)){
                    byte opacity = (byte)(Rng.FloatBetween(rng, 0.08, 022) * 255);
                    using var paint = new SKPaint {Color = p.Accent.WithAlpha(opacity),IsAntialias=true};
                    canvas.DrawRect(col*size, row*size, size, size, paint);
                }
            }
        }
    }

    private static void DrawDiagonalStripes(SKCanvas canvas, Func<double> rng, Palette p){
        float stripeWidth = Rng.IntBetween(rng, 24, 60);
        var angles = new[] { 30f, 45f, 60f, -30f, -45f, -60f };
        float angle = Rng.Pick(rng, angles);
        canvas.Save();
        canvas.Translate(250, 250);
        canvas.RotateDegrees(angle);
        canvas.Translate(-250, -250);

        for(float x=-500; x<1000; x+=stripeWidth*2){
            byte opacity = (byte)(Rng.FloatBetween(rng, 0.1, 0.2)*255);
            using var paint = new SKPaint{Color=p.Accent.WithAlpha(opacity), IsAntialias=true};
            canvas.DrawRect(x, -300, stripeWidth, 1100, paint);
        }
        canvas.Restore();
    }

    private static void DrawRadialBurst(SKCanvas canvas, Func<double> rng, Palette p)
    {
        int rays = Rng.IntBetween(rng, 8, 16);
        canvas.Save();
        canvas.Translate(250, 250);
        for (int i=0; i<rays; i++){
            float angle = (360f/rays)*i;
            byte opacity = (byte)(Rng.FloatBetween(rng, 0.08, 0.18)*255);
            using var paint = new SKPaint{Color=p.Accent.WithAlpha(opacity), IsAntialias=true};
            canvas.Save();
            canvas.RotateDegrees(angle);
            canvas.DrawRect(-6, -340, 12, 340, paint);
            canvas.Restore();
        }
        canvas.Restore();
    }
    
    private static void DrawPolkaDots(SKCanvas canvas, Func<double> rng, Palette p)
    {
        int cols = Rng.IntBetween(rng, 5, 8);
        float spacing = Size / (float)cols;
        for (int row=0; row<=cols; row++){
            for(int col=0; col<=cols; col++){
                float offset = row%2 == 0 ? 0 : spacing/2;
                float cx = col*spacing + offset;
                float cy = row*spacing;
                float r = spacing * (float)Rng.FloatBetween(rng, 0.18, 0.32);
                byte opacity = (byte)(Rng.FloatBetween(rng, 0.1, 0.2) * 255);
                using var paint = new SKPaint {Color=p.Accent.WithAlpha(opacity), IsAntialias=true};
                canvas.DrawCircle(cx, cy, r, paint);
            }
        }
    }

    private static readonly List<Action<SKCanvas, Func<double>, Palette>> Patterns = new(){
        DrawConcentric, DrawGrid, DrawDiagonalStripes, DrawRadialBurst, DrawPolkaDots, 
    };

    private static List<string> WrapText(string text, int maxCharsPerLine){
        var words = text.Split(' ');
        var lines = new List<string>();
        string current = "";
        foreach (var w in words){
            string candidate = (current + " " + w). Trim();
            if (candidate.Length > maxCharsPerLine && current.Length>0){
                lines.Add(current.Trim());
                current = w;
            }
            else{
                current=candidate;
            }
        }
        if (current.Length>0) lines.Add(current);
        return lines.Take(3).ToList();
    }

    public static byte[] RenderPng(string title, string artist, uint coverSeed){
        var rng = Rng.Mulberry32(coverSeed);
        var palette = MakePalette(rng);
        var pattern = Rng.Pick(rng, Patterns);

        using var bitmap = new SKBitmap(Size, Size);
        using var canvas = new SKCanvas(bitmap);

        float gradAngleDeg = Rng.IntBetween(rng, 0, 359);
        double rad = gradAngleDeg * Math.PI/180.0;
        var center = new SKPoint(Size/2f, Size/2f);
        float dx = (float)(Math.Cos(rad)*Size);
        float dy = (float)(Math.Sin(rad)*Size);
        var start = new SKPoint(center.X - dx/2, center.Y-dy/2);
        var end = new SKPoint(center.X + dx/2, center.Y+dy/2);

        using (var bgPaint = new SKPaint()){ 
            bgPaint.Shader = SKShader.CreateLinearGradient(start, end, new[] { palette.Bg1, palette.Bg2 }, null, SKShaderTileMode.Clamp); 
            canvas.DrawRect(0, 0, Size, Size, bgPaint);
        }

        pattern(canvas, rng, palette);

        using (var scrimPaint = new SKPaint { Color = new SKColor(0, 0, 0, 46) }) 
        {
            canvas.DrawRect(0, Size - 150, Size, 150, scrimPaint);
        }

        var titleLines = WrapText(title, 16);
        bool longLine = titleLines.Any(l => l.Length > 12);
        float titleSize = longLine ? 34 : 42;

        using (var titlePaint = new SKPaint{Color=palette.TextColor, TextSize=titleSize, IsAntialias=true, Typeface=BoldTypeface,})
        {
            float baseY = Size - 95;
            for (int i=0; i<titleLines.Count; i++){
                canvas.DrawText(titleLines[i], 42, baseY + i * (titleSize+4), titlePaint);
            }
        }

        using (var artistPaint = new SKPaint{Color = palette.TextColor.WithAlpha((byte)(0.88*255)), TextSize=22, IsAntialias=true, Typeface=RegularTypeface,})
        {
            canvas.DrawText(artist, 42, Size - 32, artistPaint);
        }
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}