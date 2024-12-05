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

    /// <summary>
    /// Récupérer tous les posts de l'utilisateur connecté
    /// </summary>
    /// <returns>Une liste de tous les posts de l'utilisateur connecté</returns>
    /// <response code="200">Retourne la liste des posts de l'utilisateur connecté</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
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

    /// <summary>
    /// Récupérer tous les posts des utilisateurs ayant un profil public
    /// </summary>
    /// <returns>Une liste de tous les posts des utilisateurs ayant un profil public</returns>
    /// <response code="200">Retourne la liste des posts des utilisateurs ayant un profil public</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
    [HttpGet]
    public async Task<IActionResult> GetVisiblePosts()
    {
        var posts = await _postService.GetVisiblePostsAsync();
        return Ok(posts);
    }

    /// <summary>
    /// Récupérer un post par son id
    /// </summary>
    /// <returns>Post récupéré par son id</returns>
    /// <response code="200">Retourne le post</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPost(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(post);
    }

    /// <summary>
    /// Modifier un post par son id
    /// </summary>
    /// <returns>Post modifié par son id</returns>
    /// <response code="200">Retourne le post modifié</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
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

    /// <summary>
    /// Création d'un post
    /// </summary>
    /// <returns>Post crée</returns>
    /// <response code="200">Retourne le post crée</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
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

    /// <summary>
    /// Supprimer un post par son id
    /// </summary>
    /// <returns>Post supprimé par son id</returns>
    /// <response code="200">Retourne le post supprimé</response>
    /// <response code="401">Si l'utilisateur n'est pas connecté</response>
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
