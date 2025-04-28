using System.Text;
using AuthService.Application.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JwtAuthController(IJwtTokenManager jwtTokenManager) : Controller//(ILogger<JwtAuthController> logger) : Controller
    {
        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;

        [Route("refresh-token")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshTokenId = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshTokenId))
            {
                return BadRequest(new { error = "Refresh token is missing." });
            }

            var result = await _jwtTokenManager.RefreshTokensAsync(refreshTokenId).ConfigureAwait(false);

            return result.Match<IActionResult>(
                Right: tokenIssuingModel =>
                {
                    // Store the new refresh token in the cookie
                    Response.Cookies.Append("RefreshToken", tokenIssuingModel.RefreshToken.Id.ToString());

                    // Store the new access token in the header
                    Response.Headers.Append("Authorization", $"Bearer {tokenIssuingModel.AccessToken}");

                    return Ok(new { Message = "Token refreshed successfully." });
                },
                Left: error => error switch
                {
                    EncoderFallbackException or
                    ArgumentNullException or
                    NotSupportedException => BadRequest(new { error = error.Message }),
                    SecurityTokenException => Unauthorized(new { error = error.Message }),
                    _ => throw error
                }
            );
        }
    }
}