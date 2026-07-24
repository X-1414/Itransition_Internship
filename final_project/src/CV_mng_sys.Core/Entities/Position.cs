using System.ComponentModel.DataAnnotations;

namespace CV_mng_sys.Core.Entities;

public class Position
{
    public int Id {get; set;}

    [Required][MaxLength(100)]
    public string Title {get; set;} = string.Empty;
    public string? Description {get; set;}
    public bool IsActive {get; set;} = true;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public List<PositionAttribute> Attributes {get; set;} = new();

    [ConcurrencyCheck]
    public uint Version {get; set;}
}