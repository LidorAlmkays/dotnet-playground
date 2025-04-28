using AuthService.Domain.Models;
using Common.CustomPracticalFunctional;
using LanguageExt;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.TokenCache
{
    public class RefreshTokenDistributedCache(IDistributedCache distributedCache) : IRefreshTokenStorage
    {
        IDistributedCache _distributedCache = distributedCache;
        private static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<Either<Exception, RefreshTokenModel>> GetAsync(string refreshTokenId, CancellationToken cancellationToken = default)
        {
            RefreshTokenModel? refreshToken;
            byte[]? refreshTokenJson = await _distributedCache.GetAsync(refreshTokenId, cancellationToken).ConfigureAwait(false);
            if (refreshTokenJson == null)
                return new KeyNotFoundException($"Refresh token with ID '{refreshTokenId}' was not found in cache.");
            try
            {
                refreshToken = JsonSerializer.Deserialize<RefreshTokenModel>(refreshTokenJson);
                ArgumentNullException.ThrowIfNull(refreshToken);
            }
            catch (ArgumentNullException ex)
            {
                return new ArgumentNullException($"Refresh token data for ID '{refreshTokenId}' was null after deserialization.", ex);
            }
            catch (JsonException ex)
            {
                return new JsonException("Failed to deserialize refresh token for ID '{refreshTokenId}'.", ex);
            }
            return refreshToken;
        }

        public async Task RemoveAsync(string refreshTokenId)
        {
            await _distributedCache.RemoveAsync(refreshTokenId).ConfigureAwait(false);
        }

        public async Task<Either<Exception, Unit>> StoreAsync(RefreshTokenModel refreshToken)
        {
            var options = new DistributedCacheEntryOptions()
           //    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
           .SetAbsoluteExpiration(refreshToken.ExpiresAt);
            try
            {
                ArgumentNullException.ThrowIfNull(refreshToken);
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(refreshToken, _serializerOptions));
                await _distributedCache.SetAsync(refreshToken.Id.ToString(), bytes, options).ConfigureAwait(false);
            }
            catch (EncoderFallbackException ex)
            {
                return new EncoderFallbackException("Couldn't translate the refresh token data", ex);
            }
            catch (ArgumentNullException ex)
            {
                return new ArgumentNullException("Refresh token cant be null when trying to store", ex);
            }
            catch (NotSupportedException ex)
            {
                return new NotSupportedException($"Couldn't serialized refresh token data", ex);
            }
            return new Unit();
        }
    }
}