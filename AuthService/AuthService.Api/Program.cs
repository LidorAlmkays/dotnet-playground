using AuthService.Application.UserAuthenticationManager;
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

var builder = WebApplication.CreateBuilder(args);
Configuration.SetupConfig(builder);
SetupAuthentication();
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
buildInfrastructure(builder);
buildApplication(builder);
buildApiLevel(builder).Run();



void buildInfrastructure(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(Configuration.GetDBConnectionString(),
         b => b.MigrationsAssembly("AuthService.Api")));
    builder.Services.AddScoped<IUserRepository, EFCoreUserRepository>();
}

void buildApplication(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IPasswordEncryption, SaltAndPepperEncryption>(provider =>
     {
         var (pepperLength, pepperLetters) = Configuration.GetSaltAndPepperConfig();
         return new SaltAndPepperEncryption(pepperLetters, pepperLength);
     });
    builder.Services.AddScoped<IUserAuthenticationManager, UserAuthenticationWithEncryptionPassword>();
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
           options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
           options.DefaultForbidScheme = GoogleDefaults.AuthenticationScheme;
           options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
       }
       ).AddCookie()
       .AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
       {
           var (clientId, clientSecret) = Configuration.GetGoogleOAUTHConfig();
           googleOptions.ClientId = clientId;
           googleOptions.ClientSecret = clientSecret;
       })
    .AddJwtBearer(options =>
       {
           var (issuer, audience, secretKey) = Configuration.GetJWTConfig();
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidIssuer = issuer,
               ValidAudience = audience,
               IssuerSigningKeys = [new SymmetricSecurityKey(Convert.FromHexString(secretKey))],
               ValidateIssuer = true,
               ValidateLifetime = true,
               ValidateAudience = true,
               ValidateIssuerSigningKey = true,

           };
       });

    builder.Services.AddAuthorization();
}

void SetupSwagger()
{
    builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = Configuration.GetServiceName(),
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