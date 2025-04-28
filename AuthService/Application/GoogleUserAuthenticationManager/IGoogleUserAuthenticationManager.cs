using AuthService.Domain.Models;
using Common.Enums;

namespace AuthService.Application.GoogleUserAuthenticationManager
{
    public interface IGoogleUserAuthenticationManager
    {
        Task RegisterUserGoogleAsync(string name, string userEmail, string providerUserId, Role role);
        Task<Guid> ValidateUserGoogleLoginAsync(string userEmail, string providerUserId);
    }
}