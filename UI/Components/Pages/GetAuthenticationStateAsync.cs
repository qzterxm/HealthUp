using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DataAccess.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using UI.ApiClients;
using WebApplication1.Interfaces;

namespace UI;

  public class CustomAuthStateProvider(CircuitServicesAccesor.CircuitServicesAccesor circuitServicesAccesor) : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var localStorage = circuitServicesAccesor.Service
                    .GetRequiredService<ProtectedLocalStorage>();
                var result = await localStorage.GetAsync<TokenDTO>(
                    nameof(TokenDTO));

                if (!result.Success || result.Value is null)
                    return new AuthenticationState(_anonymous);

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(result.Value.AccessToken);

                var now = DateTime.UtcNow;
                var refreshExpiry = result.Value.RefreshTokenExpiresAt
                    .ToUniversalTime();

                // 1) Access дійсний
                if (jsonToken.ValidTo >= now)
                {
                    var principal = GetClaimsPrincipalFromJwt(
                        result.Value.AccessToken);
                    return new AuthenticationState(principal);
                }

                // // 2) Access сплинув, але refresh ще дійсний
                // if (refreshExpiry >= now)
                // {
                //     var authService = circuitServicesAccesor.Service
                //         .GetRequiredService<IAuthService>();
                //     var newTokens = await authService.RefreshAccessToken(result.Value);
                //     var principal = GetClaimsPrincipalFromJwt(
                //         newTokens.AccessToken);
                //
                //     // Збережемо оновлені токени
                //     await localStorage.SetAsync(
                //         nameof(TokenDTO), newTokens);
                //
                //     return new AuthenticationState(principal);
                // }

                // 3) Обидва токени сплинені — logout
                await localStorage.DeleteAsync(nameof(TokenDTO));
                await MarkUserAsLoggedOut();
                return new AuthenticationState(_anonymous);
            }
            catch
            {
                await MarkUserAsLoggedOut();
                return new AuthenticationState(_anonymous);
            }
        }


        private ClaimsPrincipal GetClaimsPrincipalFromJwt(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);

            var claims = token.Claims.ToList();

            // Створюємо окрему колекцію ролей
            if (!claims.Any(c => c.Type == ClaimTypes.Role) && claims.Any(c => c.Type == "role"))
            {
                var roles = claims
                    .Where(c => c.Type == "role")
                    .Select(c => new Claim(ClaimTypes.Role, c.Value))
                    .ToList();

                claims.AddRange(roles); // Додаємо вже поза межами ітерації
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }

        public async Task MarkUserAsAuthenticated(TokenDTO tokens)
        {
            var LocalStorage = circuitServicesAccesor.Service.GetRequiredService<ProtectedLocalStorage>();
            await LocalStorage.SetAsync(nameof(TokenDTO), tokens);

            var principal = GetClaimsPrincipalFromJwt(tokens.AccessToken);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));

            await SaveUserModelToLocalStorage(tokens.AccessToken);
        }

        public async Task MarkUserAsLoggedOut()
        {
            var LocalStorage = circuitServicesAccesor.Service.GetRequiredService<ProtectedLocalStorage>();
            await LocalStorage.DeleteAsync(nameof(TokenDTO));
            await LocalStorage.DeleteAsync(nameof(UserDTO));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));

            
            
            var navigationManager = circuitServicesAccesor.Service.GetRequiredService<NavigationManager>();
            // navigationManager.NavigateTo("/auth", forceLoad: true);
        }

        public async Task SaveUserModelToLocalStorage(string accessToken)
        {

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var userService = circuitServicesAccesor.Service
                .GetRequiredService<IUserApiClient>();
            var userId = jwtToken.Claims.First(claim => claim.Type == "nameid").Value;
            var user = await userService.GetUserById(Guid.Parse(userId));

            var localStorage = circuitServicesAccesor.Service.GetRequiredService<ProtectedLocalStorage>();
            await localStorage.SetAsync(nameof(UserDTO), user);
        }
    }

