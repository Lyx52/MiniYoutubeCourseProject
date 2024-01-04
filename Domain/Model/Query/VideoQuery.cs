using System.Collections;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Domain.Constants;
using Domain.Entity;
using Domain.Model.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using WebApi.Services.Interfaces;

namespace Domain.Model.Query;

public class VideoQuery : IEntityQuery<Video>
{
    public string? Title { get; set; }
    public Guid? VideoId { get; set; }
    public Guid? CreatorId { get; set; }
    public Guid? PlaylistId { get; set; }
    public bool IncludeUnlisted { get; set; }
    public bool UsePaging { get; set; } = true;
    public VideoProcessingStatus? Status { get; set; } = null;
    public int From { get; set; }
    public int Count { get; set; }
    public bool OrderByCreated { get; set; }
    public bool OrderByPopularity { get; set; }
    
    public bool AddSources { get; set; }
    public bool AddComments { get; set; }
    public bool AddImpressions { get; set; }
    
    [JsonIgnore]
    public Expression<Func<Video, bool>> AsExpression
    {
        get
        {
            return (v) =>
                (string.IsNullOrEmpty(Title) || v.Title.ToLower().Contains(Title.ToLower())) &&
                (!VideoId.HasValue || v.Id == VideoId.Value.ToString()) &&
                (IncludeUnlisted || v.IsUnlisted == false) &&
                (!Status.HasValue || v.Status == Status.Value) &&
                (!CreatorId.HasValue || v.CreatorId == CreatorId.Value.ToString()) &&
                (!PlaylistId.HasValue || v.PlaylistsVideos.Any(pv => pv.PlaylistId == PlaylistId.ToString()));
        }
    }

    public IQueryable<Video> ToQueryable(IQueryable<Video>? queryable, bool asNonTracking)
    {
        if (asNonTracking)
            queryable = queryable!.AsNoTracking();
        
        if (AddSources)
            queryable = queryable!.Include(v => v.Sources);
       
        if (AddComments)
            queryable = queryable!.Include(v => v.Comments);
        
        if (AddImpressions)
            queryable = queryable!.Include(v => v.Impressions);

        if (OrderByCreated)
            queryable = queryable!.OrderBy(v => v.Created);

        if (OrderByPopularity)
            queryable = queryable!.OrderBy(v => v.ViewCount);
        
        return queryable!
            .Where(AsExpression)
            .Skip(From)
            .Take(Count)
            .OrderBy(v => v.Created);
    }
}