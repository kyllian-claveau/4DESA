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
