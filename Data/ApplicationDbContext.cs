using LinkUp.Models;
using LinkUp.Models.Auth;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinkUp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /* On met nos DBSet ci-dessous */
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            /* On configure notre entité Post ci-dessous */
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
            });

            /* On configure notre entité Comment ci-dessous */
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.UpdatedAt).HasDefaultValueSql("GETDATE()");

                /* On configure les relations entre les posts */
                entity.HasOne(c => c.Post)
                      .WithMany(p => p.Comments)  /* On fais en sorte qu'un post puisse avoir plusieurs commentaires */
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);  /* Quand on supprime un post, les commentaires sont supprimés aussi */
                entity.HasOne(c => c.User)
                      .WithMany()  
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict); 
            });
        }
    }
}
