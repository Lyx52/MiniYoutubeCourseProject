using System.Net;
using System.Net.Http.Json;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Model.Request;
using Domain.Model.Response;
using Domain.Model.View;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Services;

public class CommentHttpClient(
        ILogger<CommentHttpClient> logger,
        IHttpClientFactory httpClientFactory,
        ILoginManager loginManager)
        : BaseHttpClient(nameof(CommentHttpClient), logger, httpClientFactory, loginManager), ICommentHttpClient
{
    public Task<QueryCommentsResponse> GetVideoComments(Guid videoId, 
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var qb = new QueryBuilder { { "videoId", videoId.ToString() } };

        return SendQueryRequest<QueryCommentsResponse>(HttpMethod.Get, "api/Comment/Query", qb.ToQueryString(), JwtRequirement.Optional,
            cancellationToken);
    }

    public Task<Response> CreateComment(Guid videoId, string message, 
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<CreateCommentRequest, Response>("api/Comment/Create", new CreateCommentRequest()
        {
            VideoId = videoId,
            Message = message
        }, JwtRequirement.Mandatory, cancellationToken);
    }

    public Task<Response> AddCommentImpression(Guid commentId, ImpressionType impressionType, 
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return SendPayloadRequest<CommentImpressionRequest, Response>("api/Comment/Impression", new CommentImpressionRequest()
        {
            CommentId = commentId,
            Impression = impressionType
        }, JwtRequirement.Mandatory, cancellationToken);
    }
}