using AuthService.Domain.Models;
using AuthService.Infrastructure.TokenCache;
using AuthService.Properties;
using Common.DTOs;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Application.Jwt
{
    public class JwtTokenManager(ILogger<JwtTokenManager> logger, IRefreshTokenStorage refreshTokenStorage) : IJwtTokenManager
    {
        private readonly IRefreshTokenStorage _refreshTokenStorage = refreshTokenStorage;
        private readonly ILogger<JwtTokenManager> _logger = logger;
        private static Either<Exception, string> GenerateAccessToken(string email, Guid userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.JwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new(JwtRegisteredClaimNames.Email, email),
                };
            int accessTokenLifetimeMinutes = AppConfig.AccessTokenLifetimeMinutes;
            if (accessTokenLifetimeMinutes < 0)
            {
                accessTokenLifetimeMinutes = 0;
            }
            var token = new JwtSecurityToken(
                issuer: AppConfig.JwtIssuer,
                audience: AppConfig.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessTokenLifetimeMinutes),
                signingCredentials: credentials
            );
            try
            {
                var result = new JwtSecurityTokenHandler().WriteToken(token);
                return result;
            }
            catch (SecurityTokenEncryptionFailedException ex)
            {
                return new SecurityTokenEncryptionFailedException("Failed to encrypt the jwt token", ex);
            }
        }

        private static RefreshTokenModel GenerateRefreshToken(string accessToken)
        {
            int refreshTokenLifetimeDays = AppConfig.RefreshTokenLifetimeDays;
            if (refreshTokenLifetimeDays < 0)
            {
                refreshTokenLifetimeDays = 0;
            }
            var refreshToken = new RefreshTokenModel
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenLifetimeDays)
            };
            return refreshToken;
        }

        public async Task<Either<Exception, TokenIssuingModel>> IssueTokens(string email, Guid userId)
        {
            return await GenerateAccessToken(email, userId).Match(async accessToken =>
            {
                var refreshToken = GenerateRefreshToken(accessToken);
                var storingResult = await _refreshTokenStorage.StoreAsync(refreshToken).ConfigureAwait(false);
                return storingResult.Match<Either<Exception, TokenIssuingModel>>(Right: _ =>
                {
                    return new TokenIssuingModel
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken
                    };
                }, Left: error =>
                {
                    return error switch
                    {
                        EncoderFallbackException => error,
                        ArgumentNullException => error,
                        NotSupportedException => error,
                        _ => throw error,
                    };
                });
            }, error =>
                        {
                            return error switch
                            {
                                SecurityTokenEncryptionFailedException => Task.FromResult<Either<Exception, TokenIssuingModel>>(error),
                                _ => throw error,
                            };
                        }
            ).ConfigureAwait(false);
        }


        public async Task<Either<Exception, TokenIssuingModel>> RefreshTokensAsync(string oldRefreshTokenId)
        {
            var result = await _refreshTokenStorage.GetAsync(oldRefreshTokenId).ConfigureAwait(false);
            return await result.Match(async refreshToken =>
            {
                if (refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    await _refreshTokenStorage.RemoveAsync(oldRefreshTokenId).ConfigureAwait(false);
                    return new SecurityTokenException("Refresh token expired");
                }
                string email = ExtractValidatedClaims(refreshToken.AccessToken)[JwtRegisteredClaimNames.Email];
                Guid userId = Guid.Parse(ExtractValidatedClaims(refreshToken.AccessToken)[JwtRegisteredClaimNames.Sub]);

                return await (await IssueTokens(email, userId).ConfigureAwait(false)).Match<Task<Either<Exception, TokenIssuingModel>>>(
                   async tokens =>
                    {
                        await _refreshTokenStorage.RemoveAsync(oldRefreshTokenId).ConfigureAwait(false);
                        return tokens;
                    }, error =>
                    {
                        var ex = error switch
                        {
                            EncoderFallbackException => error,
                            ArgumentNullException => error,
                            NotSupportedException => error,
                            SecurityTokenEncryptionFailedException => error,
                            _ => throw error,
                        };
                        return Task.FromResult<Either<Exception, TokenIssuingModel>>(ex);
                    }
                    ).ConfigureAwait(false);
            },
             error =>
            {
                return Task.FromResult<Either<Exception, TokenIssuingModel>>(error);
            }).ConfigureAwait(false);
        }

        public Dictionary<string, string> ExtractValidatedClaims(string token, bool validateLifetime = true)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(AppConfig.JwtSecretKey); // Your signing key here
            var claims = new Dictionary<string, string>();

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = AppConfig.JwtIssuer,
                    ValidAudience = AppConfig.JwtAudience,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.Zero // No clock skew
                };

                // Try to parse the token
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                // If the token is a valid JWT token, extract claims
                if (jwtToken != null)
                {
                    claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during token parsing (invalid format, etc.)
                _logger.LogWarning($"Error extracting claims from token: {ex.Message}");
            }

            return claims;
        }
    }
}