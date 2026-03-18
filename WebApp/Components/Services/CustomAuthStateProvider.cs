using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace WebApp.Components.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated user

            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
