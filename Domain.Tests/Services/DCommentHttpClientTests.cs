using Domain.Constants;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.View;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Domain.Tests.Services;

[Collection("Auth Tests")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class DCommentHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<CommentHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILoginManager> _mockLoginManager;
    private readonly CommentHttpClient _commentHttpClient;
    private readonly SharedAuthState _sharedState;
    public DCommentHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
    {
        _sharedState = sharedState;
        _testOutputHelper = testOutputHelper;
        _logger = LoggerFactory.Create(builder =>
        {
        }).CreateLogger<CommentHttpClient>();

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLoginManager = new Mock<ILoginManager>();
        _mockLoginManager.Setup((x) => x.GetJwtToken(CancellationToken.None)).Returns(() => 
            Task.FromResult(_sharedState.Token)!);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:4200");
            return client;
        });
        _commentHttpClient = new CommentHttpClient(_logger, _mockHttpClientFactory.Object, _mockLoginManager.Object);
    }
    [Fact, Priority(0)]
    public async Task CreateCommentTest()
    {
        var response = await _commentHttpClient.CreateComment(_sharedState.VideoId, "VeryLongCommentMessage");
        Assert.True(response.Success, $"Test failed {response.Message}");
    }
    
    [Fact, Priority(10)]
    public async Task GetVideoCommentsTest()
    {
        var response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        var user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        var comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserId, user.Id);
        Assert.Equivalent(comment.UserId, _sharedState.UserId);
        Assert.Equivalent(comment.Message, "VeryLongCommentMessage");
        Assert.Equivalent(comment.VideoId, _sharedState.VideoId.ToString());
    }
    
    [Fact, Priority(20)]
    public async Task AddCommentImpressionTest()
    {
        var response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        var user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        var comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserImpression, ImpressionType.None);
        Assert.Equivalent(comment.Likes, 0L);
        Assert.Equivalent(comment.Dislikes, 0L);
        
        // Like impression
        var impressionResponse = await _commentHttpClient.AddCommentImpression(Guid.Parse(comment.Id), ImpressionType.Like);
        Assert.True(impressionResponse.Success, $"Test failed {impressionResponse.Message}");
        
        response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserImpression, ImpressionType.Like);
        Assert.Equivalent(comment.Likes, 1L);
        Assert.Equivalent(comment.Dislikes, 0L);
        
        // None impression
        impressionResponse = await _commentHttpClient.AddCommentImpression(Guid.Parse(comment.Id), ImpressionType.None);
        Assert.True(impressionResponse.Success, $"Test failed {impressionResponse.Message}");
        
        response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserImpression, ImpressionType.None);
        Assert.Equivalent(comment.Likes, 0L);
        Assert.Equivalent(comment.Dislikes, 0L);
        
        // Dislike impression
        impressionResponse = await _commentHttpClient.AddCommentImpression(Guid.Parse(comment.Id), ImpressionType.Dislike);
        Assert.True(impressionResponse.Success, $"Test failed {impressionResponse.Message}");
        
        response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserImpression, ImpressionType.Dislike);
        Assert.Equivalent(comment.Likes, 0L);
        Assert.Equivalent(comment.Dislikes, 1L);
        
        // None impression
        impressionResponse = await _commentHttpClient.AddCommentImpression(Guid.Parse(comment.Id), ImpressionType.None);
        Assert.True(impressionResponse.Success, $"Test failed {impressionResponse.Message}");
        
        response = await _commentHttpClient.GetVideoComments(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Comments);
        Assert.NotEmpty(response.Users);
        user = response.Users.FirstOrDefault();
        Assert.NotNull(user);
        comment = response.Comments.FirstOrDefault();
        Assert.NotNull(comment);
        Assert.Equivalent(comment.UserImpression, ImpressionType.None);
        Assert.Equivalent(comment.Likes, 0L);
        Assert.Equivalent(comment.Dislikes, 0L);
    }
}