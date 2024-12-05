using LinkUp.Interfaces;
using LinkUp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LinkUp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;

        public CommentController(ICommentService commentService, IPostService postService)
        {
            _commentService = commentService;
            _postService = postService;
        }
        
        /// <summary>
        /// Récupérer les commentaires d'un post par son id
        /// </summary>
        /// <returns>Commentaires récupérés du post par son id</returns>
        /// <response code="200">Retourne les commentaires du post</response>
        /// <response code="401">Si l'utilisateur n'est pas connecté</response>
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(Guid postId)
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId);
            
            if (comments == null || !comments.Any())
            {
                return NotFound("No comments found.");
            }
            
            return Ok(comments);
        }
        
        /// <summary>
        /// Création d'un commentaire pour un post par son id
        /// </summary>
        /// <returns>Commentaire crée pour un post par son id</returns>
        /// <response code="200">Retourne le commentaires crée pour le post</response>
        /// <response code="401">Si l'utilisateur n'est pas connecté</response>
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(Guid postId, [FromForm] string content)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Comment content cannot be empty.");
            }

            var comment = new Comment
            {
                Content = content,
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentService.CreateCommentAsync(comment);
            return CreatedAtAction(nameof(GetComments), new { postId }, createdComment);
        }

/// <summary>
/// Mise à jour d'un commentaire par son id.
/// </summary>
/// <param name="commentId">L'id du commentaire à modifier.</param>
/// <param name="content">Le nouveau contenu du commentaire.</param>
/// <returns>Le commentaire mis à jour.</returns>
/// <response code="200">Retourne le commentaire mis à jour.</response>
/// <response code="401">Si l'utilisateur n'est pas connecté.</response>
/// <response code="403">Si l'utilisateur n'est pas le propriétaire du commentaire.</response>
/// <response code="404">Si le commentaire n'est pas trouvé.</response>
[HttpPut("comments/{commentId}")]
public async Task<IActionResult> UpdateComment(Guid commentId, [FromForm] string content)
{
    var comment = await _commentService.GetCommentByIdAsync(commentId);

    if (comment == null)
        return NotFound("Comment not found.");

    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (comment.UserId != userId)
        return Forbid("You do not have permission to modify this comment.");

    if (string.IsNullOrWhiteSpace(content))
        return BadRequest("Content cannot be empty.");

    comment.Content = content;
    comment.UpdatedAt = DateTime.UtcNow;

    await _commentService.UpdateCommentAsync(comment);

    return Ok(comment);
}

        
        /// <summary>
        /// Suppression d'un commentaire pour un post par son id
        /// </summary>
        /// <returns>Commentaire supprimé pour un post par son id</returns>
        /// <response code="204">Retourne le commentaire supprimé pour le post</response>
        /// <response code="401">Si l'utilisateur n'est pas connecté</response>
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            var comment = await _commentService.GetCommentByIdAsync(commentId);

            if (comment == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (comment.UserId != userId)
                return Forbid();

            await _commentService.DeleteCommentAsync(commentId);
            return NoContent();
        }
    }
}
