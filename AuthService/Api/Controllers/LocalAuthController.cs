using AuthService.Application.LocalUserAuthenticationManager;
using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using Common.Enums;

namespace AuthService.Api.Controllers
{
    [Route("[controller]")]
    public class LocalAuthController(ILogger<LocalAuthController> logger, ILocalUserAuthenticationManager localUserAuthenticationManager) : Controller
    {
        private readonly ILogger<LocalAuthController> _logger = logger;
        private readonly ILocalUserAuthenticationManager _localUserAuthentication = localUserAuthenticationManager;

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> JwtLogin(UserLocalLoginRequestDTO userLocalLoginRequestDTO)
        {
            _logger.LogInformation("User trying to login");

            try
            {
                await _localUserAuthentication.LoginUserAsync(userLocalLoginRequestDTO.UserEmail, userLocalLoginRequestDTO.Password).ConfigureAwait(false);
                return Ok(new { Message = "Login successful." });
            }
            catch (ArgumentNullException)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found.", userLocalLoginRequestDTO.UserEmail);
                return NotFound(new { Message = "User not found." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return BadRequest(new { Message = "Authentication method error: " + ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login.");
                return StatusCode(500, new { Message = "An unexpected error occurred." });
            }

        }
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> RegisterLogin(UserLocalRegisterRequestDTO userLocalRegisterRequestDTO)
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