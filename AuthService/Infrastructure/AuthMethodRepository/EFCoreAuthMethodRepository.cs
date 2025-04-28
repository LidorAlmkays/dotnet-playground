using AuthService.Domain.Models;
using Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.AuthMethodRepository
{
    public class EFCoreAuthMethodRepository(AppDbContext dbContext) : IAuthMethodRepository
    {
        private readonly AppDbContext _dbContext = dbContext;
        // Get AuthMethod by user ID
        public async Task<AuthMethodModel> GetAuthMethodAsync(Guid userId, AuthProvider provider)
        {
            var authMethod = await _dbContext.AuthMethods
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == provider)
                .ConfigureAwait(false);

            ArgumentNullException.ThrowIfNull(authMethod, "Auth method not found for the given user and provider");
            return authMethod;
        }


        // Insert a new AuthMethod for a user
        public async Task InsertAuthMethodAsync(AuthMethodModel authMethod)
        {
            var exists = await _dbContext.AuthMethods
                .AnyAsync(a => a.UserId == authMethod.UserId && a.Provider == authMethod.Provider)
                .ConfigureAwait(false);

            if (exists)
            {
                throw new InvalidOperationException("Auth method already exists for this user and provider.");
            }

            await _dbContext.AuthMethods.AddAsync(authMethod).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        // Update an AuthMethod (e.g., changing auth provider or password)
        public async Task UpdateAuthMethodAsync(Guid userId, AuthProvider provider, AuthMethodModel newAuthData)
        {
            var authMethod = await _dbContext.AuthMethods
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Provider == provider)
                .ConfigureAwait(false);

            ArgumentNullException.ThrowIfNull(authMethod, "Auth method not found for the given user and provider.");

            _dbContext.Entry(authMethod).CurrentValues.SetValues(newAuthData);
            authMethod.UserId = userId;
            authMethod.Provider = provider; // preserve primary keys or important identifiers
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveAuthMethodAsync(Guid userId, AuthProvider provider)
        {
            var authMethod = await GetAuthMethodAsync(userId, provider).ConfigureAwait(false);
            _dbContext.AuthMethods.Remove(authMethod);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

    }
}