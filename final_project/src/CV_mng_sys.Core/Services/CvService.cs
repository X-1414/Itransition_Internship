using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace CV_mng_sys.Core.Services;

public record CvAttributeRow(
    int AttributeDefinitionId,
    string Name,
    AttributeDataType DataType,
    string? DropdownOptionsRaw,
    bool IsRequired,
    string? Value,
    uint ValueVersion 
);

public class CvService
{
    private readonly ApplicationDbContext _db;
    public CvService(ApplicationDbContext db) => _db = db;
    public async Task<List<CvDocument>> GetMyCvsAsync(string candidateUserId)
    {
        return await _db.CvDocuments.Include(cv => cv.Position).Where(cv => cv.CandidateUserId == candidateUserId).OrderByDescending(cv => cv.CreatedAtUtc).ToListAsync();
    }
    public async Task<CvDocument> GetOrCreateAsync(string candidateUserId, int positionId)
    {
        var existing = await _db.CvDocuments.FirstOrDefaultAsync(cv => cv.CandidateUserId == candidateUserId && cv.PositionId == positionId);
        if (existing != null) return existing;

        var created = new CvDocument
        {
            CandidateUserId = candidateUserId,
            PositionId = positionId,
            Status = CvStatus.Draft
        };
        _db.CvDocuments.Add(created);
        await _db.SaveChangesAsync();
        return created;
    }

    public async Task<CvDocument?> GetByIdAsync(int id)
    {
        return await _db.CvDocuments.Include(cv => cv.Position).Include(cv => cv.CandidateUser).FirstOrDefaultAsync(cv => cv.Id == id);
    }
    public async Task<List<CvAttributeRow>> GetAttributeRowsAsync(int positionId, string candidateUserId)
    {
        var positionAttributes = await _db.PositionAttributes.Include(pa => pa.AttributeDefinition).Where(pa => pa.PositionId == positionId).OrderBy(pa => pa.SortOrder).ToListAsync();

        var candidateValues = await _db.CandidateAttributeValues.Where(v => v.CandidateUserId == candidateUserId).ToListAsync();

        return positionAttributes.Select(pa =>
        {
            var val = candidateValues.FirstOrDefault(v => v.AttributeDefinitionId == pa.AttributeDefinitionId);
            return new CvAttributeRow(
                pa.AttributeDefinitionId,
                pa.AttributeDefinition.Name,
                pa.AttributeDefinition.DataType,
                pa.AttributeDefinition.DropdownOptionsRaw,
                pa.IsRequired,
                val?.Value,
                val?.Version ?? 0
            );
        }).ToList();
    }

    public async Task<bool> CanPublishAsync(int positionId, string candidateUserId)
    {
        var rows = await GetAttributeRowsAsync(positionId, candidateUserId);
        return rows.Where(r => r.IsRequired).All(r => !string.IsNullOrWhiteSpace(r.Value));
    }

    public async Task<(bool Success, string? Error)> PublishAsync(int cvId, uint expectedVersion)
    {
        var cv = await _db.CvDocuments.FirstOrDefaultAsync(c => c.Id == cvId);
        if (cv is null) return (false, "CV not found.");

        if (!await CanPublishAsync(cv.PositionId, cv.CandidateUserId))
            return (false, "All required attributes must be filled in before publishing.");

        _db.Entry(cv).Property(e => e.Version).OriginalValue = expectedVersion;
        cv.Status = CvStatus.Published;
        cv.PublishedAtUtc = DateTime.UtcNow;

        try { await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This CV was modified elsewhere. Please reload."); }
    }

    public async Task<(bool Success, string? Error)> UnpublishAsync(int cvId, uint expectedVersion)
    {
        var cv = await _db.CvDocuments.FirstOrDefaultAsync(c => c.Id == cvId);
        if (cv is null) return (false, "CV not found.");

        _db.Entry(cv).Property(e => e.Version).OriginalValue = expectedVersion;
        cv.Status = CvStatus.Draft;
        cv.PublishedAtUtc = null;

        try { await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This CV was modified elsewhere. Please reload."); }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int cvId, uint expectedVersion)
    {
        var cv = await _db.CvDocuments.FirstOrDefaultAsync(c => c.Id == cvId);
        if (cv is null) return (true, null);

        _db.Entry(cv).Property(e => e.Version).OriginalValue = expectedVersion;
        try { _db.CvDocuments.Remove(cv); await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This CV was modified elsewhere. Please reload."); }
    }

    public async Task<List<CvDocument>> GetAllForPositionAsync(int positionId)
    {
        return await _db.CvDocuments.Include(cv=>cv.CandidateUser).Where(cv=>cv.PositionId==positionId).ToListAsync();
    }
    
    public async Task<List<CvDocument>> GetPublishedForPositionAsync(int positionId)
    {
        return await _db.CvDocuments.Include(cv => cv.CandidateUser).Where(cv => cv.PositionId == positionId && cv.Status == CvStatus.Published).ToListAsync();
    }

    public async Task<int> GetCvsCreatedInLastDaysAsync(int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _db.CvDocuments.CountAsync(cv => cv.CreatedAtUtc >= cutoff);
    }

    public async Task<List<Project>> GetFilteredProjectsAsync(int positionId, string candidateUserId)
    {
        var position = await _db.Positions.FirstOrDefaultAsync(p=>p.Id == positionId);
        if (position is null) return new List<Project>();

        var positionTags = position.GetProjectTags();
        var candidateProjects = await _db.Projects.Where(p=>p.CandidateUserId == candidateUserId).OrderByDescending(p=>p.StartDate).ToListAsync();
        var eligible = positionTags.Count == 0 ? candidateProjects : candidateProjects.Where(p=>p.GetTags().Any(t=>positionTags.Contains(t, StringComparer.OrdinalIgnoreCase))).ToList();
        return eligible.Take(position.MaxProjectsInCv).ToList();
    }

    public async Task<Dictionary<int, bool>> GetAccessStatusForCvsAsync(List<CvDocument> cvs, PositionService positionService, string candidateUserId)
    {
        var result = new Dictionary<int, bool>();
        foreach (var cv in cvs)
        {
            result[cv.Id] = await positionService.CandidateHasAccessAsync(cv.PositionId, candidateUserId);
        }
        return result;
    }

    public Task<int> GetLikeCountAsync(int cvId) => _db.CvLikes.CountAsync(l=>l.CvDocumentId==cvId);
    public Task<bool> HasLikedAsync(int cvId, string recruiterUserId) => _db.CvLikes.AnyAsync(l=>l.CvDocumentId==cvId && l.RecruiterUserId==recruiterUserId);
    public async Task LikeAsync(int cvId, string recruiterUserId)
    {
        var exists = await _db.CvLikes.AnyAsync(l=>l.CvDocumentId==cvId && l.RecruiterUserId==recruiterUserId);
        if (exists) return;
        _db.CvLikes.Add(new CvLike {CvDocumentId = cvId, RecruiterUserId = recruiterUserId});
        await _db.SaveChangesAsync();
    }
    public async Task UnlikeAsync(int cvId, string recruiterUserId)
    {
        var like = await _db.CvLikes.FirstOrDefaultAsync(l=>l.CvDocumentId==cvId && l.RecruiterUserId==recruiterUserId);
        if (like != null)
        {
            _db.CvLikes.Remove(like);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<int, int>> GetLikeCountsAsync(List<int> cvIds)
    {
        return await _db.CvLikes.Where(l=>cvIds.Contains(l.CvDocumentId)).GroupBy(l=>l.CvDocumentId).ToDictionaryAsync(g=>g.Key, g=>g.Count());
    }
}