using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;

namespace MeetCampus.Features.Auth.Services;

public sealed class RegistrationService(
    UserManager<ApplicationUser> userManager,
    IUserStore<ApplicationUser> userStore,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender<ApplicationUser> emailSender,
    ILogger<RegistrationService> logger,
    ApplicationDbContext dbContext)
{
    public async Task<RegistrationResult> RegisterAsync(
        RegisterUserRequest request,
        Func<string, string, string> buildConfirmationUrl)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(buildConfirmationUrl);

        if (!await IsEmailDomainAllowedAsync(request.Email, CancellationToken.None))
        {
            return RegistrationResult.Failed(new[] { new IdentityErrorDescriber().InvalidEmail(request.Email) });
        }

        var user = CreateUser();

        await userStore.SetUserNameAsync(user, request.Email, CancellationToken.None);
        var emailStore = GetEmailStore();
        await emailStore.SetEmailAsync(user, request.Email, CancellationToken.None);

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return RegistrationResult.Failed(createResult.Errors);
        }

        var roleResult = await userManager.AddToRoleAsync(user, IdentityRoles.Student);
        if (!roleResult.Succeeded)
        {
            return RegistrationResult.Failed(roleResult.Errors);
        }

        dbContext.AccountLifecycles.Add(new AccountLifecycle
        {
            ApplicationUserId = user.Id,
            Status = AccountStatus.Active,
            DiscoverabilityOptIn = false
        });
        await dbContext.SaveChangesAsync(CancellationToken.None);

        logger.LogInformation("User created a new account with password.");

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = buildConfirmationUrl(userId, encodedCode);

        await emailSender.SendConfirmationLinkAsync(user, request.Email, HtmlEncoder.Default.Encode(callbackUrl));

        if (userManager.Options.SignIn.RequireConfirmedAccount)
        {
            return RegistrationResult.Success(requiresConfirmedAccount: true);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        return RegistrationResult.Success(requiresConfirmedAccount: false);
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor.");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }

        return (IUserEmailStore<ApplicationUser>)userStore;
    }

    private async Task<bool> IsEmailDomainAllowedAsync(string email, CancellationToken cancellationToken)
    {
        var separatorIndex = email.LastIndexOf('@');
        if (separatorIndex <= 0 || separatorIndex == email.Length - 1)
        {
            return false;
        }

        var originalDomain = email[(separatorIndex + 1)..].Trim().TrimEnd('.');
        var domain = originalDomain.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(domain) || !string.Equals(originalDomain, domain, StringComparison.Ordinal))
        {
            return false;
        }

        return await dbContext.AllowedEmailDomains
            .AsNoTracking()
            .AnyAsync(item => item.IsEnabled && item.Domain == domain, cancellationToken);
    }

}
