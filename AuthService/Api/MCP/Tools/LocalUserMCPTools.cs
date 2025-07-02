using AuthService.Application.Jwt;
using AuthService.Application.LocalUserAuthenticationManager;
using Common.DTOs;
using Common.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModelContextProtocol.Server;
using OpenTelemetry.Trace;
using System.ComponentModel;
using System.Text;



namespace AuthService.Api.MCP.Tools
{
    [McpServerToolType]
    public class LocalUserMCPTools(ILogger<LocalUserMCPTools> logger, ILocalUserAuthenticationManager localUserAuthenticationManager, IJwtTokenManager jwtTokenManager)
    {
        private readonly ILogger<LocalUserMCPTools> _logger = logger;
        private readonly ILocalUserAuthenticationManager _localUserAuthentication = localUserAuthenticationManager;

        private readonly IJwtTokenManager _jwtTokenManager = jwtTokenManager;

        [McpServerTool, Description("Register a new user.")]
        public async Task<string> RegisterUserAsync(UserLocalRegisterRequestDTO userLocalRegisterRequestDTO)
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

                return "Registration successful.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
                return ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during registration.");
                return "An unexpected error occurred.";
            }
        }

        [McpServerTool, Description("Login a user.")]
        public async Task<TokenIssuingResponseDTO> UserLoginAsync(UserLocalLoginRequestDTO userLocalLoginRequestDTO)
        {
            _logger.LogInformation("User trying to login");

            Guid userId = await _localUserAuthentication.ValidateUserLoginAsync(
                userLocalLoginRequestDTO.Email,
                userLocalLoginRequestDTO.Password
            ).ConfigureAwait(false);

            return (await _jwtTokenManager.IssueTokens(userLocalLoginRequestDTO.Email, userId).ConfigureAwait(false)).Match(
               Right: tokenIssuingModel =>
            {
                if (tokenIssuingModel is null)
                {
                    _logger.LogWarning("Login failed: Token issuing model is null.");
                    throw new InvalidOperationException("Token issuing model is null.");
                }
                _logger.LogInformation("Login successful, tokens issued for {Email}", userLocalLoginRequestDTO.Email);

                return new TokenIssuingResponseDTO
                {
                    AccessToken = tokenIssuingModel.AccessToken,
                    RefreshTokenId = tokenIssuingModel.RefreshToken.Id.ToString()
                };
            },
                 Left: error =>
                 {
                     throw error;
                 }
             );




        }
    }
}