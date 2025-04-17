using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.UserRepository
{
    public class EFCoreUserRepository(AppDbContext dbContext) : IUserRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email).ConfigureAwait(false);
        }
        public async Task DeleteUserByEmailAsync(string email)
        {
            var user = await GetUserByEmailAsync(email).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user, "Cant delete user, user wasn't found.");

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task InsertUserAsync(UserModel user)
        {
            var exists = await _dbContext.Users
                .AnyAsync(u => u.Email == user.Email)
                .ConfigureAwait(false);

            if (exists)
                throw new InvalidOperationException("A user with this email already exists.");

            await _dbContext.Users.AddAsync(user).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task UpdateUserByEmailAsync(string email, UserModel newUserData)
        {
            var user = await GetUserByEmailAsync(email).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user, "Cant update user, User wasn't found.");

            if (user.Email != newUserData.Email)
            {
                var emailTaken = await _dbContext.Users
                    .AnyAsync(u => u.Email == newUserData.Email && u.Id != user.Id)
                    .ConfigureAwait(false);

                if (emailTaken)
                    throw new InvalidOperationException("New email is already taken.");
            }

            _dbContext.Entry(user).CurrentValues.SetValues(newUserData);
            user.Id = user.Id;

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task DeleteUserByIdAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user, "Cant delete user, user wasn't found.");

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateUserByIdAsync(Guid userId, UserModel newUserData)
        {
            var user = await GetUserByIdAsync(userId).ConfigureAwait(false);
            ArgumentNullException.ThrowIfNull(user, "Cant update user, User wasn't found.");

            // Check for email conflict if changing
            if (user.Email != newUserData.Email)
            {
                var emailTaken = await _dbContext.Users
                    .AnyAsync(u => u.Email == newUserData.Email && u.Id != userId)
                    .ConfigureAwait(false);

                if (emailTaken)
                    throw new InvalidOperationException("Email already in use by another user.");
            }

            _dbContext.Entry(user).CurrentValues.SetValues(newUserData);

            // Preserve ID
            user.Id = userId;

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        public async Task<UserModel?> GetUserByEmailAsync(string email)
        {
            var user = await _dbContext.Users
                .Include(u => u.AuthMethods)
                .FirstOrDefaultAsync(u => u.Email == email)
                .ConfigureAwait(false);

            return user;
        }
        public async Task<UserModel?> GetUserByIdAsync(Guid id)
        {
            var user = await _dbContext.Users
                .Include(u => u.AuthMethods)
                .FirstOrDefaultAsync(u => u.Id == id)
                .ConfigureAwait(false);

            return user;
        }
    }
}