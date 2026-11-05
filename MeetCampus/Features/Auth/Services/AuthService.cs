using MeetCampus.Data.User;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Features.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace MeetCampus.Features.Auth.Services;

public sealed class AuthService(
    SignInManager<ApplicationUser> signInManager,
    ILogger<AuthService> logger)
{
    public async Task<LoginResult> LoginAsync(LoginAttempt attempt)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        if (!string.IsNullOrEmpty(attempt.PasskeyError))
        {
            return LoginResult.Failed($"Error: {attempt.PasskeyError}");
        }

        SignInResult result;
        if (!string.IsNullOrEmpty(attempt.PasskeyCredentialJson))
        {
            result = await signInManager.PasskeySignInAsync(attempt.PasskeyCredentialJson);
        }
        else
        {
            result = await signInManager.PasswordSignInAsync(
                attempt.Email,
                attempt.Password,
                attempt.RememberMe,
                lockoutOnFailure: false);
        }

        if (result.Succeeded)
        {
            logger.LogInformation("User logged in.");
            return LoginResult.Success();
        }

        if (result.RequiresTwoFactor)
        {
            return LoginResult.TwoFactorRequired();
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked out.");
            return LoginResult.LockedOut();
        }

        return LoginResult.Failed("Error: Invalid login attempt.");
    }

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
}
