using CV_mng_sys.Core.Entities;
using Xunit;

namespace CV_mng_sys.Tests;

public class AttributeDefinitionTests
{
    [Fact]
    public void GetDropdownOptions_ParsesCommaSeparatedValues()
    {
        var attr = new AttributeDefinition
        {
            Name = "English Level",
            DataType = AttributeDataType.Dropdown,
            DropdownOptionsRaw = "Beginner, Intermediate, Advanced"
        };

        var options = attr.GetDropdownOptions();

        Assert.Equal(3, options.Count);
        Assert.Equal("Beginner", options[0]);
        Assert.Equal("Intermediate", options[1]);
        Assert.Equal("Advanced", options[2]);
    }

    [Fact]
    public void GetDropdownOptions_TrimsWhitespaceAroundEachOption()
    {
        var attr = new AttributeDefinition
        {
            Name = "Level",
            DataType = AttributeDataType.Dropdown,
            DropdownOptionsRaw = "  Junior ,Middle,  Senior  "
        };

        var options = attr.GetDropdownOptions();
        Assert.Equal(new[] { "Junior", "Middle", "Senior" }, options);
    }

    [Fact]
    public void GetDropdownOptions_ReturnsEmptyList_WhenRawIsNullOrWhitespace()
    {
        var attr = new AttributeDefinition { Name = "X", DataType = AttributeDataType.Dropdown, DropdownOptionsRaw = null };
        Assert.Empty(attr.GetDropdownOptions());
    }
}