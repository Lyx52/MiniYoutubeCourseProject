using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain.Constants;
using Domain.Entity;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.Query;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class VideoHttpClient(
    ILogger<VideoHttpClient> logger,
    IHttpClientFactory httpClientFactory,
    ILoginManager loginManager)
    : BaseHttpClient(nameof(VideoHttpClient), logger, httpClientFactory, loginManager), IVideoHttpClient
{

    public Task<CreateOrUpdateVideoResponse> CreateVideo(EditVideoMetadataModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<CreateVideoRequest, CreateOrUpdateVideoResponse>("api/Video/CreateVideo", new CreateVideoRequest()
        {
            Description = model.Description,
            Title = model.Title,
            WorkSpaceId = model.WorkSpaceId,
            IsUnlisted = model.IsUnlisted
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<Response> PublishVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<PublishVideoRequest, Response>("api/Video/PublishVideo", new PublishVideoRequest()
        {
            VideoId = videoId
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<VideoStatusResponse> GetProcessingStatus(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "videoId", videoId.ToString() } };
        return SendQueryRequest<VideoStatusResponse>(HttpMethod.Get, "api/Video/Status", qb.ToQueryString(), 
            JwtRequirement.Mandatory, cancellationToken);
    }
    public Task<Response> DeleteVideo(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "videoId", videoId.ToString() } };
        return SendQueryRequest<Response>(HttpMethod.Delete, "api/Video/Delete", qb.ToQueryString(), 
            JwtRequirement.Mandatory, cancellationToken);
    }
    public Task<QueryVideosResponse> GetVideosByTitle(string searchText, int from, int count, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<QueryVideosRequest, QueryVideosResponse>("api/Video/Query", new QueryVideosRequest()
        {
            Title = searchText,
            From = from,
            Count = count,
            Status = VideoProcessingStatus.Published 
        }, JwtRequirement.Optional, cancellationToken);
    }

    public Task<VideoPlaylistResponse> GetVideoPlaylist(GetVideoPlaylistModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<VideoPlaylistRequest, VideoPlaylistResponse>("api/Video/Playlist", new VideoPlaylistRequest()
        {
            From = model.From,
            Count = model.Count,
            CreatorId = model.CreatorId,
            OrderByNewest = model.OrderByNewest,
            PlaylistId = model.PlaylistId
        }, JwtRequirement.Optional, cancellationToken);
    }

    
    public Task<QueryVideosResponse> GetUserVideos(int from, int count, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<QueryVideosRequest, QueryVideosResponse>("api/Video/Query", new QueryVideosRequest()
        {
            QueryUserVideos = true,
            From = from,
            Count = count,
            Status = null
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<Response> AddVideosToPlaylist(IEnumerable<Guid> videos, Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<AddOrRemovePlaylistRequest, Response>("api/Video/AddOrRemovePlaylist", new AddOrRemovePlaylistRequest()
        {
            PlaylistId = playlistId,
            Videos = videos,
            AddVideo = true
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<Response> RemoveVideosFromPlaylist(IEnumerable<Guid> videos, Guid playlistId, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<AddOrRemovePlaylistRequest, Response>("api/Video/AddOrRemovePlaylist", new AddOrRemovePlaylistRequest()
        {
            PlaylistId = playlistId,
            Videos = videos,
            AddVideo = false
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<VideoMetadataResponse> GetVideoMetadata(Guid videoId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "videoId", videoId.ToString() } };
        return SendQueryRequest<VideoMetadataResponse>(HttpMethod.Get, "api/Video/Metadata", qb.ToQueryString(),
            JwtRequirement.Optional, cancellationToken);
    }
    
    public Task<Response> AddVideoImpression(Guid videoId, ImpressionType impressionType, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<VideoImpressionRequest, Response>("api/Video/Impression", new VideoImpressionRequest()
        {
            VideoId = videoId,
            Impression = impressionType
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<Response> ChangeVideoVisibility(Guid videoId, bool isPrivate,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<ChangeVideoVisibilityRequest, Response>("api/Video/ChangeVisibility", new ChangeVideoVisibilityRequest()
        {
            VideoId = videoId,
            IsUnlisted = isPrivate
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<CreateOrUpdateVideoResponse> UpdateVideo(EditVideoMetadataModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<UpdateVideoRequest, CreateOrUpdateVideoResponse>("api/Video/UpdateVideo", new UpdateVideoRequest()
        {
            VideoId = model.VideoId!.Value,
            Description = model.Description,
            Title = model.Title,
            IsUnlisted = model.IsUnlisted
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    
    public Task<CreatePlaylistResponse> CreatePlaylist(CreatePlaylistModel model, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<CreatePlaylistRequest, CreatePlaylistResponse>("api/Video/CreatePlaylist", new CreatePlaylistRequest()
        {
            Title = model.Title,
            Videos = model.Videos
        }, JwtRequirement.Mandatory, cancellationToken);
    }
    public Task<CreatorPlaylistsResponse> GetCreatorPlaylists(Guid creatorId, CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "creatorId", creatorId.ToString() } };
        return SendQueryRequest<CreatorPlaylistsResponse>(HttpMethod.Get, "api/Video/CreatorPlaylists", qb.ToQueryString(),
            JwtRequirement.Optional, cancellationToken);
    }
}