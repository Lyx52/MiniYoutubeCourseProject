using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
public class AAuthHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<AuthHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly AuthHttpClient _authHttpClient;
    private readonly UserHttpClient _userHttpClient;
    private readonly SharedAuthState _sharedState;
    private readonly Mock<ILoginManager> _mockLoginManager;
    public AAuthHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
    {
        _sharedState = sharedState;
        _testOutputHelper = testOutputHelper;
        _logger = LoggerFactory.Create(builder =>
        {
        }).CreateLogger<AuthHttpClient>();

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(() =>
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:4200");
            return client;
        });
        _authHttpClient = new AuthHttpClient(_logger, _mockHttpClientFactory.Object);
        
        _mockLoginManager = new Mock<ILoginManager>();
        _mockLoginManager.Setup((x) => x.GetJwtToken(CancellationToken.None)).Returns(() => 
            Task.FromResult(_sharedState.Token)!);
        
        _mockLoginManager.Setup(x => x.SetJwtToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((t, ct) => _sharedState.Token = t)
            .Returns(Task.CompletedTask);
        _mockLoginManager.Setup(x => x.SetJwtToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((t, ct) => _sharedState.Token = t)
            .Returns(Task.CompletedTask);
        
        _mockLoginManager.Setup(x => x.SetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((t, ct) => _sharedState.RefreshToken = t)
            .Returns(Task.CompletedTask);
        
        _mockLoginManager.Setup((x) => x.GetRefreshToken(CancellationToken.None)).Returns(() => 
            Task.FromResult(_sharedState.RefreshToken)!);
        _userHttpClient = new UserHttpClient(LoggerFactory.Create(b => {}).CreateLogger<UserHttpClient>(), 
            _mockHttpClientFactory.Object, _mockLoginManager.Object);
    }
    
    [Fact, Priority(0)]
    public async Task RegisterTest()
    {
        var registerModel = new RegisterModel { Username = "lyx52", Password = "Parole123$", Email = "e@e.c"};
        var response = await _authHttpClient.RegisterAsync(registerModel);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        registerModel = new RegisterModel { Username = "lyx52", Password = "Parole123$", Email = "e@f.c"};
        response = await _authHttpClient.RegisterAsync(registerModel);
        Assert.False(response.Success, $"Test failed {response.Message}");
        Assert.Equivalent(response.Message, "User with this username already exists!");
        
        registerModel = new RegisterModel { Username = "lyx53", Password = "Parole123$", Email = "e@e.c"};
        response = await _authHttpClient.RegisterAsync(registerModel);
        Assert.False(response.Success, $"Test failed {response.Message}");
        Assert.Equivalent(response.Message, "User with this email already exists!");
    }
    
    [Fact, Priority(10)]
    public async Task LoginTest()
    {
        var loginModel = new LoginModel { Username = "lyx52", Password = "Parole123$"};
        var response = await _authHttpClient.LoginAsync(loginModel);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.Token), $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.BearerToken), $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.RefreshToken), $"Test failed {response.Message}");
        _sharedState.BearerToken = response.BearerToken;
        _sharedState.Token = response.Token;
        _sharedState.RefreshToken = response.RefreshToken;
        var handler = new JwtSecurityTokenHandler();
        var securityToken = handler.ReadJwtToken(_sharedState.Token);
        Assert.NotNull(securityToken);
        var userId = securityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.False(string.IsNullOrEmpty(userId), "userId is empty");
        _sharedState.UserId = userId;
    }
    
    [Fact, Priority(20)]
    public async Task RefreshTokenTest()
    {
        var response = await _authHttpClient.RefreshTokenAsync(_mockLoginManager.Object);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.Token), $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.BearerToken), $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.RefreshToken), $"Test failed {response.Message}");
        Assert.False(_sharedState.RefreshToken == response.RefreshToken);
        Assert.False(_sharedState.Token == response.Token);
        _sharedState.BearerToken = response.BearerToken;
        _sharedState.Token = response.Token;
        _sharedState.RefreshToken = response.RefreshToken;
        var handler = new JwtSecurityTokenHandler();
        var securityToken = handler.ReadJwtToken(_sharedState.Token);
        Assert.NotNull(securityToken);
        var userId = securityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.False(string.IsNullOrEmpty(userId), "userId is empty");
        Assert.Equivalent(_sharedState.UserId, userId);
        _sharedState.UserId = userId;
        
        // Try subscribing using new tokens (Also needed later for notification test later on)...
        var subscribeResponse = await _userHttpClient.Subscribe(Guid.Parse(_sharedState.UserId));
        Assert.True(subscribeResponse.Success, $"Test failed {subscribeResponse.Message}");
    }
    [Fact, Priority(30)]
    public async Task RevokeTokenTest()
    {
        var response = await _authHttpClient.RevokeRefreshTokenAsync(_mockLoginManager.Object);
        Assert.True(response.Success, $"Test failed {response.Message}");
        
        // Try refreshing token (Needs to fail)
        var refreshTokenResponse = await _authHttpClient.RefreshTokenAsync(_mockLoginManager.Object);
        Assert.False(refreshTokenResponse.Success, $"Test failed {refreshTokenResponse.Message}");
        
        // Login again
        await LoginTest();
    }
}