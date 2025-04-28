using Common.Enums;

namespace AuthService.Application.LocalUserAuthenticationManager
{
    public interface ILocalUserAuthenticationManager
    {
        Task RegisterUserAsync(string name, string userEmail, string password, Role role);
        Task<Guid> ValidateUserLoginAsync(string userEmail, string password);
    }
}