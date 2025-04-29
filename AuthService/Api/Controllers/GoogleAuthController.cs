using AuthService.Api.Extensions;
using AuthService.Application.GoogleUserAuthenticationManager;
using AuthService.Application.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Common.Enums;
using AuthService.Properties;
namespace AuthService.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class GoogleController(ILogger<GoogleController> logger, IGoogleUserAuthenticationManager userAuthenticationManager, IJwtTokenManager jwtTokenManager) : Controller
    {
        private readonly ILogger<GoogleController> _logger = logger;
        private readonly IGoogleUserAuthenticationManager _IUserAuthenticationManager = userAuthenticationManager;
        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;
        private static Uri _redirectUri => AppConfig.GoogleRedirectUri
            ?? throw new InvalidOperationException($"GOOGLE_REDIRECT_URI environment variable is missing."); // Update this to your actual redirect URI

        [Route("login")]
        [HttpGet]
        public async void GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = _redirectUri + "/login-response"
            };
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props)
                             .ConfigureAwait(false);
        }
        [Route("register")]
        [HttpGet]
        public async void GoogleRegister()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = _redirectUri + "/register-response"
            };
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props)
                             .ConfigureAwait(false);
        }
        [Route("login-response")]
        [HttpGet]
        public async Task<IActionResult> GoogleLoginResponse()
        {
            try
            {
                var userContext = await HttpContext.GetGoogleUserContextAsync().ConfigureAwait(false);
                _logger.LogInformation("Processing Google login for {Email}", userContext.Email);

                Guid userId = await _IUserAuthenticationManager.ValidateUserGoogleLoginAsync(userContext.Email, userContext.NameIdentifier).ConfigureAwait(false);
                return (await _jwtTokenManager.IssueTokens(userContext.Email, userId).ConfigureAwait(false)).Match<IActionResult>(
                                Right: tokenIssuingModel =>
                                {
                                    // Set the Access Token in the Authorization header
                                    Response.Headers.Append("Authorization", $"Bearer {tokenIssuingModel.AccessToken}");

                                    // Set the Refresh Token in a cookie with HttpOnly and Secure flags
                                    var cookieOptions = new CookieOptions
                                    {
                                        HttpOnly = true,
                                        Secure = true,  // Make sure you're using HTTPS in production
                                        SameSite = SameSiteMode.Strict, // Or Lax, depending on your client
                                        Expires = tokenIssuingModel.RefreshToken.ExpiresAt // Or however long you want the refresh token to live
                                    };

                                    Response.Cookies.Append("RefreshToken", tokenIssuingModel.RefreshToken.Id.ToString(), cookieOptions);

                                    _logger.LogInformation("Login successful, tokens issued for {Email}", userContext.Email);

                                    return Ok(new { Message = "Login successful." });
                                },
                                   Left: error => error switch
                                   {
                                       EncoderFallbackException or
                                       ArgumentNullException or
                                       NotSupportedException => BadRequest(new { Error = error.Message }),
                                       SecurityTokenException => Unauthorized(new { Error = error.Message }),
                                       _ => throw error // Let unhandled exceptions bubble up
                                   }
                               );
            }
            catch (ArgumentNullException)
            {
                _logger.LogWarning("Google login failed: User with email not found.");
                return NotFound(new { Message = "User not found." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Google login failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Google login failed: {Message}", ex.Message);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during Google login.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }
        [Route("register-response")]
        [HttpGet]
        public async Task<IActionResult> GoogleRegisterResponse()
        {
            try
            {
                var userContext = await HttpContext.GetGoogleUserContextAsync().ConfigureAwait(false);

                _logger.LogInformation("Processing Google registration for {Email}", userContext.Email);

                await _IUserAuthenticationManager.RegisterUserGoogleAsync(
                    userContext.GivenName,
                    userContext.Email,
                    userContext.NameIdentifier,
                    Role.User
                ).ConfigureAwait(false);

                return Ok(new { Message = $"Successfully processed Google registration for {userContext.Email}" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Google registration failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during Google registration.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }

    }
}