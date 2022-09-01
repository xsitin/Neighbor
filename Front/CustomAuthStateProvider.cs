using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Board.Infrastructure;
using Common.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Board
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ILocalStorageService StorageService { get; set; }

        public CustomAuthStateProvider(ILocalStorageService storageService)
        {
            StorageService = storageService;
        }

        AuthenticationState CreateAnonymousState() => new(new ClaimsPrincipal(new ClaimsIdentity()));

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await StorageService.GetAsync<SecurityToken>(nameof(SecurityToken));
            if (token != null && token.ExpiredAt.ToUniversalTime() > DateTime.Now.ToUniversalTime())
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, token.Username),
                    new Claim("access_token", token.AccessToken),
                    new Claim(ClaimTypes.Expired, token.ExpiredAt.ToUniversalTime().ToLongDateString()),
                    new Claim(ClaimTypes.Role, token.Role)
                };
                var identity = new ClaimsIdentity(claims, "bearer token");
                var principal = new ClaimsPrincipal(identity);
                return new AuthenticationState(principal);
            }
            else
                return CreateAnonymousState();
        }

        public async Task Logout()
        {
            await StorageService.RemoveAsync(nameof(SecurityToken));
            var anonymousState = CreateAnonymousState();
            NotifyAuthenticationStateChanged(Task.FromResult(anonymousState));
        }
    }
}