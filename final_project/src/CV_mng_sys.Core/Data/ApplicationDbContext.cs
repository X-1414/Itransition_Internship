using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<PositionAttribute> PositionAttributes => Set<PositionAttribute>();
    public DbSet<CandidateAttributeValue> CandidateAttributeValues => Set<CandidateAttributeValue>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<PositionAccessRule> PositionAccessRules => Set<PositionAccessRule>();
    public DbSet<DiscussionPost> DiscussionPosts => Set<DiscussionPost>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PositionAttribute>().HasIndex(pa => new { pa.PositionId, pa.AttributeDefinitionId }).IsUnique();
        builder.Entity<CandidateAttributeValue>().HasIndex(cav => new { cav.CandidateUserId, cav.AttributeDefinitionId }).IsUnique();
        builder.Entity<AttributeDefinition>().Property(a => a.Version).IsRowVersion();
        builder.Entity<AttributeDefinition>().HasIndex(a=>a.Name).IsUnique();
        builder.Entity<Position>().Property(p => p.Version).IsRowVersion();
        builder.Entity<CandidateAttributeValue>().Property(c => c.Version).IsRowVersion();
        builder.Entity<CvDocument>().HasIndex(cv => new { cv.PositionId, cv.CandidateUserId }).IsUnique();
        builder.Entity<CvDocument>().Property(c => c.Version).IsRowVersion();
        builder.Entity<Project>().Property(p=>p.Version).IsRowVersion();
    }
    public DbSet<CvDocument> CvDocuments => Set<CvDocument>();
}