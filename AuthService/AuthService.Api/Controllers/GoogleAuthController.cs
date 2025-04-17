using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.Api.Extensions;
using AuthService.Application.UserAuthenticationManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class GoogleAuthController(ILogger<GoogleAuthController> logger, IUserAuthenticationManager userAuthenticationManager) : Controller
    {
        private readonly ILogger<GoogleAuthController> _logger = logger;
        private readonly IUserAuthenticationManager _IUserAuthenticationManager = userAuthenticationManager;
        [Route("login")]
        [HttpGet]
        public async Task GoogleLogin()
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
        public async Task GoogleRegister()
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
            var userContext = await HttpContext.GetGoogleUserContextAsync().ConfigureAwait(false);
            _logger.LogInformation("Processing Google login for {Email}", userContext.Email);
            await _IUserAuthenticationManager.LoginUserGoogleAsync(userContext.Email, userContext.NameIdentifier).ConfigureAwait(false);

            return Ok($"Successfully processed Google login for {userContext.Email}");
        }
        [Route("register-response")]
        [HttpGet]
        public async Task<IActionResult> GoogleRegisterResponse()
        {
            var userContext = await HttpContext.GetGoogleUserContextAsync().ConfigureAwait(false);

            _logger.LogInformation("Processing Google registration for {Email}", userContext.Email);
            await _IUserAuthenticationManager.RegisterUserGoogleAsync(userContext.GivenName, userContext.Email, userContext.NameIdentifier, Common.Enums.Role.User).ConfigureAwait(false);
            return Ok($"Successfully processed Google register for {userContext.Email}");
        }

    }
}