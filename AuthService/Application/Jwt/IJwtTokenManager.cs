using AuthService.Domain.Models;
using Common.CustomPracticalFunctional;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.Application.Jwt
{

    //TODO: I KNOW I SHOULD CHANGE THE ERROS BEING THROWN TO CUSTOM ONES .... BUT I GET THE POINT
    public interface IJwtTokenManager
    {
        /// <summary>
        /// Extracts and validates claims from a JWT access token.
        /// </summary>
        /// <param name="token">The JWT access token to validate and extract claims from.</param>
        /// <param name="validateLifetime">
        /// A boolean indicating whether the token's lifetime should be validated.
        /// Defaults to <c>true</c>.
        /// </param>
        /// <returns>
        /// A dictionary containing the claim types and their corresponding values if the token is valid;
        /// otherwise, an empty dictionary if validation fails.
        /// </returns>
        /// <remarks>
        /// Token validation includes:
        /// <list type="bullet">
        /// <item><description>Signature validation using the configured symmetric key.</description></item>
        /// <item><description>Issuer and audience checks based on application configuration.</description></item>
        /// <item><description>Optional lifetime validation with no clock skew allowed.</description></item>
        /// </list>
        /// 
        /// This method swallows any exceptions and returns an empty dictionary if validation fails. 
        /// Customize this behavior if more detailed error reporting is needed.
        /// </remarks>

        Dictionary<string, string> ExtractValidatedClaims(string token, bool validateLifetime = true);
        /// <summary>
        /// Issues a new pair of access and refresh tokens for the specified user.
        /// </summary>
        /// <param name="email">The email address associated with the user.</param>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>
        /// An <see cref="Either{TLeft, TRight}"/> containing:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="System.Exception"/></term>
        /// <description>
        /// If an error occurs during token generation, encryption, serialization, or storage.
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="TokenIssuingModel"/></term>
        /// <description>
        /// If both the access and refresh tokens were successfully generated and stored.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Possible exceptions returned:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="SecurityTokenEncryptionFailedException"/></term>
        /// <description>When the access token cannot be encrypted.</description>
        /// </item>
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
        /// <para>
        /// Any unexpected exceptions are not handled and will bubble up to the caller.
        /// </para>
        /// </remarks>
        Task<Either<Exception, TokenIssuingModel>> IssueTokens(string email, Guid userId);
        /// <summary>
        /// Refreshes a pair of tokens using a valid, non-expired refresh token.
        /// </summary>
        /// <param name="oldRefreshTokenId">The ID of the refresh token to be validated and replaced.</param>
        /// <returns>
        /// An <see cref="Either{TLeft, TRight}"/> containing:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="System.Exception"/></term>
        /// <description>
        /// If the refresh token is invalid, expired, or if any error occurs during token regeneration or storage.
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="TokenIssuingModel"/></term>
        /// <description>
        /// If the refresh token is valid and new tokens were successfully generated and stored.
        /// </description>
        /// </item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Possible exceptions returned:
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="SecurityTokenException"/></term>
        /// <description>When the refresh token has expired.</description>
        /// </item>
        /// <item>
        /// <term><see cref="SecurityTokenEncryptionFailedException"/></term>
        /// <description>When the access token cannot be encrypted.</description>
        /// </item>
        /// <item>
        /// <term><see cref="EncoderFallbackException"/></term>
        /// <description>When the refresh token data cannot be converted to bytes.</description>
        /// </item>
        /// <item>
        /// <term><see cref="ArgumentNullException"/></term>
        /// <description>When the refresh token or any of its required data is null.</description>
        /// </item>
        /// <item>
        /// <term><see cref="NotSupportedException"/></term>
        /// <description>When the refresh token data cannot be serialized to JSON.</description>
        /// </item>
        /// </list>
        /// <para>
        /// Any unhandled exceptions are rethrown and will bubble up to the caller.
        /// </para>
        /// </remarks>

        Task<Either<Exception, TokenIssuingModel>> RefreshTokensAsync(string oldRefreshTokenId);
    }
}