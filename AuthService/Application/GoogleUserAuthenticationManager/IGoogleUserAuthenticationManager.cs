using Common.Enums;

namespace AuthService.Application.GoogleUserAuthenticationManager
{
    public interface IGoogleUserAuthenticationManager
    {
        Task RegisterUserGoogleAsync(string name, string userEmail, string providerUserId, Role role);
        Task LoginUserGoogleAsync(string userEmail, string providerUserId);
    }
}