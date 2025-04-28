using AuthService.Api.Extensions;
using AuthService.Application.GoogleUserAuthenticationManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class GoogleAuthController(ILogger<GoogleAuthController> logger, IGoogleUserAuthenticationManager userAuthenticationManager) : Controller
    {
        private readonly ILogger<GoogleAuthController> _logger = logger;
        private readonly IGoogleUserAuthenticationManager _IUserAuthenticationManager = userAuthenticationManager;
        [Route("login")]
        [HttpGet]
        public async void GoogleLogin()
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = "/GoogleAuth/login-response"
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
                RedirectUri = "/GoogleAuth/register-response"
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

                await _IUserAuthenticationManager.LoginUserGoogleAsync(userContext.Email, userContext.NameIdentifier).ConfigureAwait(false);

                return Ok(new { Message = $"Successfully processed Google login for {userContext.Email}" });
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
                    Common.Enums.Role.User
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