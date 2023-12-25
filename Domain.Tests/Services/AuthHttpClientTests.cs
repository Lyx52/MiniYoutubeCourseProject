using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Interfaces;
using Domain.Model;
using Domain.Model.View;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Domain.Tests.Services;

[Collection("Auth Tests")]
public class AuthHttpClientTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ILogger<AuthHttpClient> _logger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly AuthHttpClient _authHttpClient;
    private readonly SharedAuthState _sharedState;
    public AuthHttpClientTests(ITestOutputHelper testOutputHelper, SharedAuthState sharedState)
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

    }
    
    [Fact]
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
    
    [Fact]
    public async Task LoginTest()
    {
        var loginModel = new LoginModel { Username = "lyx52", Password = "Parole123$"};
        var response = await _authHttpClient.LoginAsync(loginModel);
        Assert.True(response.Success, $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.Token), $"Test failed {response.Message}");
        Assert.False(string.IsNullOrEmpty(response.BearerToken), $"Test failed {response.Message}");
        _sharedState.BearerToken = response.BearerToken;
        _sharedState.Token = response.Token;
        var handler = new JwtSecurityTokenHandler();
        var securityToken = handler.ReadJwtToken(_sharedState.Token);
        Assert.NotNull(securityToken);
        var userId = securityToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        Assert.False(string.IsNullOrEmpty(userId), "userId is empty");
        _sharedState.UserId = userId;
    }
}