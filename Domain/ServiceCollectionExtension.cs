using System.Net;
using System.Threading.Channels;
using Domain.Interfaces;
using Domain.Model.Configuration;
using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Domain;

public static class ServiceCollectionExtension
{
    public static void AddJwtAuthentication(this IServiceCollection services, AppConfiguration configuration)
    {
        services.AddHttpClient<IAuthHttpClient, AuthHttpClient>(nameof(AuthHttpClient), client =>
        {
            client.BaseAddress =
                new Uri(configuration.ApiEndpoint);
        })
        .AddPolicyHandler(Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), 2)));
        services.AddHttpClient();
        services.AddScoped<IAuthHttpClient, AuthHttpClient>();
        services.AddScoped<ILoginManager, LoginManagerService>();
    }

    public static void AddContentClient(this IServiceCollection services, AppConfiguration configuration)
    {
        services.AddHttpClient<IContentHttpClient, ContentHttpClient>(nameof(ContentHttpClient), client =>
            {
                client.BaseAddress = new Uri(configuration.ApiEndpoint);
            })
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), 2)));
        services.AddHttpClient();
        services.AddScoped<IContentHttpClient, ContentHttpClient>();
    }
    public static void AddVideoClient(this IServiceCollection services, AppConfiguration configuration)
    {
        services.AddHttpClient<IVideoHttpClient, VideoHttpClient>(nameof(VideoHttpClient), client =>
            {
                client.BaseAddress = new Uri(configuration.ApiEndpoint);
            })
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), 2)));
        services.AddHttpClient();
        services.AddScoped<IVideoHttpClient, VideoHttpClient>();
    }
    public static void AddCommentClient(this IServiceCollection services, AppConfiguration configuration)
    {
        services.AddHttpClient<ICommentHttpClient, CommentHttpClient>(nameof(CommentHttpClient), client =>
            {
                client.BaseAddress = new Uri(configuration.ApiEndpoint);
            })
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(500), 2)));
        services.AddHttpClient();
        services.AddScoped<ICommentHttpClient, CommentHttpClient>();
    }
    public static AppConfiguration AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppConfiguration>(configuration.GetSection(nameof(AppConfiguration)));
        var settings = configuration.GetSection(nameof(AppConfiguration)).Get<AppConfiguration>();
        if (settings is null) throw new ApplicationException("Cannot get AppConfiguration section!");
        return settings;
    }
    public static ApiConfiguration AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiConfiguration>(configuration.GetSection(nameof(ApiConfiguration)));
        var settings = configuration.GetSection(nameof(ApiConfiguration)).Get<ApiConfiguration>();
        if (settings is null) throw new ApplicationException("Cannot get ApiConfiguration section!");
        return settings;
    }

    public static IServiceCollection AddChannel<TMessageType>(this IServiceCollection services)
    {
        services.AddSingleton<Channel<TMessageType>>(Channel.CreateUnbounded<TMessageType>());
        services.AddSingleton<ChannelReader<TMessageType>>(s => s.GetRequiredService<Channel<TMessageType>>().Reader);
        services.AddSingleton<ChannelWriter<TMessageType>>(s => s.GetRequiredService<Channel<TMessageType>>().Writer);
        return services;
    }
}