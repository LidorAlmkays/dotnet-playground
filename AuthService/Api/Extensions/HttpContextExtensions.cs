using AuthService.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthService.Api.Extensions
{
    public static class HttpContextExtensions
    {
        private static string GetClaimValueOrThrow(ClaimsPrincipal principal, string claimType, string claimName)
        {
            var claim = principal?.Identities
                .SelectMany(i => i.Claims)
                .FirstOrDefault(c => c.Type == claimType || c.Type == claimName)?.Value;

            if (string.IsNullOrEmpty(claim))
                throw new ArgumentNullException(claimType, $"{claimType} claim is missing.");

            return claim;
        }

        public static async Task<UserContext> GetGoogleUserContextAsync(this HttpContext httpContext)
        {
            var result = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            if (!result.Succeeded || result.Principal == null)
            {
                throw new UnauthorizedAccessException("Authentication failed or no principal found.");
            }

            var principal = result.Principal;

            // Extract claims with the helper method
            var nameIdentifier = GetClaimValueOrThrow(principal, ClaimTypes.NameIdentifier, "sub");
            var name = GetClaimValueOrThrow(principal, ClaimTypes.Name, "name");
            var givenName = GetClaimValueOrThrow(principal, ClaimTypes.GivenName, "given_name");
            var email = GetClaimValueOrThrow(principal, ClaimTypes.Email, "email");

            return new UserContext
            {
                NameIdentifier = nameIdentifier,
                Name = name,
                GivenName = givenName,
                Email = email
            };
        }
    }
}