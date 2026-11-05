using MeetCampus.Contracts.Features.Auth;
using MeetCampus.Data.User;
using MeetCampus.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace MeetCampus.Features.Account;

[ApiController]
[Route("api/auth/account")]
[Authorize]
public sealed class AccountController(
    AccountEligibilityService accountEligibilityService,
    AccountLifecycleService accountLifecycleService,
    AccountService accountService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("eligibility")]
    public async Task<ActionResult<AccountEligibilityResponse>> GetEligibilityAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var eligibility = await accountEligibilityService.GetEligibilityAsync(user.Id, cancellationToken);
        return eligibility is null ? Unauthorized() : Ok(eligibility);
    }

    [HttpPost("lifecycle")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> UpdateLifecycleAsync(
        [FromBody] UpdateAccountLifecycleRequest request,
        CancellationToken cancellationToken) =>
        Ok(await accountLifecycleService.UpdateAsync(User, request, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<AccountProfileResponse>> GetAccountAsync()
    {
        var response = await accountService.GetProfileAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPut("phone")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> UpdatePhoneAsync(
        [FromBody] UpdatePhoneNumberRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.UpdatePhoneNumberAsync(User, request.PhoneNumber));
    }

    [HttpPost("password")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.ChangePasswordAsync(User, request.CurrentPassword, request.NewPassword));
    }

    [HttpPost("set-password")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> SetPasswordAsync(
        [FromBody] SetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Ok(await accountService.SetPasswordAsync(User, request.NewPassword));
    }

    [HttpPost("email/verification")]
    [EnableRateLimiting("verification")]
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

    [HttpPost("email")]
    [EnableRateLimiting("verification")]
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

    [HttpGet("2fa")]
    public async Task<ActionResult<TwoFactorStatusResponse>> GetTwoFactorStatusAsync()
    {
        var response = await accountService.GetTwoFactorStatusAsync(User, HttpContext);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("2fa/forget-browser")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> ForgetTwoFactorBrowserAsync() =>
        Ok(await accountService.ForgetTwoFactorBrowserAsync());

    [HttpGet("authenticator")]
    public async Task<ActionResult<AuthenticatorSetupResponse>> GetAuthenticatorSetupAsync()
    {
        var response = await accountService.GetAuthenticatorSetupAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("authenticator")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodesResponse>> VerifyAuthenticatorAsync(
        [FromBody] VerifyAuthenticatorRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = await accountService.VerifyAuthenticatorAsync(User, request.Code);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("2fa/disable")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<AccountOperationResponse>> DisableTwoFactorAsync() =>
        Ok(await accountService.DisableTwoFactorAsync(User));

    [HttpPost("2fa/recovery-codes")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<RecoveryCodesResponse>> GenerateRecoveryCodesAsync()
    {
        var response = await accountService.GenerateRecoveryCodesAsync(User);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("delete")]
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
