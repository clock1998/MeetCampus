using System.Text;
using System.Text.Encodings.Web;
using MeetCampus.Data;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Features.Auth.Services;

public sealed class EmailConfirmationService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender<ApplicationUser> emailSender,
    ApplicationDbContext dbContext)
{
    public async Task<EmailConfirmationResult> ConfirmEmailAsync(Guid userId, string encodedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedCode);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return EmailConfirmationResult.NotFound();
        }

        var result = await userManager.ConfirmEmailAsync(user, DecodeToken(encodedCode));
        if (result.Succeeded)
        {
            var lifecycle = await dbContext.AccountLifecycles.FindAsync(userId);
            if (lifecycle is not null)
            {
                lifecycle.EmailVerifiedAt = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }

        return result.Succeeded ? EmailConfirmationResult.Success() : EmailConfirmationResult.Failed();
    }

    public async Task<EmailConfirmationResult> ConfirmEmailChangeAsync(
        Guid userId,
        string email,
        string encodedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedCode);

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return EmailConfirmationResult.NotFound();
        }

        var result = await userManager.ChangeEmailAsync(user, email, DecodeToken(encodedCode));
        if (!result.Succeeded)
        {
            return EmailConfirmationResult.Failed();
        }

        var setUserNameResult = await userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            return EmailConfirmationResult.Failed();
        }

        var lifecycle = await dbContext.AccountLifecycles.FindAsync(userId);
        if (lifecycle is not null)
        {
            lifecycle.EmailVerifiedAt = null;
            await dbContext.SaveChangesAsync();
        }

        await signInManager.RefreshSignInAsync(user);
        return EmailConfirmationResult.Success();
    }

    public async Task ResendConfirmationAsync(
        string email,
        Func<string, string, string> buildConfirmationUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentNullException.ThrowIfNull(buildConfirmationUrl);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return;
        }

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = buildConfirmationUrl(userId, encodedCode);
        await emailSender.SendConfirmationLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
    }

    private static string DecodeToken(string encodedToken) =>
        Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
}
