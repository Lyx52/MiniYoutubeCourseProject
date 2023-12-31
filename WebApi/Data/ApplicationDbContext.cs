﻿using Domain.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<ContentSource> Sources { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentImpression> CommentImpressions { get; set; }
    public DbSet<VideoImpression> VideoImpressions { get; set; }
    public DbSet<Subscriber> Subscribers { get; set; }
    public DbSet<UserNotification> Notifications { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistVideo> PlaylistVideos { get; set; }
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
        
        builder.Entity<Comment>()
            .HasOne<Video>(c => c.Video)
            .WithMany(v => v.Comments)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<Video>()
            .HasMany<Comment>(v => v.Comments)
            .WithOne(c => c.Video)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<CommentImpression>()
            .HasOne<Comment>(c => c.Comment)
            .WithMany(v => v.Impressions)
            .HasForeignKey(c => c.CommentId);
        
        builder.Entity<Comment>()
            .HasMany<CommentImpression>(c => c.Impressions)
            .WithOne(c => c.Comment)
            .HasForeignKey(c => c.CommentId);
       
        
        builder.Entity<VideoImpression>()
            .HasOne<Video>(c => c.Video)
            .WithMany(v => v.Impressions)
            .HasForeignKey(c => c.VideoId);
        
        builder.Entity<Video>()
            .HasMany<VideoImpression>(c => c.Impressions)
            .WithOne(c => c.Video)
            .HasForeignKey(c => c.VideoId);

        builder.Entity<PlaylistVideo>()
            .HasKey(pv =>  new { pv.PlaylistId, pv.VideoId });

        builder.Entity<PlaylistVideo>()
            .HasOne<Video>(pv => pv.Video)
            .WithMany(v => v.PlaylistsVideos)
            .HasForeignKey(pv => pv.VideoId);
        
        builder.Entity<PlaylistVideo>()
            .HasOne<Playlist>(pv => pv.Playlist)
            .WithMany(v => v.PlaylistsVideos)
            .HasForeignKey(pv => pv.PlaylistId);
        base.OnModelCreating(builder);
    }
}