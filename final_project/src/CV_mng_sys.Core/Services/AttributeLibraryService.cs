using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;
using System.Net.Mail;
using System.Runtime.CompilerServices;

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
    public async Task<(bool Success, string? Error, AttributeDefinition? Created)> CreateAsync(string name, AttributeCategory category, string? description, AttributeDataType dataType, string? dropdownOptionsRaw)
    {
        var nameExists = await _db.AttributeDefinitions.AnyAsync(a=>a.Name==name);
        if (nameExists) return (false, "An attribute with this name already exists.", null);
        var entity = new AttributeDefinition
        {
            Name = name,
            Category = category,
            Description = description,
            DataType = dataType,
            DropdownOptionsRaw = dataType == AttributeDataType.Dropdown ? dropdownOptionsRaw : null 
        };
        _db.AttributeDefinitions.Add(entity);
        await _db.SaveChangesAsync();
        return (true, null, entity);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, string name, AttributeCategory category, string? description, AttributeDataType dataType, string? dropdownOptionsRaw, uint expectedVersion)
    {
        var entity = await _db.AttributeDefinitions.FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return (false, "Attribute definition not found.");
        
        var nameTaken = await _db.AttributeDefinitions.AnyAsync(a=>a.Name == name && a.Id != id);
        if (nameTaken) return (false, "An attribute with this name already exists.");

        _db.Entry(entity).Property(e=>e.Version).OriginalValue = expectedVersion;
        entity.Name = name;
        entity.Category = category;
        entity.Description = description;
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
        if (entity.IsBuiltIn) return (false, "This is a built-in attribute and cannot be deleted.");
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
    
    public Task<List<AttributeDefinition>> GetRecentlyUsedAsync(int count=5) => _db.AttributeDefinitions.Where(a=>a.LastUsedUtc!=null).OrderByDescending(a=>a.LastUsedUtc).Take(count).ToListAsync();
}