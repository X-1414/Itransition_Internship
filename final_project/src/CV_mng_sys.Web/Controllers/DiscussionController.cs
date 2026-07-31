using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CV_mng_sys.Core.Entities;
using CV_mng_sys.Core.Services;

namespace CV_mng_sys.Web.Controllers;

[Authorize]
public class DiscussionController : Controller
{
    private readonly DiscussionService _discussions;
    private readonly UserManager<ApplicationUser> _userManager;
    public DiscussionController(DiscussionService discussions, UserManager<ApplicationUser> userManager)
    {
        _discussions = discussions;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Poll(int discussionPositionId, int afterId = 0)
    {
        var posts = await _discussions.GetPostsAsync(discussionPositionId, afterId);
        return Ok(posts.Select(p=> new
        {
            p.Id,
            AuthorName = p.AuthorUser.Email,
            AuthorUserId = p.AuthorUserId,
            p.ContentMarkdown,
            CreatedAtUtc = p.CreatedAtUtc.ToString("o")
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Post(int discussionPositionId, string content)
    {
        if(string.IsNullOrWhiteSpace(content)) return BadRequest(new {error = "Post cannot be empty."});
        var userId = _userManager.GetUserId(User)!;
        var post = await _discussions.AddPostAsync(discussionPositionId, userId, content);
        return Ok(new
        {
            post.Id,
            AuthorName = post.AuthorUser.Email,
            AuthorUserId = post.AuthorUserId,
            post.ContentMarkdown,
            CreatedAtUtc = post.CreatedAtUtc.ToString("o")
        });
    }
}