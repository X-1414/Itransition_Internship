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

    public async Task<Position> CreateAsync(string title, string? description, string? projectTagsRaw, int maxProjectsInCv)
    {
        var entity = new Position { Title = title, Description = description, ProjectTagsRaw = projectTagsRaw, MaxProjectsInCv = maxProjectsInCv};
        _db.Positions.Add(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(int id, string title, string? description, string? projectTagsRaw, int maxProjectsInCv, uint expectedVersion)
    {
        var entity = await _db.Positions.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null) return (false, "Position not found.");

        _db.Entry(entity).Property(e => e.Version).OriginalValue = expectedVersion;
        entity.Title = title;
        entity.Description = description;
        entity.ProjectTagsRaw = projectTagsRaw;
        entity.MaxProjectsInCv = maxProjectsInCv;

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
        var copy = new Position { Title = original.Title + " (Copy)", Description = original.Description, ProjectTagsRaw = original.ProjectTagsRaw, MaxProjectsInCv = original.MaxProjectsInCv};
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
        var attribute = await _db.AttributeDefinitions.FindAsync(attributeDefinitionId);
        if (attribute != null) attribute.LastUsedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAttributeAsync(int positionAttributeId)
    {
        var pa = await _db.PositionAttributes.FindAsync(positionAttributeId);
        if (pa != null) { _db.PositionAttributes.Remove(pa); await _db.SaveChangesAsync(); }
    }
    
    public Task<int> GetActivePositionCountAsync() => _db.Positions.CountAsync(p => p.IsActive);

    public async Task<bool> CandidateHasAccessAsync(int positionId, string candidateUserId)
    {
        var rules = await _db.PositionAccessRules.Include(r=>r.AttributeDefinition).Where(r=>r.PositionId == positionId).ToListAsync();
        if (rules.Count == 0) return true;
        var candidateValues = await _db.CandidateAttributeValues.Where(v=>v.CandidateUserId == candidateUserId).ToListAsync();
        foreach (var rule in rules)
        {
            var value = candidateValues.FirstOrDefault(v=>v.AttributeDefinitionId == rule.AttributeDefinitionId)?.Value;
            if(!EvaluateRule(rule, value)) return false;
        }
        return true;
    }

    private static bool EvaluateRule(PositionAccessRule rule, string? candidateValue)
    {
        switch (rule.Operator)
        {
            case AccessRuleOperator.IsChecked:
                return candidateValue == "true";
            case AccessRuleOperator.IsUnchecked:
                return candidateValue != "true";
            case AccessRuleOperator.Contains:
                if(string.IsNullOrEmpty(candidateValue) || string.IsNullOrWhiteSpace(rule.ComparisonValue)) return false;
                var candidates = rule.ComparisonValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                return candidates.Any(c=>candidateValue.Contains(c, StringComparison.OrdinalIgnoreCase));
            case AccessRuleOperator.Equals:
                return string.Equals(candidateValue, rule.ComparisonValue, StringComparison.OrdinalIgnoreCase);
            case AccessRuleOperator.GreaterThan:
            case AccessRuleOperator.LessThan:
            case AccessRuleOperator.GreaterThanEqual:
            case AccessRuleOperator.LessThanEqual:
                if (!double.TryParse(candidateValue, out var candidateNum)) return false;
                if (!double.TryParse(rule.ComparisonValue, out var ruleNum)) return false;
                return rule.Operator switch
                {
                    AccessRuleOperator.GreaterThan => candidateNum > ruleNum,
                    AccessRuleOperator.LessThan => candidateNum < ruleNum,
                    AccessRuleOperator.GreaterThanEqual => candidateNum >= ruleNum,
                    AccessRuleOperator.LessThanEqual => candidateNum <= ruleNum,
                    _ => false
                };
            default:
                return false;
        }
    }

    public async Task<List<PositionAccessRule>> GetAccessRulesAsync(int positionId) => await _db.PositionAccessRules.Include(r=>r.AttributeDefinition).Where(r=>r.PositionId == positionId).ToListAsync();
    public async Task SetAccessRulesAsync(int positionId, List<(int AttributeDefinitionId, AccessRuleOperator Operator, string? ComparisonValue)> rules)
    {
        var existing = await _db.PositionAccessRules.Where(r=>r.PositionId == positionId).ToListAsync();
        _db.PositionAccessRules.RemoveRange(existing);
        foreach(var(attrId, op, val) in rules)
        {
            _db.PositionAccessRules.Add(new PositionAccessRule
            {
                PositionId = positionId,
                AttributeDefinitionId = attrId,
                Operator = op,
                ComparisonValue = val
            });
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> GetUnmetRequirementAttributeNamesAsync(int positionId, string candidateUserId)
    {
        var rules = await _db.PositionAccessRules.Include(r=>r.AttributeDefinition).Where(r=>r.PositionId == positionId).ToListAsync();
        if (rules.Count == 0) return new List<string>();
        var candidateValues = await _db.CandidateAttributeValues.Where(v=>v.CandidateUserId == candidateUserId).ToListAsync();
        var unmet = new List<string>();
        foreach (var rule in rules)
        {
            var value = candidateValues.FirstOrDefault(v=>v.AttributeDefinitionId == rule.AttributeDefinitionId)?.Value;
            if(!EvaluateRule(rule, value)) unmet.Add(rule.AttributeDefinition.Name);
        }
        return unmet;
    }
}