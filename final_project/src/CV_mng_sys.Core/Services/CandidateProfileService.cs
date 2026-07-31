using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public class CandidateProfileService
{
    private readonly ApplicationDbContext _db;
    public CandidateProfileService(ApplicationDbContext db) => _db = db;

    // All attributes in the library, joined with whichever ones this
    // candidate has already filled in (if any) - drives the "Info" section.
    public async Task<List<(AttributeDefinition Definition, CandidateAttributeValue? Value)>> GetProfileAttributesAsync(string userId)
    {
        var allAttributes = await _db.AttributeDefinitions.OrderBy(a => a.Name).ToListAsync();
        var myValues = await _db.CandidateAttributeValues
            .Where(v => v.CandidateUserId == userId)
            .ToListAsync();

        return allAttributes
            .Select(a => (a, myValues.FirstOrDefault(v => v.AttributeDefinitionId == a.Id)))
            .ToList();
    }
    public async Task<(bool Success, string? Error, uint NewVersion)> SetValueAsync(
        string userId, int attributeDefinitionId, string? value, uint expectedVersion)
    {
        var definition = await _db.AttributeDefinitions.FindAsync(attributeDefinitionId);
        if (definition is null) return (false, "Attribute not found.", 0);

        // Server-side validation matching the attribute's declared type - never
        // trust the client alone, since a raw fetch() call could bypass the
        // HTML5 input constraints entirely.
        if (!string.IsNullOrWhiteSpace(value))
        {
            switch (definition.DataType)
            {
                case AttributeDataType.Number:
                    if (!double.TryParse(value, out _))
                        return (false, "Value must be a number.", 0);
                    break;
                case AttributeDataType.Date:
                    if (!DateOnly.TryParse(value, out _))
                        return (false, "Value must be a valid date (YYYY-MM-DD).", 0);
                    break;
                case AttributeDataType.Boolean:
                    if (value != "true" && value != "false")
                        return (false, "Checkbox value must be true or false.", 0);
                    break;
                case AttributeDataType.Dropdown:
                    if (!definition.GetDropdownOptions().Contains(value))
                        return (false, "Value must be one of the allowed options.", 0);
                    break;
            }
        }

        var existing = await _db.CandidateAttributeValues.FirstOrDefaultAsync(v=>v.CandidateUserId == userId && v.AttributeDefinitionId == attributeDefinitionId);
        if (existing is null)
        {
            var created = new CandidateAttributeValue
            {
                CandidateUserId = userId,
                AttributeDefinitionId = attributeDefinitionId,
                Value = value
            };
            _db.CandidateAttributeValues.Add(created);
            await _db.SaveChangesAsync();
            return (true, null, created.Version);
        }

        _db.Entry(existing).Property(e => e.Version).OriginalValue = expectedVersion;
        existing.Value = value;

        try
        {
            await _db.SaveChangesAsync();
            return (true, null, existing.Version);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, "This value was changed elsewhere. Please reload.", 0);
        }
    }

    public async Task RemoveValueAsync(string userId, int attributeDefinitionId)
    {
        var existing = await _db.CandidateAttributeValues
            .FirstOrDefaultAsync(v => v.CandidateUserId == userId && v.AttributeDefinitionId == attributeDefinitionId);
        if (existing != null)
        {
            _db.CandidateAttributeValues.Remove(existing);
            await _db.SaveChangesAsync();
        }
    }
}