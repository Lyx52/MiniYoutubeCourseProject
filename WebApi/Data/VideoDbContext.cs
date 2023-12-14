using Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class VideoDbContext : DbContext
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<ContentSource> Sources { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options)
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
        
        builder.Entity<Comment>()
            .HasOne<Video>(c => c.Video)
            .WithMany(v => v.Comments)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<Video>()
            .HasMany<Comment>(v => v.Comments)
            .WithOne(c => c.Video)
            .HasForeignKey(c => c.VideoId);

        base.OnModelCreating(builder);
    }
}