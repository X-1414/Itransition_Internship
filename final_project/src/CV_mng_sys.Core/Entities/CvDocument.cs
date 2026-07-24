using System.ComponentModel.DataAnnotations;

namespace CV_mng_sys.Core.Entities;
public class CvDocument
{
    public int Id { get; set; }

    public int PositionId { get; set; }
    public Position Position { get; set; } = null!;

    public string CandidateUserId { get; set; } = string.Empty;
    public ApplicationUser CandidateUser { get; set; } = null!;

    public CvStatus Status { get; set; } = CvStatus.Draft;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; set; }

    [ConcurrencyCheck]
    public uint Version { get; set; }
}