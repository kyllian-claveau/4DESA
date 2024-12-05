using LinkUp.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace LinkUp.Models;

public class Post
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; }
    [Required]
    public string Content { get; set; }
    public List<string> MediaUrls { get; set; } = new();
    public ApplicationUser User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}