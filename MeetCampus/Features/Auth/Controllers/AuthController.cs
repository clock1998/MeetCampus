using System.Security.Claims;
using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data.User;
using MeetCampus.Features.Auth.Models;
using MeetCampus.Features.Auth.Services;
using MeetCampus.Shared;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MeetCampus.Features.Auth.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthService authService,
    RegistrationService registrationService,
    PasswordRecoveryService passwordRecoveryService,
    EmailConfirmationService emailConfirmationService,
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
    [EnableRateLimiting("verification")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RegisterResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await registrationService.RegisterAsync(
            new RegisterUserRequest(request.Email, request.Password),
            (userId, code) => ClientUrlBuilder.Build(Request.Scheme, Request.Host, "/auth/confirm-email", new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["code"] = code,
            }));

        return Ok(new RegisterResponse(true, true, null));
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return Ok(new AuthOperationResponse(true));
    }

    [HttpPost("resend-confirmation")]
    [EnableRateLimiting("verification")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> ResendConfirmationAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await emailConfirmationService.ResendConfirmationAsync(
            request.Email,
            (userId, code) => ClientUrlBuilder.Build(Request.Scheme, Request.Host, "/auth/confirm-email", new Dictionary<string, object?>
            {
                ["userId"] = userId,
                ["code"] = code,
            }));

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
        return Guid.TryParse(userId, out var parsedUserId)
            ? Ok(new AuthUserResponse(true, parsedUserId, email, roles))
            : Unauthorized();
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("verification")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AuthOperationResponse>> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await passwordRecoveryService.RequestResetAsync(
            request.Email,
            code => ClientUrlBuilder.Build(Request.Scheme, Request.Host, "/auth/reset-password", new Dictionary<string, object?>
            {
                ["code"] = code,
            }));

        return Ok(new AuthOperationResponse(true));
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("verification")]
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
            result.Succeeded,
            result.Succeeded ? null : ["The reset link is invalid or expired. Request a new reset link and try again."]));
    }

    [HttpPost("confirm-email")]
    [EnableRateLimiting("verification")]
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
        return Ok(await authService.LoginWithAuthenticatorAsync(request.Code, request.RememberMe, request.RememberMachine));
    }

    [HttpPost("login/recovery-code")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodeLoginResponse>> LoginWithRecoveryCodeAsync(
        [FromBody] RecoveryCodeLoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await authService.LoginWithRecoveryCodeAsync(request.RecoveryCode));
    }

}
