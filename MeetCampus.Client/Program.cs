using System.Globalization;
using MeetCampus.Shared.Services.Profile;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddLocalization();
builder.Services.AddHttpClient<IProfileSetupClient, ProfileSetupClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

var host = builder.Build();

// Set the culture from localStorage before the app renders, so all
// IStringLocalizer<T> instances serve the correct language from the first render.
var js = host.Services.GetRequiredService<IJSRuntime>();
var savedCulture = await js.InvokeAsync<string?>("localStorage.getItem", "language");
if (!string.IsNullOrWhiteSpace(savedCulture))
{
    try
    {
        var culture = new CultureInfo(savedCulture);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
    catch (CultureNotFoundException)
    {
        // Unrecognised culture tag — fall back to browser default
    }
}

await host.RunAsync();
