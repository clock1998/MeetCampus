using MeetCampus.Components;
using MeetCampus.Components.Account;
using MeetCampus.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddLocalization();

var app = builder.Build();

// Apply pending migrations and seed Identity data.
using (var scope = app.Services.CreateScope())
{
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await dbContext.Database.MigrateAsync();
    await SeedIdentityDataAsync(configuration, roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
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

app.Run();

static async Task SeedIdentityDataAsync(
    IConfiguration configuration,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
{
    ArgumentNullException.ThrowIfNull(configuration);
    ArgumentNullException.ThrowIfNull(roleManager);
    ArgumentNullException.ThrowIfNull(userManager);

    var requiredRoles = new[] { IdentityRoles.Admin, IdentityRoles.PowerUser, IdentityRoles.User };
    foreach (var roleName in requiredRoles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!createRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {string.Join(", ", createRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    var adminEmail = configuration["Seed:AdminUser:Email"];
    var adminUserName = configuration["Seed:AdminUser:UserName"];
    var adminPassword = configuration["Seed:AdminUser:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail))
    {
        throw new InvalidOperationException("Seed admin email is missing. Configure 'Seed:AdminUser:Email'.");
    }

    if (string.IsNullOrWhiteSpace(adminUserName))
    {
        throw new InvalidOperationException("Seed admin user name is missing. Configure 'Seed:AdminUser:UserName'.");
    }

    if (string.IsNullOrWhiteSpace(adminPassword))
    {
        throw new InvalidOperationException("Seed admin password is missing. Configure 'Seed:AdminUser:Password'.");
    }

    const string adminUserId = "20000000-0000-0000-0000-000000000001";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            Id = adminUserId,
            UserName = adminUserName,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createUserResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to seed admin user: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, IdentityRoles.Admin))
    {
        var addRoleResult = await userManager.AddToRoleAsync(adminUser, IdentityRoles.Admin);
        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add admin user to role '{IdentityRoles.Admin}': {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
        }
    }
}
