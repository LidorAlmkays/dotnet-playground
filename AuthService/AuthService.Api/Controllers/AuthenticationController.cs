using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthService.Application.UserAuthenticationManager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Controllers
{

    [ApiController]
    [Route("[controller]/[action]")]
    public class AuthenticationController(ILogger<AuthenticationController> logger, IWebHostEnvironment env, IUserAuthenticationManager userAuthenticationManager) : Controller
    {
        private readonly ILogger<AuthenticationController> _logger = logger;
        private readonly IUserAuthenticationManager _IUserAuthenticationManager = userAuthenticationManager;
        [HttpGet]
        public async Task Google([FromQuery] string action = "login")
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            action = action.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

            if (action != "login" && action != "register")
            {
                throw new BadHttpRequestException($"❌ Invalid action '{action}'. Expected 'login' or 'register'.");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            props.Items["action"] = action;

            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props)
                             .ConfigureAwait(false);

        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            var action = result.Properties?.Items.TryGetValue("action", out var a) == true ? a : "login";
            ArgumentNullException.ThrowIfNull(action);
            var claims = result.Principal?
                .Identities
                .SelectMany(identity => identity.Claims)
                .ToList();

            var nameIdentifier = claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;

            var name = claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.Name)?.Value;

            var givenName = claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.GivenName)?.Value;

            var email = claims?.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email)?.Value;

            if (env.IsDevelopment())
            {
                Console.WriteLine($"🆔 NameIdentifier: {nameIdentifier}");
                Console.WriteLine($"👤 Name: {name}");
                Console.WriteLine($"🧍 GivenName: {givenName}");
                Console.WriteLine($"📧 Email: {email}");
            }
            ArgumentNullException.ThrowIfNullOrEmpty(nameIdentifier);
            ArgumentNullException.ThrowIfNullOrEmpty(email);
            ArgumentNullException.ThrowIfNullOrEmpty(givenName);


#pragma warning disable CA1308 // Normalize strings to uppercase
            switch (action.ToLowerInvariant())
            {
                case "login":
                    _logger.LogInformation("Processing Google login for {Email}", email);
                    await _IUserAuthenticationManager.LoginUserGoogleAsync(email, nameIdentifier).ConfigureAwait(false);
                    break;

                case "register":
                    _logger.LogInformation("Processing Google registration for {Email}", email);
                    await _IUserAuthenticationManager.RegisterUserGoogleAsync(givenName, email, nameIdentifier, Common.Enums.Role.User).ConfigureAwait(false);
                    break;

                default:
                    _logger.LogWarning("Unknown Google action: {Action}", action);
                    return BadRequest($"Invalid action '{action}'. Expected 'login' or 'register'.");
            }
#pragma warning restore CA1308 // Normalize strings to uppercase

            return Ok($"Successfully processed Google {action} for {email}");
        }

    }
}