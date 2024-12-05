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

        /* On met nos DbSet ci-dessous */
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }  // Ajout du DbSet pour les abonnements

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
                      .WithMany(p => p.Comments)  /* Un post peut avoir plusieurs commentaires */
                      .HasForeignKey(c => c.PostId)
                      .OnDelete(DeleteBehavior.Cascade);  /* Si un post est supprimé, ses commentaires sont supprimés */
                entity.HasOne(c => c.User)
                      .WithMany()  
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);  /* L'utilisateur ne peut pas être supprimé si des commentaires lui appartiennent */
            });

            /* On configure l'entité Subscription ci-dessous */
            builder.Entity<Subscription>(entity =>
            {
                entity.HasKey(s => s.Id);

                /* On configure la relation entre abonnés ci-dessous */
                entity.HasOne(s => s.Subscriber)
                      .WithMany()  
                      .HasForeignKey(s => s.SubscriberId)
                      .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(s => s.Subscribed)
                      .WithMany()
                      .HasForeignKey(s => s.SubscribedId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
