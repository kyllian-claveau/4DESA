using LinkUp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LinkUp.Interfaces;

public interface ICommentService
{
    Task<Comment> GetCommentByIdAsync(Guid commentId);
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
    Task<Comment> CreateCommentAsync(Comment comment);
    
    Task UpdateCommentAsync(Comment comment);

    
    Task DeleteCommentAsync(Guid commentId);
}