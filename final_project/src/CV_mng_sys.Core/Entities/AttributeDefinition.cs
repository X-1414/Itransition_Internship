using System.ComponentModel.DataAnnotations;

namespace CV_mng_sys.Core.Entities;

public class AttributeDefinition
{
    public int Id{get; set;}

    [Required][MaxLength(100)]
    public string Name {get; set;} = string.Empty;
    public AttributeDataType DataType {get; set;}
    public string? DropdownOptionsRaw{get; set;}

    [ConcurrencyCheck]
    public uint Version {get; set;}
    public List<string> GetDropdownOptions() => string.IsNullOrWhiteSpace(DropdownOptionsRaw) ? new List<string>() : DropdownOptionsRaw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
}