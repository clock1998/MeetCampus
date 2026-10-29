using System.Security.Claims;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data;
using MeetCampus.Data.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetCampus.Features.Auth.Services;

public sealed class AccountLifecycleService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager)
{
    public async Task<AccountOperationResponse> UpdateAsync(
        ClaimsPrincipal principal,
        UpdateAccountLifecycleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["Account was not found."]);
        }

        var lifecycle = await dbContext.AccountLifecycles
            .SingleOrDefaultAsync(item => item.ApplicationUserId == user.Id, cancellationToken);
        if (lifecycle is null)
        {
            return new AccountOperationResponse(false, ["Account status is unavailable."]);
        }

        switch (request.Action)
        {
            case AccountLifecycleAction.Pause when lifecycle.Status == AccountStatus.Active:
                lifecycle.Status = AccountStatus.Paused;
                lifecycle.DeactivatedAt = DateTimeOffset.UtcNow;
                break;
            case AccountLifecycleAction.Activate when lifecycle.Status is AccountStatus.Paused or AccountStatus.Deactivated:
                lifecycle.Status = AccountStatus.Active;
                lifecycle.DeactivatedAt = null;
                break;
            case AccountLifecycleAction.Activate when lifecycle.Status == AccountStatus.Suspended
                && lifecycle.SuspendedUntil.HasValue
                && lifecycle.SuspendedUntil.Value <= DateTimeOffset.UtcNow:
                lifecycle.Status = AccountStatus.Active;
                lifecycle.SuspendedUntil = null;
                break;
            case AccountLifecycleAction.RequestRemoval when lifecycle.Status != AccountStatus.Removed:
                lifecycle.Status = AccountStatus.RemovalPending;
                lifecycle.RemovalRequestedAt = DateTimeOffset.UtcNow;
                break;
            case AccountLifecycleAction.SetDiscoverabilityOptIn when request.DiscoverabilityOptIn.HasValue:
                lifecycle.DiscoverabilityOptIn = request.DiscoverabilityOptIn.Value;
                break;
            default:
                return new AccountOperationResponse(false, ["The requested account status change is not allowed."]);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new AccountOperationResponse(true);
    }
}
