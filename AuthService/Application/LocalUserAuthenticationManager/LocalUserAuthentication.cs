using AuthService.Domain.Models;
using AuthService.Infrastructure.Encryption;
using AuthService.Infrastructure.UserRepository;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Application.LocalUserAuthenticationManager
{
    public class LocalUserAuthentication(ILogger<LocalUserAuthentication> logger, IUserRepository userRepository, IPasswordEncryption passwordEncryption) : ILocalUserAuthenticationManager
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordEncryption _passwordEncryption = passwordEncryption;
        ILogger<LocalUserAuthentication> _logger = logger;
        public async Task LoginUserAsync(string userEmail, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user);
            var localAuthMethod = user.AuthMethods.FirstOrDefault(auth => auth.Provider == AuthProvider.Local) ??
            throw new ArgumentException("No local authentication method found for this user.");
            if (!_passwordEncryption.CheckPasswordValid(password, localAuthMethod.Password, localAuthMethod.PasswordKey))
            {
                throw new InvalidOperationException($"Error: The provided password is incorrect for the user with email {userEmail}.");
            }
            _logger.LogInformation("Found local AuthMethod for user with email: {UserEmail}", userEmail);
        }

        public async Task RegisterUserAsync(string name, string userEmail, string password, Role role)
        {
            var user = await _userRepository.GetUserByEmailAsync(userEmail).ConfigureAwait(false);
            if (user != null)
                throw new InvalidOperationException("The email address is already in use. Please try with a different email.");
            var (encryptedPassword, encryptedPasswordKey) = _passwordEncryption.EncryptionPassword(password);
            AuthMethodModel authMethodModel = new()
            {
                PasswordKey = encryptedPasswordKey,
                Password = encryptedPassword,
                Provider = AuthProvider.Local,
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