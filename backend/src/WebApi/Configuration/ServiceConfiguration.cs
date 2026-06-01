namespace WebApi.Configuration;

using System.Text;
using Application.Configuration;
using Application.Interfaces;
using Application.Services;
using Infrastructure;
using Infrastructure.External;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class ServiceConfiguration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
        builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection(SpotifyOptions.SectionName));
        builder.Services.Configure<GeniusOptions>(builder.Configuration.GetSection(GeniusOptions.SectionName));
        builder.Services.Configure<MusicTheoryOptions>(builder.Configuration.GetSection(MusicTheoryOptions.SectionName));
        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<IChordService, ChordService>();
        builder.Services.AddScoped<ISearchChordsService, SearchChordsService>();
        builder.Services.AddScoped<IRegisterService, RegisterService>();
        builder.Services.AddScoped<ILoginService, LoginService>();
        builder.Services.AddScoped<ISubmitChordsService, SubmitChordsService>();
        builder.Services.AddScoped<IGetLyricsService, GetLyricsService>();
        builder.Services.AddHttpClient<ISearchTracksService, SpotifySearchTracksService>();
        builder.Services.AddHttpClient("Genius");
        builder.Services.AddSingleton<ILyricsService, GeniusLyricsService>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var key = Encoding.ASCII.GetBytes(jwtKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Set to true and provide 'ValidIssuer' in production
                ValidateAudience = false, // Set to true and provide 'ValidAudience' in production
                ClockSkew = TimeSpan.Zero // Immediate expiration
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });
    }
}