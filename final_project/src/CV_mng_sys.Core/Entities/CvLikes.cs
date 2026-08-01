namespace CV_mng_sys.Core.Entities;

public class CvLike
{
    public int Id {get; set;}
    public int CvDocumentId {get; set;}
    public CvDocument CvDocument {get; set;} = null!;
    public string RecruiterUserId {get; set;} = string.Empty;
    public ApplicationUser RecruiterUser {get; set;} = null!;
    public DateTime CreatedAtUtc {get; set;} = DateTime.UtcNow;
}