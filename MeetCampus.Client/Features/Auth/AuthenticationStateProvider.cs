using System.Security.Claims;
using MeetCampus.Contracts.Features.Auth.Contracts;
using MeetCampus.HttpClients.Features.Auth;
using MeetCampus.HttpClients.Infrastructure.Http;
using Microsoft.AspNetCore.Components.Authorization;

namespace MeetCampus.Client.Features.Auth;

internal sealed class ClientAuthenticationStateProvider(
    IAuthClient authClient) : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());
    private AuthenticationState? currentState;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (currentState is not null)
        {
            return currentState;
        }

        try
        {
            currentState = CreateState(await authClient.GetCurrentUserAsync());
        }
        catch (ApiClientException)
        {
            currentState = new AuthenticationState(Anonymous);
        }

        return currentState;
    }

    public async Task RefreshAsync()
    {
        currentState = CreateState(await authClient.GetCurrentUserAsync());
        NotifyAuthenticationStateChanged(Task.FromResult(currentState));
    }

    private static AuthenticationState CreateState(AuthUserResponse user)
    {
        if (!user.IsAuthenticated)
        {
            return new AuthenticationState(Anonymous);
        }

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(user.UserId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId));
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.Name, user.Email));
        }

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "Identity.Application")));
    }
}
