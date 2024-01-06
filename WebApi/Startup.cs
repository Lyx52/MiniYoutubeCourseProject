#define USE_INMEMORY
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Domain.Constants;
using Domain.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApi.Data;
using WebApi.Services;
using WebApi.Services.Interfaces;
using WebApi.Swagger;
using Domain;
using Domain.Model.Configuration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;

using WebApi.Services.Models;

namespace WebApi;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var settings = services.AddApiConfiguration(Configuration);
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        services.Configure<FormOptions>(x =>
        {
            x.MultipartBodyLengthLimit = int.MaxValue - 1;
        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<UserDbContext>((options) =>
        {
            #if USE_INMEMORY
            options.UseInMemoryDatabase("UserDb");
            #else
            options.UseNpgsql(
                    $"Host={settings.Database.Hostname};Username={(settings.Database.Username)};Password={(settings.Database.Password)};Database={(settings.Database.DatabaseName)}",
                    opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .EnableDetailedErrors(true);
            #endif
        });
        services.AddDbContext<ApplicationDbContext>((options) =>
        {
            #if USE_INMEMORY
            options.UseInMemoryDatabase("AppDb");
            #else
            options.UseNpgsql(
                    $"Host={settings.Database.Hostname};Username={(settings.Database.Username)};Password={(settings.Database.Password)};Database={(settings.Database.DatabaseName)}",
                    opt => opt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .EnableDetailedErrors(true);
            #endif
        });

        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password = PasswordOptionConfig.Default;
            })
            .AddEntityFrameworkStores<UserDbContext>()
            .AddDefaultTokenProviders();
        // Adding Authentication
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true,
                    ValidAudience = settings.JWT.ValidAudience,
                    ValidIssuer = settings.JWT.ValidIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.JWT.Secret))
                };
            });
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<FileOperationFilter>();  
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                In = ParameterLocation.Header, 
                Description = "JWT Authentication token",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey 
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                { 
                    new OpenApiSecurityScheme 
                    { 
                        Reference = new OpenApiReference 
                        { 
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer" 
                        }
                    },
                    new string[] { } 
                } 
            });
        });
        
        services.AddTransient<IWorkFileService, WorkFileService>();
        services.AddTransient<IContentProcessingService, ContentProcessingService>();
        services.AddTransient<INotificationProcessingService, NotificationProcessingService>();
        services.AddTransient<IEmailProcessingService, EmailProcessingService>();
        services.AddSingleton<IViewProcessingService, ViewProcessingService>();
        
        services.AddTransient<IContentService, ContentService>();
        
        services.AddTransient<IVideoRepository, VideoRepository>();
        services.AddTransient<IContentRepository, ContentRepository>();
        services.AddTransient<ICommentRepository, CommentRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<ISubscriberRepository, SubscriberRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IPlaylistRepository, PlaylistRepository>();
        services.AddMemoryCache();
        
        // Processing channel
        services.AddChannel<BackgroundTask>();
        
        services.AddSingleton<ApiConfiguration>(
            _ => Configuration.GetSection(nameof(ApiConfiguration)).Get<ApiConfiguration>()!
        );
        services.AddHostedService<BackgroundProcessingService>();
    }
    

    public void Configure(IApplicationBuilder app, IHostEnvironment env, ApplicationDbContext appDbContext, UserDbContext userDbContext)
    {
        Task.WhenAll(
                appDbContext.Database.EnsureCreatedAsync(), 
                userDbContext.Database.EnsureCreatedAsync())
            .GetAwaiter()
            .GetResult();
        app.UseSwagger(options =>
        {
        });
        app.UseSwaggerUI();
        app.UseStaticFiles();
        app.UseRouting();
        
        app.UseHttpsRedirection();
        
        // Auth
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
            builder.MapSwagger()
                .WithOpenApi()
                .WithName("MinitubeApi")
                .RequireAuthorization();
        });
    }
}