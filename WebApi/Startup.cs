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
        services.AddDbContext<UserDbContext>((options) =>
        {
            // TODO: Use proper db.
            options
                .UseInMemoryDatabase("UserDb")
                .EnableDetailedErrors(true);
            //options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        services.AddDbContext<VideoDbContext>((options) =>
        {
            // TODO: Use proper db.
            options
                .UseInMemoryDatabase("VideoDb")
                .EnableDetailedErrors(true);
            //options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
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
                    ValidateIssuer = true,
                    ValidateAudience = true,
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
        services.AddTransient<IContentService, ContentService>();
        services.AddTransient<IVideoRepository, VideoRepository>();
        services.AddTransient<IContentRepository, ContentRepository>();
        services.AddMemoryCache();
        
        // Processing channel
        services.AddChannel<VideoTask>();
        
        services.AddSingleton<ApiConfiguration>(
            _ => Configuration.GetSection(nameof(ApiConfiguration)).Get<ApiConfiguration>()!
        );
        services.AddHostedService<BackgroundProcessingService>();
    }
    

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
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