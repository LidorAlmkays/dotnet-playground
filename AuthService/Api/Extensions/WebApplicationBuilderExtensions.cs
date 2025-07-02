using AuthService.Properties;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace AuthService.Api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void SetupAuthentication(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(options =>
               {
                   options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                   options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
               }
               ).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
               {
                   options.ClientId = AppConfig.GoogleClientId;
                   options.ClientSecret = AppConfig.GoogleClientSecret;
                   options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidIssuer = AppConfig.JwtIssuer,
                         ValidAudience = AppConfig.JwtAudience,
                         IssuerSigningKeys = [new SymmetricSecurityKey(Convert.FromHexString(AppConfig.JwtSecretKey))],
                         ValidateIssuer = true,
                         ValidateLifetime = true,
                         ValidateAudience = true,
                         ValidateIssuerSigningKey = true,

                     };
                 }
            );

            builder.Services.AddAuthorization();
        }




        public static void SetupSwaggerConfig(this WebApplicationBuilder builder)
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

        public static void SetupMCP(this WebApplicationBuilder builder)
        {
            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();
        }


    }
}