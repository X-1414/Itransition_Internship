using System.ComponentModel.DataAnnotations;

namespace CV_mng_sys.Core.Entities;

public class CandidateAttributeValue
{
    public int Id {get; set;}
    public string CandidateUserId {get; set;} = string.Empty;
    public ApplicationUser CandidateUser {get; set;} = null!;
    public int AttributeDefinitionId {get; set;}
    public AttributeDefinition AttributeDefinition { get; set; } = null!;
    public string? Value {get; set;}

    [ConcurrencyCheck]
    public uint Version{get; set;}
}