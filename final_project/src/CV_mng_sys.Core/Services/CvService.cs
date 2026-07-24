using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

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

    public async Task<List<CvDocument>> GetPublishedForPositionAsync(int positionId)
    {
        return await _db.CvDocuments.Include(cv => cv.CandidateUser).Where(cv => cv.PositionId == positionId && cv.Status == CvStatus.Published).ToListAsync();
    }

    public async Task<int> GetCvsCreatedInLastDaysAsync(int days)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _db.CvDocuments.CountAsync(cv => cv.CreatedAtUtc >= cutoff);
    }
}