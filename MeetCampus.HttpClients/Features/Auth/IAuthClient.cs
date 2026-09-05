using MeetCampus.Contracts.Features.Auth.Contracts;

namespace MeetCampus.HttpClients.Features.Auth;

public interface IAuthClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<TwoFactorLoginResponse> LoginWithTwoFactorAsync(TwoFactorLoginRequest request, CancellationToken cancellationToken = default);

    Task<RecoveryCodeLoginResponse> LoginWithRecoveryCodeAsync(RecoveryCodeLoginRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<AuthUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<AuthOperationResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    Task<AccountProfileResponse> GetAccountAsync(CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> UpdatePhoneAsync(UpdatePhoneNumberRequest request, CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> SendEmailVerificationAsync(CancellationToken cancellationToken = default);

    Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> ForgetTwoFactorBrowserAsync(CancellationToken cancellationToken = default);

    Task<AuthenticatorSetupResponse> GetAuthenticatorSetupAsync(CancellationToken cancellationToken = default);

    Task<RecoveryCodesResponse> VerifyAuthenticatorAsync(VerifyAuthenticatorRequest request, CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> DisableTwoFactorAsync(CancellationToken cancellationToken = default);

    Task<RecoveryCodesResponse> GenerateRecoveryCodesAsync(CancellationToken cancellationToken = default);

    Task<AccountOperationResponse> DeleteAccountAsync(DeleteAccountRequest request, CancellationToken cancellationToken = default);
}