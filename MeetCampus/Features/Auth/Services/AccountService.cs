using System.Text;
using System.Security.Claims;
using MeetCampus.Contracts.Features.Auth.Contracts;
using MeetCampus.Data;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Features.Auth.Services;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender<ApplicationUser> emailSender)
{
    public async Task<TwoFactorLoginResponse> LoginWithAuthenticatorAsync(
        string code,
        bool rememberMe,
        bool rememberMachine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var normalizedCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(normalizedCode, rememberMe, rememberMachine);
        return new TwoFactorLoginResponse(result.Succeeded, result.IsLockedOut, result.Succeeded ? null : "Invalid authenticator code.");
    }

    public async Task<RecoveryCodeLoginResponse> LoginWithRecoveryCodeAsync(string recoveryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recoveryCode);
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode.Replace(" ", string.Empty));
        return new RecoveryCodeLoginResponse(result.Succeeded, result.IsLockedOut, result.Succeeded ? null : "Invalid recovery code entered.");
    }

    public async Task<AccountProfileResponse?> GetProfileAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        return new AccountProfileResponse(
            await userManager.GetEmailAsync(user) ?? string.Empty,
            await userManager.GetPhoneNumberAsync(user),
            await userManager.IsEmailConfirmedAsync(user),
            await userManager.HasPasswordAsync(user));
    }

    public async Task<AccountOperationResponse> UpdatePhoneNumberAsync(ClaimsPrincipal principal, string? phoneNumber)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var result = await userManager.SetPhoneNumberAsync(user, phoneNumber);
        if (!result.Succeeded)
        {
            return ToResponse(result.Errors);
        }

        await signInManager.RefreshSignInAsync(user);
        return new AccountOperationResponse(true);
    }

    public async Task<AccountOperationResponse> ChangePasswordAsync(ClaimsPrincipal principal, string currentPassword, string newPassword)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return ToResponse(result.Errors);
        }

        await signInManager.RefreshSignInAsync(user);
        return new AccountOperationResponse(true);
    }

    public async Task<AccountOperationResponse> SetPasswordAsync(ClaimsPrincipal principal, string newPassword)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var result = await userManager.AddPasswordAsync(user, newPassword);
        if (!result.Succeeded)
        {
            return ToResponse(result.Errors);
        }

        await signInManager.RefreshSignInAsync(user);
        return new AccountOperationResponse(true);
    }

    public async Task<AccountOperationResponse> RequestEmailChangeAsync(
        ClaimsPrincipal principal,
        string newEmail,
        Func<string, string, string> buildConfirmationUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
        ArgumentNullException.ThrowIfNull(buildConfirmationUrl);
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        await emailSender.SendConfirmationLinkAsync(user, newEmail, buildConfirmationUrl(userId, encodedCode));
        return new AccountOperationResponse(true);
    }

    public async Task<AccountOperationResponse> SendEmailVerificationAsync(
        ClaimsPrincipal principal,
        Func<string, string, string> buildConfirmationUrl)
    {
        ArgumentNullException.ThrowIfNull(buildConfirmationUrl);
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var email = await userManager.GetEmailAsync(user);
        if (string.IsNullOrWhiteSpace(email))
        {
            return new AccountOperationResponse(false, ["User email was not found."]);
        }

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        await emailSender.SendConfirmationLinkAsync(user, email, buildConfirmationUrl(userId, encodedCode));
        return new AccountOperationResponse(true);
    }

    public async Task<TwoFactorStatusResponse?> GetTwoFactorStatusAsync(ClaimsPrincipal principal, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        return new TwoFactorStatusResponse(
            httpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true,
            await userManager.GetTwoFactorEnabledAsync(user),
            await userManager.GetAuthenticatorKeyAsync(user) is not null,
            await signInManager.IsTwoFactorClientRememberedAsync(user),
            await userManager.CountRecoveryCodesAsync(user));
    }

    public async Task<AccountOperationResponse> ForgetTwoFactorBrowserAsync()
    {
        await signInManager.ForgetTwoFactorClientAsync();
        return new AccountOperationResponse(true);
    }

    public async Task<AuthenticatorSetupResponse?> GetAuthenticatorSetupAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await userManager.GetEmailAsync(user) ?? string.Empty;
        var formattedKey = FormatKey(key!);
        var uri = string.Format(
            "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
            Uri.EscapeDataString("MeetCampus"),
            Uri.EscapeDataString(email),
            key);
        return new AuthenticatorSetupResponse(formattedKey, uri);
    }

    public async Task<RecoveryCodesResponse?> VerifyAuthenticatorAsync(ClaimsPrincipal principal, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        var normalizedCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var valid = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, normalizedCode);
        if (!valid)
        {
            return new RecoveryCodesResponse([]);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        if (await userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            return new RecoveryCodesResponse(recoveryCodes?.ToArray() ?? []);
        }

        return new RecoveryCodesResponse([]);
    }

    public async Task<AccountOperationResponse> DisableTwoFactorAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        return ToResponse(result.Succeeded ? [] : result.Errors);
    }

    public async Task<RecoveryCodesResponse?> GenerateRecoveryCodesAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return null;
        }

        var codes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return new RecoveryCodesResponse(codes?.ToArray() ?? []);
    }

    public async Task<AccountOperationResponse> DeleteAccountAsync(ClaimsPrincipal principal, string? password)
    {
        var user = await GetUserAsync(principal);
        if (user is null)
        {
            return new AccountOperationResponse(false, ["User account was not found."]);
        }

        if (await userManager.HasPasswordAsync(user) && !await userManager.CheckPasswordAsync(user, password))
        {
            return new AccountOperationResponse(false, ["Incorrect password."]);
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return ToResponse(result.Errors);
        }

        await signInManager.SignOutAsync();
        return new AccountOperationResponse(true);
    }

    private async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return await userManager.GetUserAsync(principal);
    }

    private static AccountOperationResponse ToResponse(IEnumerable<IdentityError> errors) =>
        new(false, errors.Select(error => error.Description).ToArray());

    private static string FormatKey(string key)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < key.Length; index += 4)
        {
            if (index > 0)
            {
                builder.Append(' ');
            }

            builder.Append(key.AsSpan(index, Math.Min(4, key.Length - index)));
        }

        return builder.ToString().ToUpperInvariant();
    }
}
