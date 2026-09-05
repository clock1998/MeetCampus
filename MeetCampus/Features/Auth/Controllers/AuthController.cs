using System.Security.Claims;
using MeetCampus.Contracts.Features.Auth.Contracts;
using MeetCampus.Data;
using MeetCampus.Features.Auth.Models;
using MeetCampus.Features.Auth.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Features.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthService authService,
    RegistrationService registrationService,
    PasswordRecoveryService passwordRecoveryService,
    EmailConfirmationService emailConfirmationService,
    AccountService accountService,
    SignInManager<ApplicationUser> signInManager,
    IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet("antiforgery")]
    [AllowAnonymous]
    public ActionResult<AntiforgeryResponse> GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new AntiforgeryResponse(tokens.RequestToken ?? string.Empty));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<LoginResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await authService.LoginAsync(new LoginAttempt(
            request.Email,
            request.Password,
            request.RememberMe,
            PasskeyCredentialJson: null,
            PasskeyError: null));

        return Ok(new LoginResponse(
            result.Succeeded,
            result.RequiresTwoFactor,
            result.IsLockedOut,
            result.ErrorMessage));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RegisterResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await registrationService.RegisterAsync(
            new RegisterUserRequest(request.Email, request.Password),
            (userId, code) => BuildClientUrl("/auth/confirm-email", new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["code"] = code,
            }));

        return Ok(new RegisterResponse(
            result.Succeeded,
            result.RequiresConfirmedAccount,
            result.Errors?.Select(error => error.Description).ToArray()));
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return Ok(new AuthOperationResponse(true));
    }

    [HttpGet("me")]
    [AllowAnonymous]
    public ActionResult<AuthUserResponse> GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Ok(new AuthUserResponse(false, null, null, []));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
        return Ok(new AuthUserResponse(true, userId, email, roles));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await passwordRecoveryService.RequestResetAsync(
            request.Email,
            code => BuildClientUrl("/auth/reset-password", new Dictionary<string, object?>
            {
                ["code"] = code,
            }));

        return Ok(new AuthOperationResponse(true));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await passwordRecoveryService.ResetAsync(
            request.Email,
            PasswordRecoveryService.DecodeResetCode(request.Code),
            request.NewPassword);

        return Ok(new ResetPasswordResponse(
            result.Succeeded || result.UserNotFound,
            result.Errors));
    }

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> ConfirmEmailAsync(
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = request.Email is null
            ? await emailConfirmationService.ConfirmEmailAsync(request.UserId, request.Code)
            : await emailConfirmationService.ConfirmEmailChangeAsync(request.UserId, request.Email, request.Code);

        return Ok(new AuthOperationResponse(result.Succeeded));
    }

    [HttpPost("login/2fa")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<TwoFactorLoginResponse>> LoginWithTwoFactorAsync(
        [FromBody] TwoFactorLoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.LoginWithAuthenticatorAsync(request.Code, request.RememberMe, request.RememberMachine));
    }

    [HttpPost("login/recovery-code")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodeLoginResponse>> LoginWithRecoveryCodeAsync(
        [FromBody] RecoveryCodeLoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.LoginWithRecoveryCodeAsync(request.RecoveryCode));
    }

    [HttpGet("account")]
    [Authorize]
    public async Task<ActionResult<AccountProfileResponse>> GetAccountAsync()
    {
        var response = await accountService.GetProfileAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPut("account/phone")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> UpdatePhoneAsync(
        [FromBody] UpdatePhoneNumberRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.UpdatePhoneNumberAsync(User, request.PhoneNumber));
    }

    [HttpPost("account/password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.ChangePasswordAsync(User, request.CurrentPassword, request.NewPassword));
    }

    [HttpPost("account/set-password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> SetPasswordAsync(
        [FromBody] SetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.SetPasswordAsync(User, request.NewPassword));
    }

    [HttpPost("account/email")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> ChangeEmailAsync(
        [FromBody] ChangeEmailRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.RequestEmailChangeAsync(
            User,
            request.NewEmail,
            (userId, code) => BuildClientUrl("/auth/confirm-email-change", new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["email"] = request.NewEmail,
                ["code"] = code,
            })));
    }

    [HttpPost("account/email/verification")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> SendEmailVerificationAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.SendEmailVerificationAsync(
            User,
            (userId, code) => BuildClientUrl("/auth/confirm-email", new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["code"] = code,
            })));
    }

    [HttpGet("account/2fa")]
    [Authorize]
    public async Task<ActionResult<TwoFactorStatusResponse>> GetTwoFactorStatusAsync()
    {
        var response = await accountService.GetTwoFactorStatusAsync(User, HttpContext);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("account/2fa/forget-browser")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> ForgetTwoFactorBrowserAsync()
    {
        return Ok(await accountService.ForgetTwoFactorBrowserAsync());
    }

    [HttpGet("account/authenticator")]
    [Authorize]
    public async Task<ActionResult<AuthenticatorSetupResponse>> GetAuthenticatorSetupAsync()
    {
        var response = await accountService.GetAuthenticatorSetupAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("account/authenticator")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodesResponse>> VerifyAuthenticatorAsync(
        [FromBody] VerifyAuthenticatorRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = await accountService.VerifyAuthenticatorAsync(User, request.Code);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("account/2fa/disable")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> DisableTwoFactorAsync()
    {
        return Ok(await accountService.DisableTwoFactorAsync(User));
    }

    [HttpPost("account/2fa/recovery-codes")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodesResponse>> GenerateRecoveryCodesAsync()
    {
        var response = await accountService.GenerateRecoveryCodesAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("account/delete")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> DeleteAccountAsync(
        [FromBody] DeleteAccountRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.DeleteAccountAsync(User, request.Password));
    }

    private string BuildClientUrl(string path, Dictionary<string, object?> parameters)
    {
        var baseUri = $"{Request.Scheme}://{Request.Host}{path}";
        return QueryHelpers.AddQueryString(
            baseUri,
            parameters.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString()));
    }
}
