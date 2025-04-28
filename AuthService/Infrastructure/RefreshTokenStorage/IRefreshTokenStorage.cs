using AuthService.Domain.Models;
using LanguageExt;

namespace AuthService.Infrastructure.TokenCache
{
    public interface IRefreshTokenStorage
    {
        /// <summary>
        /// Retrieves a refresh token from the storage system by its ID.
        /// </summary>
        /// <param name="refreshTokenId">The identifier of the refresh token to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// An <see cref="Either{TLeft, TRight}"/> containing:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="System.Exception"/></term>
        /// <description>
        /// If the operation fails due to missing data, deserialization issues, or null content.
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="RefreshTokenModel"/></term>
        /// <description>
        /// If the refresh token was successfully retrieved and deserialized.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Possible exceptions returned:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="KeyNotFoundException"/></term>
        /// <description>When no data is found in the storage for the provided refresh token ID.</description>
        /// </item>
        /// <item>
        /// <term><see cref="ArgumentNullException"/></term>
        /// <description>When the deserialized refresh token is unexpectedly null.</description>
        /// </item>
        /// <item>
        /// <term><see cref="JsonException"/></term>
        /// <description>When the refresh token data cannot be deserialized properly.</description>
        /// </item>
        /// </list>
        /// </remarks>`
        Task<Either<Exception, RefreshTokenModel>> GetAsync(string refreshTokenId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Removes a stored refresh token by its identifier.
        /// </summary>
        /// <param name="refreshTokenId">The unique identifier of the refresh token to remove.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        /// <remarks>
        /// This operation completes silently if the token does not exist.
        /// </remarks>
        Task RemoveAsync(string refreshTokenId);
        /// <summary>
        /// Stores the given refresh token in the storage system with its expiration settings.
        /// </summary>
        /// <param name="refreshToken">The refresh token model to be stored.</param>
        /// <returns>
        /// An <see cref="Either{TLeft, TRight}"/> containing:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="System.Exception"/></term>
        /// <description>
        /// If the operation fails due to serialization, encoding, null arguments, or unsupported data formats.
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="LanguageExt.Unit"/></term>
        /// <description>
        /// If the refresh token was successfully stored in the storage.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Possible exceptions returned:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="EncoderFallbackException"/></term>
        /// <description>When the refresh token data cannot be converted to bytes.</description>
        /// </item>
        /// <item>
        /// <term><see cref="ArgumentNullException"/></term>
        /// <description>When the input refresh token is null.</description>
        /// </item>
        /// <item>
        /// <term><see cref="NotSupportedException"/></term>
        /// <description>When the refresh token data cannot be serialized to JSON.</description>
        /// </item>
        /// </list>
        /// </remarks>
        Task<Either<Exception, Unit>> StoreAsync(RefreshTokenModel refreshToken);
    }
}