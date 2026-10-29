using MeetCampus;
using MeetCampus.Components;
using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Infrastructure;
using MeetCampus.Features.Auth.Services;
using MeetCampus.Features.Administration.Services;
using MeetCampus.Features.Configuration;
using MeetCampus.Features.Profile.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
builder.Services.AddOptions<MatchingOptions>()
    .Bind(builder.Configuration.GetSection("Matching"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ContactOptions>()
    .Bind(builder.Configuration.GetSection("Contact"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<PrivacyOptions>()
    .Bind(builder.Configuration.GetSection("Privacy"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<SafetyOptions>()
    .Bind(builder.Configuration.GetSection("Safety"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<VerificationOptions>()
    .Bind(builder.Configuration.GetSection("Verification"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddControllersWithViews();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<EmailConfirmationService>();
builder.Services.AddScoped<PasswordRecoveryService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AccountEligibilityService>();
builder.Services.AddScoped<AccountLifecycleService>();
builder.Services.AddScoped<AdministrativeDataService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"))
    .AddPolicy("ModeratorOrAdministrator", policy => policy.RequireRole("Moderator", "Administrator"));
builder.Services.AddRateLimiter(options => options.AddPolicy("verification", context =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue<int>("Verification:MaxRequestsPerHourPerAddress", 5),
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
            AutoReplenishment = true
        })));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddMeetCampusIdentityCore()
    .AddSignInManager()
    .AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Verification:TokenLifetimeMinutes", 30)));

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddLocalization();
builder.Services.AddScoped<UserProfileService>();
builder.Services.AddScoped<ProfileImageService>();
builder.Services.Configure<S3Options>(builder.Configuration.GetSection("S3"));
builder.Services.AddSingleton<S3ProfileImageStorage>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
app.UseRateLimiter();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MeetCampus.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapControllers();

app.Run();

public partial class Program;
