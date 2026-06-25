using Microsoft.EntityFrameworkCore;

namespace Playlist.Data;

public class PlaylistDbContext : DbContext{
    public PlaylistDbContext(DbContextOptions<PlaylistDbContext> options) : base (options) { }

    public DbSet<Locale> Locales => Set<Locale>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<TitleNoun> TitleNouns => Set<TitleNoun>();
    public DbSet<TitleAdjective> TitleAdjectives => Set<TitleAdjective>();
    public DbSet<AlbumWord> AlbumWords => Set<AlbumWord>();
    public DbSet<ReviewSentence> ReviewSentences => Set<ReviewSentence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Entity<Locale>(e => 
        {
            e.HasKey(l=>l.Code);
            e.Property(l=>l.Code).HasMaxLength(8);
            e.Property(l=>l.Label).HasMaxLength(64).IsRequired();
            e.Property(l=>l.NameLocale).HasMaxLength(8).IsRequired();
        });
        
        modelBuilder.Entity<Genre>(e => 
        {
            e.HasKey(g=>g.Id);
            e.Property(g=>g.Name).HasMaxLength(64).IsRequired();
            e.HasOne(g=>g.Locale).WithMany(l=>l.Genres).HasForeignKey(g=>g.LocaleCode).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(g=>g.LocaleCode);
        });

        modelBuilder.Entity<TitleNoun>(e => {
            e.HasKey(n=>n.Id);
            e.Property(n=>n.Word).HasMaxLength(64).IsRequired();
            e.HasOne(n=>n.Locale).WithMany(l=>l.TitleNouns).HasForeignKey(n=>n.LocaleCode).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(n=>n.LocaleCode);
        });

        modelBuilder.Entity<TitleAdjective>(e => {
            e.HasKey(a=>a.Id);
            e.Property(a=>a.Word).HasMaxLength(64).IsRequired();
            e.HasOne(a=>a.Locale).WithMany(l=>l.TitleAdjectives).HasForeignKey(a=>a.LocaleCode).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(a=>new{ a.LocaleCode, a.Gender });
        });

        modelBuilder.Entity<AlbumWord>(e => {
            e.HasKey(w=>w.Id);
            e.Property(w=>w.Word).HasMaxLength(64).IsRequired();
            e.HasOne(w=>w.Locale).WithMany(l=>l.AlbumWords).HasForeignKey(w=>w.LocaleCode).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(w=>w.LocaleCode);
        });


        modelBuilder.Entity<ReviewSentence>(e => 
        {
            e.HasKey(r=>r.Id);
            e.Property(r=>r.Text).HasMaxLength(256).IsRequired();
            e.HasOne(r=>r.Locale).WithMany(l=>l.ReviewSentences).HasForeignKey(r=>r.LocaleCode).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r=>r.LocaleCode);
        });
    }
}