using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;
using Xunit;

namespace CV_mng_sys.Tests;

public class CvServiceTests
{
    private static ApplicationDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: dbName).Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAttributeRowsAsync_JoinsPositionTemplateWithCandidateValues()
    {
        await using var db = CreateInMemoryDb(nameof(GetAttributeRowsAsync_JoinsPositionTemplateWithCandidateValues));

        var gpaAttr = new AttributeDefinition { Name = "GPA", DataType = AttributeDataType.Number };
        var englishAttr = new AttributeDefinition { Name = "English Level", DataType = AttributeDataType.Dropdown, DropdownOptionsRaw = "Beginner,Advanced" };
        db.AttributeDefinitions.AddRange(gpaAttr, englishAttr);

        var position = new Position { Title = "Business Analyst" };
        db.Positions.Add(position);
        await db.SaveChangesAsync();

        db.PositionAttributes.AddRange(
            new PositionAttribute { PositionId = position.Id, AttributeDefinitionId = gpaAttr.Id, IsRequired = true, SortOrder = 0 },
            new PositionAttribute { PositionId = position.Id, AttributeDefinitionId = englishAttr.Id, IsRequired = false, SortOrder = 1 }
        );

        db.CandidateAttributeValues.Add(new CandidateAttributeValue
        {
            CandidateUserId = "candidate-1",
            AttributeDefinitionId = gpaAttr.Id,
            Value = "3.9"
        });
        await db.SaveChangesAsync();

        var service = new CvService(db);
        var rows = await service.GetAttributeRowsAsync(position.Id, "candidate-1");
        Assert.Equal(2, rows.Count);

        var gpaRow = rows.Single(r => r.Name == "GPA");
        Assert.Equal("3.9", gpaRow.Value);
        Assert.True(gpaRow.IsRequired);

        var englishRow = rows.Single(r => r.Name == "English Level");
        Assert.Null(englishRow.Value); // never filled in - should surface as empty, not throw
        Assert.False(englishRow.IsRequired);
    }

    [Fact]
    public async Task CanPublishAsync_ReturnsFalse_WhenARequiredAttributeIsEmpty()
    {
        await using var db = CreateInMemoryDb(nameof(CanPublishAsync_ReturnsFalse_WhenARequiredAttributeIsEmpty));

        var gpaAttr = new AttributeDefinition { Name = "GPA", DataType = AttributeDataType.Number };
        db.AttributeDefinitions.Add(gpaAttr);
        var position = new Position { Title = "QA Engineer" };
        db.Positions.Add(position);
        await db.SaveChangesAsync();

        db.PositionAttributes.Add(new PositionAttribute
        {
            PositionId = position.Id,
            AttributeDefinitionId = gpaAttr.Id,
            IsRequired = true
        });
        await db.SaveChangesAsync();

        var service = new CvService(db);
        var canPublish = await service.CanPublishAsync(position.Id, "candidate-1");
        Assert.False(canPublish);
    }

    [Fact]
    public async Task CanPublishAsync_ReturnsTrue_WhenAllRequiredAttributesAreFilled()
    {
        await using var db = CreateInMemoryDb(nameof(CanPublishAsync_ReturnsTrue_WhenAllRequiredAttributesAreFilled));

        var gpaAttr = new AttributeDefinition { Name = "GPA", DataType = AttributeDataType.Number };
        db.AttributeDefinitions.Add(gpaAttr);
        var position = new Position { Title = "QA Engineer" };
        db.Positions.Add(position);
        await db.SaveChangesAsync();

        db.PositionAttributes.Add(new PositionAttribute
        {
            PositionId = position.Id,
            AttributeDefinitionId = gpaAttr.Id,
            IsRequired = true
        });
        db.CandidateAttributeValues.Add(new CandidateAttributeValue
        {
            CandidateUserId = "candidate-1",
            AttributeDefinitionId = gpaAttr.Id,
            Value = "3.5"
        });
        await db.SaveChangesAsync();

        var service = new CvService(db);
        var canPublish = await service.CanPublishAsync(position.Id, "candidate-1");
        Assert.True(canPublish);
    }

    [Fact]
    public async Task GetOrCreateAsync_IsIdempotent_DoesNotDuplicateOnSecondCall()
    {
        await using var db = CreateInMemoryDb(nameof(GetOrCreateAsync_IsIdempotent_DoesNotDuplicateOnSecondCall));

        var position = new Position { Title = "DevOps Engineer" };
        db.Positions.Add(position);
        await db.SaveChangesAsync();

        var service = new CvService(db);
        var first = await service.GetOrCreateAsync("candidate-1", position.Id);
        var second = await service.GetOrCreateAsync("candidate-1", position.Id);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, await db.CvDocuments.CountAsync());
    }
}