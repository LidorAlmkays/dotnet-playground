using AuthService.Domain.Models;
using AuthService.Infrastructure.UserRepository;
using Common.Enums;
using AuthService.Infrastructure.Encryption;
using AuthService.Infrastructure.AuthMethodRepository;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.GoogleUserAuthenticationManager
{
    public class GoogleUserAuthentication(ILogger<GoogleUserAuthentication> logger, IUserRepository userRepository) : IGoogleUserAuthenticationManager
    // IPasswordEncryption passwordEncryption, IAuthMethodRepository authMethodRepository) 
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ILogger<GoogleUserAuthentication> _logger = logger;
        // private readonly IPasswordEncryption _passwordEncryption = passwordEncryption;
        // private readonly IAuthMethodRepository _authMethodRepository = authMethodRepository;

        public async Task LoginUserGoogleAsync(string userEmail, string providerUserId)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user);

            var googleAuthMethod = user.AuthMethods.FirstOrDefault(auth => auth.Provider == AuthProvider.Google)
                ?? throw new ArgumentException("No Google authentication method found for this user.");

            if (googleAuthMethod.ProviderUserId != providerUserId)
            {
                throw new InvalidOperationException($"Error: The current user ID for Google authentication ({googleAuthMethod.ProviderUserId}) is not the same as the one provided during login ({providerUserId}).");
            }

            _logger.LogInformation("Found Google AuthMethod with UserId: {ProviderUserId}", providerUserId);
        }

        public async Task RegisterUserGoogleAsync(string name, string userEmail, string providerUserId, Role role)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail).ConfigureAwait(false);
            if (user != null)
            {
                throw new InvalidOperationException("The email address is already in use. Please try with a different email.");
            }

            AuthMethodModel authMethodModel = new()
            {
                ProviderUserId = providerUserId,
                Provider = AuthProvider.Google,
            };
            user = new()
            {
                Name = name,
                Email = userEmail,
                AuthMethods = [authMethodModel]
            };

            await _userRepository.InsertUserAsync(user).ConfigureAwait(false);
        }
    }
}