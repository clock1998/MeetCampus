using System.Text;
using System.Text.Encodings.Web;
using MeetCampus.Data;
using MeetCampus.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Features.Auth.Services;

public sealed class PasswordRecoveryService(
    UserManager<ApplicationUser> userManager,
    IEmailSender<ApplicationUser> emailSender)
{
    public async Task RequestResetAsync(
        string email,
        Func<string, string> buildResetUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentNullException.ThrowIfNull(buildResetUrl);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return;
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = buildResetUrl(encodedCode);
        await emailSender.SendPasswordResetLinkAsync(user, email, HtmlEncoder.Default.Encode(callbackUrl));
    }

    public async Task<PasswordResetResult> ResetAsync(
        string email,
        string code,
        string newPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return PasswordResetResult.NotFound();
        }

        var result = await userManager.ResetPasswordAsync(user, code, newPassword);
        return result.Succeeded
            ? PasswordResetResult.Success()
            : PasswordResetResult.Failed(result.Errors.Select(error => error.Description));
    }

    public static string DecodeResetCode(string encodedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedCode);
        return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedCode));
    }
}
