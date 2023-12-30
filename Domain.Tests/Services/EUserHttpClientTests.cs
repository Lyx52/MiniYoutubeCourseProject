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
public class EUserHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<UserHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILoginManager> _mockLoginManager;
    private readonly UserHttpClient _userHttpClient;
    private readonly ContentHttpClient _contentHttpClient;
    private readonly VideoHttpClient _videoHttpClient;
    private readonly SharedAuthState _sharedState;
    public EUserHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
    {
        _sharedState = sharedState;
        _testOutputHelper = testOutputHelper;
        _logger = LoggerFactory.Create(builder =>
        {
        }).CreateLogger<UserHttpClient>();

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
        _userHttpClient = new UserHttpClient(_logger, _mockHttpClientFactory.Object, _mockLoginManager.Object);
    }
    
    [Fact, Priority(0)]
    public async Task GetUserProfileTest()
    {
        var response = await _userHttpClient.GetUserProfile();
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.Equivalent(response.User.Username, "lyx52");
        Assert.Equivalent(response.User.CreatorName, "lyx52");
        Assert.Equivalent(response.User.Email, "e@e.c");
        Assert.Equivalent(response.User.Id, _sharedState.UserId);
    }
    
    [Fact, Priority(10)]
    public async Task GetPublicUserProfileTest()
    {
        var response = await _userHttpClient.GetPublicUserProfile(Guid.Parse(_sharedState.UserId));
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.Equivalent(response.User.Username, string.Empty);
        Assert.Equivalent(response.User.CreatorName, "lyx52");
        Assert.Equivalent(response.User.Email, string.Empty);
        Assert.Equivalent(response.User.Id, _sharedState.UserId);
    }
    
    [Fact, Priority(20)]
    public async Task GetCreatorProfileTest()
    {
        var response = await _userHttpClient.GetCreatorProfile(Guid.Parse(_sharedState.UserId));
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotNull(response.Creator);
        var creator = response.Creator;
        Assert.Equivalent(creator.Username, string.Empty);
        Assert.Equivalent(creator.CreatorName, "lyx52");
        Assert.Equivalent(creator.Email, string.Empty);
        Assert.Equivalent(creator.Id, _sharedState.UserId);
        
        Assert.Equivalent(response.IsSubscribed, true);
        Assert.Equivalent(response.SubscriberCount, 1L);
    }
    
    [Fact, Priority(30)]
    public async Task SubscribeTest()
    {
        var response = await _userHttpClient.GetCreatorProfile(Guid.Parse(_sharedState.UserId));
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotNull(response.Creator);
        Assert.Equivalent(response.IsSubscribed, true);
        Assert.Equivalent(response.SubscriberCount, 1L);
        
        // Unsubscribe
        var unsubscribeResponse = await _userHttpClient.Unsubscribe(Guid.Parse(_sharedState.UserId));
        Assert.True(unsubscribeResponse.Success, $"Test failed {unsubscribeResponse.Message}");
        
        response = await _userHttpClient.GetCreatorProfile(Guid.Parse(_sharedState.UserId));
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotNull(response.Creator);
        Assert.Equivalent(response.IsSubscribed, false);
        Assert.Equivalent(response.SubscriberCount, 0L);
        
        // Subscribe
        var subscribeResponse = await _userHttpClient.Subscribe(Guid.Parse(_sharedState.UserId));
        Assert.True(subscribeResponse.Success, $"Test failed {subscribeResponse.Message}");
        
        response = await _userHttpClient.GetCreatorProfile(Guid.Parse(_sharedState.UserId));
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotNull(response.Creator);
        Assert.Equivalent(response.IsSubscribed, true);
        Assert.Equivalent(response.SubscriberCount, 1L);
    }
    
    [Fact, Priority(40)]
    public async Task NotificationTest()
    {
        var response = await _userHttpClient.GetUserNotifications();
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.NotEmpty(response.Notifications);
        Assert.Equivalent(response.UserId, _sharedState.UserId);
        var notification = response.Notifications.FirstOrDefault();
        Assert.NotNull(notification);
        Assert.Equivalent(notification.UserId, _sharedState.UserId);
        
        // Dismiss
        await Task.Delay(5000); // TODO: Stupid fix
        var dismissResponse = await _userHttpClient.DismissUserNotifications(Guid.Parse(notification.Id));
        Assert.True(dismissResponse.Success, $"Test failed {dismissResponse.Message}");
        
        response = await _userHttpClient.GetUserNotifications();
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.Empty(response.Notifications);
    }
}