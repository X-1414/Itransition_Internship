using System.ComponentModel.DataAnnotations;

namespace CV_mng_sys.Core.Entities;

public class Project
{
    public int Id {get; set;}
    public string CandidateUserId {get; set;} = string.Empty;
    public ApplicationUser CandidateUser {get; set;} = null!;

    [Required, MaxLength(200)]
    public string Name {get; set;} = string.Empty;

    public DateOnly? StartDate {get; set;}
    public DateOnly? EndDate {get; set;}
    public string? DescriptionMarkdown {get; set;}
    public string? TagsRaw {get; set;}

    [ConcurrencyCheck]
    public uint Version {get; set;}

    public List<string> GetTags() => string.IsNullOrWhiteSpace(TagsRaw) ? new List<string>() : TagsRaw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
}