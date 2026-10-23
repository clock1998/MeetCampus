using MeetCampus.Data;
using MeetCampus.Features.Auth.Infrastructure;
using MeetCampus.Features.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

var adminEmail = builder.Configuration["BootstrapAdmin:Email"];
if (string.IsNullOrWhiteSpace(adminEmail))
    throw new InvalidOperationException("Bootstrap administrator email is required in BootstrapAdmin:Email.");

var adminPassword = builder.Configuration["BootstrapAdmin:Password"];
if (string.IsNullOrWhiteSpace(adminPassword))
    throw new InvalidOperationException("Bootstrap administrator password is required in BootstrapAdmin:Password.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddMeetCampusIdentityCore();
builder.Services.AddScoped<InitialAdministratorProvisioner>();

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
if (pendingMigrations.Any())
    throw new InvalidOperationException("Apply all database migrations before bootstrapping the administrator.");

var provisioner = scope.ServiceProvider.GetRequiredService<InitialAdministratorProvisioner>();
await provisioner.ProvisionAsync(adminEmail, adminPassword);

Console.WriteLine("Initial administrator provisioning completed.");
