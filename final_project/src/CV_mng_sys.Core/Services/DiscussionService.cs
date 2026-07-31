using Microsoft.EntityFrameworkCore;
using CV_mng_sys.Core.Data;
using CV_mng_sys.Core.Entities;

namespace CV_mng_sys.Core.Services;

public class DiscussionService
{
    private readonly ApplicationDbContext _db;
    public DiscussionService(ApplicationDbContext db) => _db = db;

    public async Task<List<DiscussionPost>> GetPostsAsync(int positionId, int afterId = 0)
    {
        return await _db.DiscussionPosts.Include(p=>p.AuthorUser).Where(p=>p.PositionId == positionId && p.Id > afterId).OrderBy(p=>p.CreatedAtUtc).ToListAsync();
    }

    public async Task<DiscussionPost> AddPostAsync(int positionId, string authorUserId, string contentMartkdown)
    {
        var post = new DiscussionPost
        {
            PositionId = positionId,
            AuthorUserId = authorUserId,
            ContentMarkdown = contentMartkdown
        };
        _db.DiscussionPosts.Add(post);
        await _db.SaveChangesAsync();
        return (await _db.DiscussionPosts.Include(p=>p.AuthorUser).FirstAsync(p=>p.Id == post.Id));
    }
}