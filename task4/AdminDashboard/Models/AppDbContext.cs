// EF config, unique indexing 
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdminDashboard.Models;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options){}

    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // required UNIQUE INDEX check
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u=>u.Id);
            entity.HasIndex(u=>u.Email).IsUnique().HasDatabaseName("ix_users_email_unique");
            entity.Property(u=>u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
            entity.Property(u=>u.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(u=>u.PasswordHash).HasColumnName("password_hash");
            entity.Property(u=>u.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(u=>u.RegisteredAt).HasColumnName("registered_at");
            entity.Property(u=>u.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(u=>u.EmailVerificationToken).HasColumnName("email_verification_token");
            entity.Property(u=>u.ActivityLog).HasColumnName("activity_log");
            entity.Property(u=>u.WasEverVerified).HasColumnName("was_ever_verified").HasDefaultValue(false);
            entity.Property(u=>u.CurrentSessionStartUnix).HasColumnName("current_session_start_unix");
        });
    }
}


