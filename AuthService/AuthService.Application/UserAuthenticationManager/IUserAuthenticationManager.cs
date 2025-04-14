using Common.Enums;

namespace AuthService.Application.UserAuthenticationManager
{
    public interface IUserAuthenticationManager
    {
        Task RegisterUserGoogleAsync(string name, string userEmail, string providerUserId, Role role);
        Task LoginUserGoogleAsync(string userEmail, string providerUserId);
    }
}