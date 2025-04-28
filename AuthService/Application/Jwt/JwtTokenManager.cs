using AuthService.Domain.Models;
using AuthService.Infrastructure.TokenCache;
using AuthService.Properties;
using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Application.Jwt
{
    public class JwtTokenManager(AppConfig appConfig, IRefreshTokenStorage refreshTokenStorage) : IJwtTokenManager
    {
        private readonly IRefreshTokenStorage _refreshTokenStorage = refreshTokenStorage;
        private Either<Exception, string> GenerateAccessToken(string email, Guid userId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appConfig.JwtSecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new(JwtRegisteredClaimNames.Email, email),
                };
            var token = new JwtSecurityToken(
                issuer: appConfig.JwtIssuer,
                audience: appConfig.JwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(appConfig.AccessTokenLifetimeMinutes),
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

        private RefreshTokenModel GenerateRefreshToken(string accessToken)
        {
            var refreshToken = new RefreshTokenModel
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddDays(appConfig.RefreshTokenLifetimeDays)
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
                    return new TokenIssuingModel()
                    {
                        RefreshToken = refreshToken,
                        AccessToken = accessToken
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
            var key = Encoding.UTF8.GetBytes(appConfig.JwtSecretKey); // Your signing key here

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = appConfig.JwtIssuer,
                    ValidAudience = appConfig.JwtAudience,
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.Zero // No clock skew
                };

                // Validate the token
                var principal = handler.ValidateToken(token, parameters, out _);

                // Return claims if valid
                return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
            }
            catch (Exception)
            {
                // Handle invalid token: could log or return empty dictionary or throw exception
                return []; // Token is invalid
            }
        }
    }
}