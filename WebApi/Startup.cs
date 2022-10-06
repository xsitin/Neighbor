using Common.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using WebApi.Infrastructure;

namespace WebApi;

using domain;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var client = new MongoClient(Configuration["ConnectionStrings_Mongo"] ??
                                     Configuration["ConnectionStrings:Mongo"]);
        var db = client.GetDatabase("Board");
        services.AddSingleton(_ => db);
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        services.AddScoped<IRatingCalculator, RatingCalculator>();

        services.AddScoped<IMongoCollection<ImageDocument>>(_ => db.GetCollection<ImageDocument>("Images"));
        services.AddScoped<IImageRepository, ImageRepository>(provider => new ImageRepository(
            provider.GetService<IMongoCollection<ImageDocument>>(), Configuration["Paths_SavedImages"] ??
                                                                    Configuration["Paths:SavedImages"]));

        services.AddScoped<AdRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = nameof(Account.Name);
            options.ClaimsIdentity.RoleClaimType = nameof(Account.Role);
        });
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = AuthOptions.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetAsymmetricSecurityKey(),
                ValidateIssuerSigningKey = true,
            };
        });

        services.AddCors(options =>
        {
            options.AddPolicy("myCorsPolicy", builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.SetIsOriginAllowed(s => true);
                builder.AllowAnyOrigin();
            });
        });
        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "Neighbor WebApi", Version = "v1"});
            c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.",
            });

            c.OperationFilter<AuthOperationFilter>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors("myCorsPolicy");
        app.UseResponseCaching();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}