using MeetCampus.Contracts.Features.Auth.Contracts;

namespace MeetCampus.HttpClients.Features.Auth;

public interface IAuthClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);

    Task<AuthUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);

    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);
}