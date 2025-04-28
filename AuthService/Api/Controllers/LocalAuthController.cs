using AuthService.Application.LocalUserAuthenticationManager;
using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Common.Enums;
using AuthService.Application.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Api.Controllers
{
    [Route("[controller]")]
    public class LocalAuthController(ILogger<LocalAuthController> logger, ILocalUserAuthenticationManager localUserAuthenticationManager, IJwtTokenManager jwtTokenManager) : Controller
    {
        private readonly ILogger<LocalAuthController> _logger = logger;
        private readonly ILocalUserAuthenticationManager _localUserAuthentication = localUserAuthenticationManager;

        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> JwtLogin([FromBody] UserLocalLoginRequestDTO userLocalLoginRequestDTO)
        {
            _logger.LogInformation("User trying to login");

            try
            {
                Guid userId = await _localUserAuthentication.ValidateUserLoginAsync(
                    userLocalLoginRequestDTO.Email,
                    userLocalLoginRequestDTO.Password
                ).ConfigureAwait(false);

                return (await _jwtTokenManager.IssueTokens(userLocalLoginRequestDTO.Email, userId).ConfigureAwait(false)).Match<IActionResult>(
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

                    _logger.LogInformation("Login successful, tokens issued for {Email}", userLocalLoginRequestDTO.Email);

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
                _logger.LogWarning("Login failed: User with email {Email} not found.", userLocalLoginRequestDTO.Email);
                return NotFound(new { Message = "User not found." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }

        }
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterLogin([FromBody] UserLocalRegisterRequestDTO userLocalRegisterRequestDTO)
        {
            _logger.LogInformation("User trying to register");

            try
            {
                await _localUserAuthentication.RegisterUserAsync(
                    userLocalRegisterRequestDTO.Name,
                    userLocalRegisterRequestDTO.UserEmail,
                    userLocalRegisterRequestDTO.Password,
                    Role.User
                ).ConfigureAwait(false);

                return Ok(new { Message = "Registration successful." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during registration.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }
        }
    }
}