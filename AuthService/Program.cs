using AuthService.Infrastructure.Encryption;
using AuthService.Infrastructure;
using AuthService.Infrastructure.UserRepository;
using Microsoft.EntityFrameworkCore;
using AuthService.Properties;
using AuthService.Application.Jwt;
using AuthService.Infrastructure.TokenCache;
using AuthService.Application.GoogleUserAuthenticationManager;
using AuthService.Application.LocalUserAuthenticationManager;
using AuthService.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.SetupAuthentication();
builder.AddServiceDefaults();
builder.Services.AddOpenApi();
buildInfrastructure(builder);
buildApplication(builder);
buildApiLevel(builder).Run();



void buildInfrastructure(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
        optionsBuilder.UseNpgsql(AppConfig.DbConnectionString,
         b => b.MigrationsAssembly(builder.Environment.ApplicationName)));

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = AppConfig.DistributedCacheConnectionString;
        options.InstanceName = builder.Environment.ApplicationName + "-";
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
    builder.SetupSwaggerConfig();
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.ApplyMigrations();
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

