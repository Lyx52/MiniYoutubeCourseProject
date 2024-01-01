using Domain;
using Domain.Model.Configuration;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Client.Pages;
using WebApp.Components;
using WebApp.Services;

namespace WebApp;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var settings = services.AddAppConfiguration(Configuration);
        services.AddSingleton<AppConfiguration>(
           _ => Configuration.GetSection(nameof(AppConfiguration)).Get<AppConfiguration>()!
        );
        services.AddJwtAuthentication(settings);
        services.AddContentClient(settings);
        services.AddVideoClient(settings);
        services.AddCommentClient(settings);
        services.AddUserClient(settings);
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
    		.AddInteractiveWebAssemblyComponents();
        services.AddBlazorBootstrap();
        services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
        
        services.AddCascadingAuthenticationState();
        services.AddAuthenticationCore();
        services.AddAuthorizationCore();
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        } else {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseRouting();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.UseEndpoints((config) =>
        {
            config.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
    			.AddInteractiveWebAssemblyRenderMode()
    			.AddAdditionalAssemblies(typeof(Counter).Assembly);
        });
    }
}