using LinkUp.Models.Auth;

namespace LinkUp.Models;
public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid PostId { get; set; }
        public Post Post { get; set; }
    }