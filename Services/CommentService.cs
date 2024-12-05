using LinkUp.Data;
using LinkUp.Models;
using LinkUp.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Services;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;

    public CommentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId)
    {
        return await _context.Comments
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Comment> GetCommentByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .Where(c => c.Id == commentId)
            .FirstOrDefaultAsync();
    }

    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }
    
    public async Task UpdateCommentAsync(Comment comment)
    {
        var existingComment = await _context.Comments.FindAsync(comment.Id);
        if (existingComment != null)
        {
            existingComment.Content = comment.Content;
            existingComment.UpdatedAt = comment.UpdatedAt;

            _context.Comments.Update(existingComment);
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("Comment not found.");
        }
    }

    public async Task DeleteCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}