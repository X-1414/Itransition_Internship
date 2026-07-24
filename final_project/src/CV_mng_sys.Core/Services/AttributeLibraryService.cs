using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public class AttributeLibraryService
{
    private readonly ApplicationDbContext _db;

    public AttributeLibraryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<AttributeDefinition>> GetAllAsync() => _db.AttributeDefinitions.OrderBy(a=>a.Name).ToListAsync();
    public Task<AttributeDefinition?> GetByIdAsync(int id) => _db.AttributeDefinitions.FirstOrDefaultAsync(a=>a.Id == id);
    public async Task<AttributeDefinition> CreateAsync(string name, AttributeDataType dataType, string? dropdownOptionsRaw)
    {
        var entity = new AttributeDefinition
        {
            Name = name,
            DataType = dataType,
            DropdownOptionsRaw = dataType == AttributeDataType.Dropdown ? dropdownOptionsRaw : null 
        };
        _db.AttributeDefinitions.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, string name, AttributeDataType dataType, string? dropdownOptionsRaw, uint expectedVersion)
    {
        var entity = await _db.AttributeDefinitions.FirstOrDefaultAsync(a => a.Id == id);
        if (entity == null) return (false, "Attribute definition not found.");
        _db.Entry(entity).Property(e=>e.Version).OriginalValue = expectedVersion;
        entity.Name = name;
        entity.DataType = dataType;
        entity.DropdownOptionsRaw = dataType == AttributeDataType.Dropdown ? dropdownOptionsRaw : null;

        try
        {
            await _db.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "The attribute definition was modified by another user. Please reload and try again.");
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, uint expectedVersion)
    {
        var entity = await _db.AttributeDefinitions.FirstOrDefaultAsync(a => a.Id == id);
        if (entity == null)
            return (true, null); // Already deleted = success

        _db.Entry(entity).Property(e => e.Version).OriginalValue = expectedVersion;
        try
        {
            _db.AttributeDefinitions.Remove(entity);
            await _db.SaveChangesAsync();
            return (true, null);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "The attribute definition was modified by another user. Please reload and try again.");
        }
    } 
}