using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CV_mng_sys.Core.Entities;

public class AttributeDefinition
{
    public int Id{get; set;}

    [Required][MaxLength(100)]
    public string Name {get; set;} = string.Empty;

    public AttributeCategory Category {get; set;}

    [MaxLength(1000)]
    public string? Description {get; set;}

    public AttributeDataType DataType {get; set;}
    public string? DropdownOptionsRaw{get; set;}
    public bool IsBuiltIn {get; set;}
    public DateTime? LastUsedUtc {get; set;}

    [ConcurrencyCheck]
    public uint Version {get; set;}
    public List<string> GetDropdownOptions() => string.IsNullOrWhiteSpace(DropdownOptionsRaw) ? new List<string>() : DropdownOptionsRaw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
}