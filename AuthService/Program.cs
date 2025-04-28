using AuthService.Api;
using AuthService.Infrastructure.Encryption;
using AuthService.Infrastructure;
using AuthService.Infrastructure.UserRepository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthService.Api.Controllers;
using AuthService.Properties;
using DotNetEnv;
using AuthService.Application.Jwt;
using AuthService.Infrastructure.TokenCache;
using AuthService.Application.GoogleUserAuthenticationManager;
using AuthService.Application.LocalUserAuthenticationManager;

var builder = WebApplication.CreateBuilder(args);
Env.Load();
var appConfig = new AppConfig();
builder.Services.AddSingleton(appConfig);
SetupAuthentication();
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
buildInfrastructure(builder);
buildApplication(builder);
buildApiLevel(builder).Run();



void buildInfrastructure(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(appConfig.DbConnectionString,
         b => b.MigrationsAssembly(builder.Environment.ApplicationName)));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = appConfig.DistributedCacheConnectionString;
        options.InstanceName = "CacheInstance";
    });
    builder.Services.AddScoped<IUserRepository, EFCoreUserRepository>();
    builder.Services.AddScoped<IRefreshTokenStorage, RefreshTokenDistributedCache>();

}

void buildApplication(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IJwtTokenManager, JwtTokenManager>();
    builder.Services.AddScoped<IPasswordEncryption, SaltAndPepperEncryption>();
    builder.Services.AddScoped<IGoogleUserAuthenticationManager, GoogleUserAuthentication>();
    builder.Services.AddScoped<ILocalUserAuthenticationManager, LocalUserAuthentication>();

}

WebApplication buildApiLevel(WebApplicationBuilder builder)
{
    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    SetupSwagger();
    var app = builder.Build();


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapSwagger().RequireAuthorization();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapDefaultEndpoints();
    return app;
}


void SetupAuthentication()
{
    builder.Services.AddAuthentication(options =>
       {
           options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
       }
       ).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
       {
           options.ClientId = appConfig.GoogleClientId;
           options.ClientSecret = appConfig.GoogleClientSecret;
           options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
         {
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidIssuer = appConfig.JwtIssuer,
                 ValidAudience = appConfig.JwtAudience,
                 IssuerSigningKeys = [new SymmetricSecurityKey(Convert.FromHexString(appConfig.JwtSecretKey))],
                 ValidateIssuer = true,
                 ValidateLifetime = true,
                 ValidateAudience = true,
                 ValidateIssuerSigningKey = true,

             };
         }
    );

    builder.Services.AddAuthorization();
}

void SetupSwagger()
{
    builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = builder.Environment.ApplicationName,
        Version = "v1",
    });
    options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme,
    securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the Bearer Authorization : `Bearer Generated-JWT-Token`",
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Name = "google oauth2",
        Description = "OAuth2 authentication using Google.",
        Type = SecuritySchemeType.OAuth2,
        Flows = new()
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://accounts.google.com/o/oauth2/v2/auth"),
                TokenUrl = new Uri("https://oauth2.googleapis.com/token"),
                Scopes = new Dictionary<string, string>
           {
                    { "openid", "OpenID Connect scope" },
                    { "profile", "Access user profile" },
                    { "email", "Access user email" }
                   // Add additional scopes as needed for your API access
           }
            },
        }
    });
});

}