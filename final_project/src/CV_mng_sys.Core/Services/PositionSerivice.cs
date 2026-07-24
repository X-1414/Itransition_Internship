using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public class PositionService
{
    private readonly ApplicationDbContext _db;
    public PositionService(ApplicationDbContext db) => _db = db;
    public Task<List<Position>> GetAllAsync() => _db.Positions.OrderBy(p => p.Title).ToListAsync();
    public Task<Position?> GetByIdAsync(int id) =>
        _db.Positions.Include(p => p.Attributes).ThenInclude(pa => pa.AttributeDefinition).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Position> CreateAsync(string title, string? description)
    {
        var entity = new Position { Title = title, Description = description };
        _db.Positions.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, string title, string? description, uint expectedVersion)
    {
        var entity = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return (false, "Position not found.");

        _db.Entry(entity).Property(e => e.Version).OriginalValue = expectedVersion;
        entity.Title = title;
        entity.Description = description;

        try { await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This position was modified by someone else. Please reload."); }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, uint expectedVersion)
    {
        var entity = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return (true, null);

        _db.Entry(entity).Property(e => e.Version).OriginalValue = expectedVersion;
        try { _db.Positions.Remove(entity); await _db.SaveChangesAsync(); return (true, null); }
        catch (DbUpdateConcurrencyException) { return (false, "This position was modified by someone else. Please reload."); }
    }

    public async Task<Position> DuplicateAsync(int id)
    {
        var original = await GetByIdAsync(id) ?? throw new InvalidOperationException("Position not found.");
        var copy = new Position { Title = original.Title + " (Copy)", Description = original.Description };
        _db.Positions.Add(copy);
        await _db.SaveChangesAsync();

        foreach (var pa in original.Attributes)
        {
            _db.PositionAttributes.Add(new PositionAttribute
            {
                PositionId = copy.Id,
                AttributeDefinitionId = pa.AttributeDefinitionId,
                IsRequired = pa.IsRequired,
                SortOrder = pa.SortOrder
            });
        }
        await _db.SaveChangesAsync();
        return copy;
    }

    public async Task AddAttributeAsync(int positionId, int attributeDefinitionId, bool isRequired)
    {
        var exists = await _db.PositionAttributes.AnyAsync(pa => pa.PositionId == positionId && pa.AttributeDefinitionId == attributeDefinitionId);
        if (exists) return;

        _db.PositionAttributes.Add(new PositionAttribute
        {
            PositionId = positionId,
            AttributeDefinitionId = attributeDefinitionId,
            IsRequired = isRequired,
            SortOrder = await _db.PositionAttributes.CountAsync(pa => pa.PositionId == positionId)
        });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAttributeAsync(int positionAttributeId)
    {
        var pa = await _db.PositionAttributes.FindAsync(positionAttributeId);
        if (pa != null) { _db.PositionAttributes.Remove(pa); await _db.SaveChangesAsync(); }
    }
    
    public Task<int> GetActivePositionCountAsync() => _db.Positions.CountAsync(p => p.IsActive);
}