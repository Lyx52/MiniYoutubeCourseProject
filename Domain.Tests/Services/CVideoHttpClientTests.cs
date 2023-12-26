using System.Text.Json;
using Domain.Constants;
using Domain.Entity;
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
public class CVideoHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<VideoHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILoginManager> _mockLoginManager;
    private readonly VideoHttpClient _videoHttpClient;
    private readonly SharedAuthState _sharedState;
    public CVideoHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
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
        Assert.False(video.IsUnlisted, "Video must be listed");
        
        var visibilityResponse = await _videoHttpClient.ChangeVideoVisibility(_sharedState.VideoId, true);
        Assert.True(visibilityResponse.Success, $"Test failed {visibilityResponse.Message}");
        
        response = await _videoHttpClient.GetUserVideos(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.Id, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        Assert.True(video.IsUnlisted, "Video must be unlisted");
        
        visibilityResponse = await _videoHttpClient.ChangeVideoVisibility(_sharedState.VideoId, false);
        Assert.True(visibilityResponse.Success, $"Test failed {visibilityResponse.Message}");
        
        response = await _videoHttpClient.GetUserVideos(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.Id, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        Assert.False(video.IsUnlisted, "Video must be listed");
    }
    
    [Fact, Priority(50)]
    public async Task GetVideoPlaylistTest()
    {
        var response = await _videoHttpClient.GetVideoPlaylist(0, 1);
        Assert.True(response.Success, $"Test failed {response.Message}");
        var video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
        Assert.Equivalent(video.VideoId, _sharedState.VideoId.ToString());
        Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        Assert.NotNull(video.Poster);
        Assert.NotNull(video.PosterGif);
    }
    
    [Fact, Priority(60)]
    public async Task GetVideoMetadataTest()
    {
        var response = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotNull(response.Metadata);
        Assert.Equivalent(response.Metadata.CreatorId.ToString(), _sharedState.UserId);
        Assert.Equivalent(response.Metadata.VideoId, _sharedState.VideoId);
        Assert.Equivalent(response.Metadata.Description, "LongDescription123");
        Assert.Equivalent(response.Metadata.Title, "LongTitle123fff");
        Assert.NotEmpty(response.Metadata.ContentSources);
        Assert.NotEmpty(response.Metadata.ContentSources.Where(c => c.Type == ContentSourceType.Video));
        Assert.NotNull(response.Metadata.ContentSources.FirstOrDefault(c => c.Type == ContentSourceType.Thumbnail));
        Assert.NotNull(response.Metadata.ContentSources.FirstOrDefault(c => c.Type == ContentSourceType.ThumbnailGif));
    }
    
    [Fact, Priority(60)]
    public async Task SetVideoImpressionTest()
    {
        // Must be None
        var metadataResponse = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(metadataResponse.Success, $"Test failed {metadataResponse.Message}");
        Assert.Equivalent(metadataResponse.Metadata.UserImpression, ImpressionType.None);
        Assert.Equivalent(metadataResponse.Metadata.VideoId, _sharedState.VideoId);
        
        // Must be Like
        var response = await _videoHttpClient.AddVideoImpression(_sharedState.VideoId, ImpressionType.Like);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        metadataResponse = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(metadataResponse.Success, $"Test failed {metadataResponse.Message}");
        Assert.Equivalent(metadataResponse.Metadata.UserImpression, ImpressionType.Like);
        Assert.Equivalent(metadataResponse.Metadata.VideoId, _sharedState.VideoId);
        
        // Must be None
        response = await _videoHttpClient.AddVideoImpression(_sharedState.VideoId, ImpressionType.None);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        metadataResponse = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(metadataResponse.Success, $"Test failed {metadataResponse.Message}");
        Assert.Equivalent(metadataResponse.Metadata.UserImpression, ImpressionType.None);
        Assert.Equivalent(metadataResponse.Metadata.VideoId, _sharedState.VideoId);
        
        // Must be Dislike
        response = await _videoHttpClient.AddVideoImpression(_sharedState.VideoId, ImpressionType.Dislike);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        metadataResponse = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(metadataResponse.Success, $"Test failed {metadataResponse.Message}");
        Assert.Equivalent(metadataResponse.Metadata.UserImpression, ImpressionType.Dislike);
        Assert.Equivalent(metadataResponse.Metadata.VideoId, _sharedState.VideoId);
        
        // Must be None
        response = await _videoHttpClient.AddVideoImpression(_sharedState.VideoId, ImpressionType.None);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        metadataResponse = await _videoHttpClient.GetVideoMetadata(_sharedState.VideoId);
        Assert.True(metadataResponse.Success, $"Test failed {metadataResponse.Message}");
        Assert.Equivalent(metadataResponse.Metadata.UserImpression, ImpressionType.None);
        Assert.Equivalent(metadataResponse.Metadata.VideoId, _sharedState.VideoId);
    }

    [Fact, Priority(70)]
    public async Task GetVideosByTitleTest()
    {
        var response = await _videoHttpClient.GetVideosByTitle("123", 0, 999);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Videos);
        var video = response.Videos.FirstOrDefault();
        Assert.NotNull(video);
    }

    [Fact, Priority(80)]
    public async Task UpdateVideoTest()
    {
        var response = await _videoHttpClient.UpdateVideo(new EditVideoMetadataModel()
        {
            Description = "LongDescription123",
            IsUnlisted = false,
            Title = "LongTitle321AAA",
            WorkSpaceId = _sharedState.WorkSpaceId,
            VideoId = _sharedState.VideoId
        });
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.Equal(response.VideoId, _sharedState.VideoId);
        
        var videoByTitleResponse = await _videoHttpClient.GetVideosByTitle("321AAA", 0, 999);
        Assert.True(videoByTitleResponse.Success, $"Test failed {videoByTitleResponse.Message}");
        Assert.NotEmpty(videoByTitleResponse.Videos);
        var video = videoByTitleResponse.Videos.FirstOrDefault();
        Assert.NotNull(video);
    }
    
    [Fact, Priority(90)]
    public async Task DeleteUserVideosTest()
    {
        
        // var response = await _videoHttpClient.GetUserVideos(0, 9999);
        // Assert.True(response.Success, $"Test failed {response.Message}");
        // Assert.NotEmpty(response.Videos);
        // foreach (var video in response.Videos)
        // {
        //     Assert.NotNull(video);
        //     Assert.Equivalent(video.CreatorId, _sharedState.UserId);
        //     var deleteResponse = await _videoHttpClient.DeleteVideo(Guid.Parse(video.Id));
        //     Assert.True(deleteResponse.Success, $"Test failed {deleteResponse.Message}");
        // }
        //
        // // TODO: This is dumb, there must be a better way
        // await Task.Delay(5000);
        // response = await _videoHttpClient.GetUserVideos(0, 9999);
        // Assert.True(response.Success, $"Test failed {response.Message}");
        // Assert.Empty(response.Videos);
    }
}