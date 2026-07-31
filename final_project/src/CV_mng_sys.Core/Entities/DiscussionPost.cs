namespace CV_mng_sys.Core.Entities;

public class DiscussionPost
{
    public int Id { get; set; }

    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public string AuthorUserId {get; set;} = string.Empty;
    public ApplicationUser AuthorUser {get; set;} = null!;
    public string ContentMarkdown {get; set;} = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}