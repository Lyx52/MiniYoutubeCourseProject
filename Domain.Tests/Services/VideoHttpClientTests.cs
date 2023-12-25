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
public class VideoHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<VideoHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILoginManager> _mockLoginManager;
    private readonly VideoHttpClient _videoHttpClient;
    private readonly SharedAuthState _sharedState;
    public VideoHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
    {
        _sharedState = sharedState;
        _testOutputHelper = testOutputHelper;
        _logger = LoggerFactory.Create(builder =>
        {
        }).CreateLogger<VideoHttpClient>();

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
        _videoHttpClient = new VideoHttpClient(_logger, _mockHttpClientFactory.Object, _mockLoginManager.Object);
    }
    
    [Fact, Priority(-10)]
    public async Task CreateVideoTest()
    {
        var model = new EditVideoMetadataModel()
        {
            Description = "LongDescription123",
            IsUnlisted = false,
            Title = "LongTitle123fff",
            WorkSpaceId = _sharedState.WorkSpaceId,
            VideoId = null
        };
        var response = await _videoHttpClient.CreateVideo(model);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.True(response.VideoId != Guid.Empty, "Bad guid generated");
        _sharedState.VideoId = response.VideoId;
    }
    
    [Fact, Priority(0)]
    public async Task GetVideoStatusTest()
    {
        var response = await _videoHttpClient.GetProcessingStatus(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.True(response.VideoId != Guid.Empty, "Bad guid generated");
        Assert.True(response.Status is VideoProcessingStatus.Processing or VideoProcessingStatus.ProcessingFinished, "Invalid video state");
    }
    
    [Fact, Priority(10)]
    public async Task WaitForProcessingFinishedTest()
    {
        DateTime started = DateTime.UtcNow;
        var status = VideoProcessingStatus.Processing;
        while (status is VideoProcessingStatus.Processing && (DateTime.UtcNow - started).TotalSeconds < 120)
        {
            var response = await _videoHttpClient.GetProcessingStatus(_sharedState.VideoId);
            status = response.Status;
        }

        Assert.False(status is VideoProcessingStatus.ProcessingFailed, "status is VideoProcessingStatus.ProcessingFailed");
        Assert.False(status is VideoProcessingStatus.Processing, "status is VideoProcessingStatus.Processing");
        Assert.True(status is VideoProcessingStatus.ProcessingFinished, "status is not VideoProcessingStatus.ProcessingFinished");
    }
    
    [Fact, Priority(20)]
    public async Task PublishVideoTest()
    {
        var response = await _videoHttpClient.PublishVideo(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        await Task.Delay(2500);
        var statusResponse = await _videoHttpClient.GetProcessingStatus(_sharedState.VideoId);
        Assert.True(statusResponse.Success, $"Test failed {statusResponse.Message}");
        Assert.Equivalent(statusResponse.VideoId, _sharedState.VideoId);
        Assert.True(statusResponse.Status is VideoProcessingStatus.Published, "status is not VideoProcessingStatus.Published");
    }
    
    [Fact, Priority(30)]
    public async Task GetUserVideoTest()
    {
        var response = await _videoHttpClient.GetUserVideos(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        var video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.Id, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
    }
    
    [Fact, Priority(40)]
    public async Task ChangeVideoVisibilityTest()
    {
        var response = await _videoHttpClient.GetUserVideos(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        var video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.Id, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        Assert.False(video.IsUnlisted, "Video must be unlisted");
        
        var visibilityResponse = await _videoHttpClient.ChangeVideoVisibility(_sharedState.VideoId, true);
        Assert.True(visibilityResponse.Success, $"Test failed {visibilityResponse.Message}");
        
        response = await _videoHttpClient.GetUserVideos(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.Id, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        Assert.True(video.IsUnlisted, "Video must be listed");
    }
    
    [Fact, Priority(50)]
    public async Task DeleteUserVideosTest()
    {
        var response = await _videoHttpClient.GetUserVideos(0, 9999);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Videos);
        foreach (var video in response.Videos)
        {
            Assert.NotNull(video);
            Assert.Equivalent(video.CreatorId, _sharedState.UserId);
            var deleteResponse = await _videoHttpClient.DeleteVideo(Guid.Parse(video.Id));
            Assert.True(deleteResponse.Success, $"Test failed {deleteResponse.Message}");
        }

        await Task.Delay(5000);
        response = await _videoHttpClient.GetUserVideos(0, 9999);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.Empty(response.Videos);
    }
}