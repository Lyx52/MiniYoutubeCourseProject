using System.Text;
using Domain.Constants;
using Domain.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApi.Data;
using WebApi.Swagger;

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
        services.AddDbContext<ApplicationDbContext>((options) =>
        {
            // TODO: Use proper db.
            options
                .UseInMemoryDatabase("AppDb")
                .EnableDetailedErrors(true)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            //options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
        
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password = PasswordOptionConfig.Default;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
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
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]!))
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