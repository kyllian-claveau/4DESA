using LinkUp.Interfaces;
using LinkUp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IMediaService _mediaService;
    private readonly ICommentService _commentService;

    public PostsController(IPostService postService, IMediaService mediaService, ICommentService commentService)
    {
        _postService = postService;
        _mediaService = mediaService;
        _commentService = commentService;
    }

    [HttpGet("my-posts")]
    public async Task<IActionResult> GetMyPosts()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User not authenticated.");
        }
        
        var posts = await _postService.GetUserPostsAsync(userId);
        
        return Ok(posts);
    }

    [HttpGet]
    public async Task<IActionResult> GetVisiblePosts()
    {
        var posts = await _postService.GetVisiblePostsAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(
        Guid id, 
        [FromForm] string content, 
        [FromForm] IFormFile[]? media = null)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (post.UserId != userId)
            return Forbid();
        
        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;
        
        if (media != null && media.Length > 0)
        {
            foreach (var mediaUrl in post.MediaUrls)
            {
                await _mediaService.DeleteMediaAsync(mediaUrl);
            }
            post.MediaUrls.Clear();
            
            foreach (var file in media)
            {
                var mediaUrl = await _mediaService.UploadMediaAsync(file);
                post.MediaUrls.Add(mediaUrl);
            }
        }

        await _postService.UpdatePostAsync(post);
        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost(
        [FromForm] string content,
        [FromForm] IFormFile[]? media = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        var post = new Post
        {
            Content = content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (media != null && media.Length > 0)
        {
            foreach (var file in media)
            {
                var mediaUrl = await _mediaService.UploadMediaAsync(file);
                post.MediaUrls.Add(mediaUrl);
            }
        }

        await _postService.CreatePostAsync(post);
        return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (post.UserId != userId)
            return Forbid();

        foreach (var mediaUrl in post.MediaUrls)
        {
            await _mediaService.DeleteMediaAsync(mediaUrl);
        }

        await _postService.DeletePostAsync(id);
        return NoContent();
    }


}
