using System.Net;
using Domain.Interfaces;
using Domain.Model.Configuration;
using Domain.WebClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Domain;

public static class DependencyInjection
{
    public static void AddAuthServices(this ServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfiguration>(configuration.GetSection(nameof(AppConfiguration)));
        var settings = configuration.GetSection(nameof(AppConfiguration)).Get<AppConfiguration>();
        if (settings is null) throw new ApplicationException("Cannot get AppConfiguration section!");
        services.AddHttpClient<IAuthHttpClient, AuthHttpClient>(client =>
        {
            client.BaseAddress =
                new Uri(settings.ApiEndpoint);
        }).AddPolicyHandler(Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), 5)));
        services.AddHttpClient();
    }    
}