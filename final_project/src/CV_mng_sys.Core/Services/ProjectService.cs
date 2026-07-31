using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public class ProjectService
{
    private readonly ApplicationDbContext _db;
    public ProjectService(ApplicationDbContext db) => _db=db;

    public Task<List<Project>> GetForCandidateAsync(string candidateUserId) => _db.Projects.Where(p=>p.CandidateUserId == candidateUserId).OrderByDescending(p=>p.StartDate).ToListAsync();
    public Task<Project?> GetByIdAsync(int id) => _db.Projects.FirstOrDefaultAsync(p=>p.Id == id);
    public async Task<Project> CreateAsync(string candidateUserId, string name, DateOnly? start, DateOnly? end, string? descriptionMarkdown, string? tagsRaw)
    {
        var entity = new Project
        {
            CandidateUserId = candidateUserId,
            Name = name,
            StartDate = start,
            EndDate = end,
            DescriptionMarkdown = descriptionMarkdown,
            TagsRaw = tagsRaw
        };
        _db.Projects.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
    public async Task<(bool Success, string? Error)> UpdateAsync(int id, string name, DateOnly? start, DateOnly? end, string? descriptionMarkdown, string? tagsRaw, uint expectedVersion)
    {
        var entity = await _db.Projects.FirstOrDefaultAsync(p=>p.Id == id);
        if (entity is null) return (false, "Project not found.");
        _db.Entry(entity).Property(e=>e.Version).OriginalValue = expectedVersion;

        entity.Name = name;
        entity.StartDate = start;
        entity.EndDate = end;
        entity.DescriptionMarkdown = descriptionMarkdown;
        entity.TagsRaw = tagsRaw;

        try {await _db.SaveChangesAsync(); return (true, null);}
        catch (DbUpdateConcurrencyException){ return (false, "This project was modified elsewhere. Please reload.");}
    }

    public async Task <(bool Success, string? Error)> DeleteAsync(int id, uint expectedVersion)
    {
        var entity = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return (true, null);

        _db.Entry(entity).Property(e => e.Version).OriginalValue = expectedVersion;
        try { _db.Projects.Remove(entity); await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This project was modified elsewhere. Please reload."); }
    }

    public async Task<List<string>> GetAllDistinctTagsAsync()
    {
        var allTagsRaw = await _db.Projects.Where(p => p.TagsRaw != null).Select(p => p.TagsRaw!).ToListAsync();
        return allTagsRaw.SelectMany(raw => raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    }
}