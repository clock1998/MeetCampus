using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MeetCampus.IntegrationTests.Features.Auth;

public sealed class CookieAuthenticationTests
{
    [Fact]
    public async Task WhenAnonymousUserRequestsProtectedProfileThenApplicationCookieChallengeIsReturned()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("ConnectionStrings:DefaultConnection", "Host=127.0.0.1;Database=unused;Username=unused;Password=unused"));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/api/profile/setup", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.OriginalString, StringComparison.OrdinalIgnoreCase);
    }
}
