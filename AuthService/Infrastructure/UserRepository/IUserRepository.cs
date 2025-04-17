using AuthService.Domain.Models;

namespace AuthService.Infrastructure.UserRepository
{
    public interface IUserRepository
    {
        Task InsertUserAsync(UserModel user);
        Task<bool> IsEmailTakenAsync(string email);
        Task UpdateUserByEmailAsync(string email, UserModel newUserData);
        Task DeleteUserByEmailAsync(string email);
        Task<UserModel?> GetUserByEmailAsync(string email);
        Task DeleteUserByIdAsync(Guid id);
        Task UpdateUserByIdAsync(Guid userId, UserModel newUserData);
        Task<UserModel?> GetUserByIdAsync(Guid id);
    }
}