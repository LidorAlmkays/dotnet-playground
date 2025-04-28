using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JwtAuthController(ILogger<JwtAuthController> logger, IJwtTokenManager jwtTokenManager) : Controller//(ILogger<JwtAuthController> logger) : Controller
    {
        private readonly ILogger<JwtAuthController> _logger = logger;
        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;
        [Route("login")]
        [HttpGet]
        public IActionResult JwtLogin()
        {
            _logger.LogInformation("User trying to login");
            // var (a, v) = _jwtTokenManager.IssueTokens("asfsafsaf");
            // return Ok(new { a, v });
            return Ok();
        }
        [Route("TEST")]
        [HttpGet]
        public async Task<IActionResult> TESTREMOVEAsync()
        {
            _logger.LogInformation("REMOVE ME IM TEST");
            var result = await _jwtTokenManager.RefreshTokensAsync("asfsafsaf").ConfigureAwait(false);
            return Ok();
        }
    }
}