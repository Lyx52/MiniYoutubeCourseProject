using Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<ContentSource> Sources { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ContentSource>()
            .HasOne<Video>(c => c.Video)
            .WithMany(v => v.Sources)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<Video>()
            .HasMany<ContentSource>(v => v.Sources)
            .WithOne(c => c.Video)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<Video>()
            .HasOne<User>(v => v.Creator)
            .WithMany(u => u.Videos)
            .HasForeignKey(v => v.CreatorId);
        

        
        base.OnModelCreating(builder);
    }
}