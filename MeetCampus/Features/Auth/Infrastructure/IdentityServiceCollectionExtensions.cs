using MeetCampus.Data;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MeetCampus.Features.Auth.Infrastructure;

public static class IdentityServiceCollectionExtensions
{
    public static IdentityBuilder AddMeetCampusIdentityCore(this IServiceCollection services)
    {
        return services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
    }
}
