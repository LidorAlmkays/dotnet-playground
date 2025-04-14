using AuthService.Domain.Models;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Infrastructure.AuthMethodRepository
{
    public interface IAuthMethodRepository
    {
        Task<AuthMethodModel> GetAuthMethodAsync(Guid userId, AuthProvider provider);
        Task InsertAuthMethodAsync(AuthMethodModel authMethod);
        Task UpdateAuthMethodAsync(Guid userId, AuthProvider provider, AuthMethodModel newAuthData);
        Task RemoveAuthMethodAsync(Guid userId, AuthProvider provider);
    }
}