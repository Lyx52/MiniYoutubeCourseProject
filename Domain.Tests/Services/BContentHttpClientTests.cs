using Domain.Interfaces;
using Domain.Model;
using Domain.Model.View;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Domain.Tests.Services;

[Collection("Auth Tests")]
public class BContentHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<ContentHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILoginManager> _mockLoginManager;
    private readonly ContentHttpClient _contentHttpClient;
    private readonly SharedAuthState _sharedState;
    public BContentHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
    {
        _sharedState = sharedState;
        _testOutputHelper = testOutputHelper;
        _logger = LoggerFactory.Create(builder =>
        {
        }).CreateLogger<ContentHttpClient>();

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
        _contentHttpClient = new ContentHttpClient(_logger, _mockHttpClientFactory.Object, _mockLoginManager.Object);
    }
    [Fact]
    public async Task UploadVideoTest()
    {
        var data = await File.ReadAllBytesAsync("./video/elephants2.mp4");
        var ms = new MemoryStream(data, false);
        var payload = new UploadVideoModel()
        {
            FileName = "elephants2.mp4",
            FileSize = ms.Length,
            FileStream = ms
        };
        var response = await _contentHttpClient.UploadVideoFile(payload);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.True(response.FileId != Guid.Empty, "Bad guid generated");
        Assert.False(string.IsNullOrEmpty(response.FileName), "Bad filename generated");
        _sharedState.WorkSpaceId = response.FileId;
    }
}