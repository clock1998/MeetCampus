using MeetCampus.Contracts.Features.Auth.Contracts;
using MeetCampus.HttpClients.Infrastructure.Http;

namespace MeetCampus.HttpClients.Features.Auth;

public sealed class AuthClient(HttpClient httpClient) : BaseApiClient(httpClient), IAuthClient
{
    private const string Endpoint = "/api/auth";
    private string? antiforgeryToken;

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostWithAntiforgeryAsync<LoginRequest, LoginResponse>($"{Endpoint}/login", request, cancellationToken);
    }

    public Task<TwoFactorLoginResponse> LoginWithTwoFactorAsync(TwoFactorLoginRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<TwoFactorLoginRequest, TwoFactorLoginResponse>($"{Endpoint}/login/2fa", request, cancellationToken);

    public Task<RecoveryCodeLoginResponse> LoginWithRecoveryCodeAsync(RecoveryCodeLoginRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<RecoveryCodeLoginRequest, RecoveryCodeLoginResponse>($"{Endpoint}/login/recovery-code", request, cancellationToken);

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostWithAntiforgeryAsync<RegisterRequest, RegisterResponse>($"{Endpoint}/register", request, cancellationToken);
    }

    public Task<AccountProfileResponse> GetAccountAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AccountProfileResponse>($"{Endpoint}/account", cancellationToken);

    public Task<AccountOperationResponse> UpdatePhoneAsync(UpdatePhoneNumberRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<UpdatePhoneNumberRequest, AccountOperationResponse>($"{Endpoint}/account/phone", request, cancellationToken);

    public Task<AccountOperationResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<ChangePasswordRequest, AccountOperationResponse>($"{Endpoint}/account/password", request, cancellationToken);

    public Task<AccountOperationResponse> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<SetPasswordRequest, AccountOperationResponse>($"{Endpoint}/account/set-password", request, cancellationToken);

    public Task<AccountOperationResponse> ChangeEmailAsync(ChangeEmailRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<ChangeEmailRequest, AccountOperationResponse>($"{Endpoint}/account/email", request, cancellationToken);

    public Task<AccountOperationResponse> SendEmailVerificationAsync(CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<object, AccountOperationResponse>($"{Endpoint}/account/email/verification", new { }, cancellationToken);

    public Task<TwoFactorStatusResponse> GetTwoFactorStatusAsync(CancellationToken cancellationToken = default) =>
        GetAsync<TwoFactorStatusResponse>($"{Endpoint}/account/2fa", cancellationToken);

    public Task<AccountOperationResponse> ForgetTwoFactorBrowserAsync(CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<object, AccountOperationResponse>($"{Endpoint}/account/2fa/forget-browser", new { }, cancellationToken);

    public Task<AuthenticatorSetupResponse> GetAuthenticatorSetupAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AuthenticatorSetupResponse>($"{Endpoint}/account/authenticator", cancellationToken);

    public Task<RecoveryCodesResponse> VerifyAuthenticatorAsync(VerifyAuthenticatorRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<VerifyAuthenticatorRequest, RecoveryCodesResponse>($"{Endpoint}/account/authenticator", request, cancellationToken);

    public Task<AccountOperationResponse> DisableTwoFactorAsync(CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<object, AccountOperationResponse>($"{Endpoint}/account/2fa/disable", new { }, cancellationToken);

    public Task<RecoveryCodesResponse> GenerateRecoveryCodesAsync(CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<object, RecoveryCodesResponse>($"{Endpoint}/account/2fa/recovery-codes", new { }, cancellationToken);

    public Task<AccountOperationResponse> DeleteAccountAsync(DeleteAccountRequest request, CancellationToken cancellationToken = default) =>
        PostWithAntiforgeryAsync<DeleteAccountRequest, AccountOperationResponse>($"{Endpoint}/account/delete", request, cancellationToken);

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        using var request = await CreateRequestWithAntiforgeryAsync(HttpMethod.Post, $"{Endpoint}/logout", cancellationToken);
        using var response = await SendAsync(request, cancellationToken);
        await EnsureResponseSuccessAsync(response, cancellationToken);
    }

    public Task<AuthUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AuthUserResponse>($"{Endpoint}/me", cancellationToken);

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await PostWithAntiforgeryAsync<ForgotPasswordRequest, AuthOperationResponse>($"{Endpoint}/forgot-password", request, cancellationToken);
    }

    public Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostWithAntiforgeryAsync<ResetPasswordRequest, ResetPasswordResponse>($"{Endpoint}/reset-password", request, cancellationToken);
    }

    public Task<AuthOperationResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return PostWithAntiforgeryAsync<ConfirmEmailRequest, AuthOperationResponse>($"{Endpoint}/confirm-email", request, cancellationToken);
    }

    private static async Task EnsureResponseSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw ApiClientException.FromStatusCode(response.StatusCode, await response.Content.ReadAsStringAsync(cancellationToken));
    }

    private async Task<TResponse> PostWithAntiforgeryAsync<TRequest, TResponse>(
        string requestUri,
        TRequest requestBody,
        CancellationToken cancellationToken)
    {
        await EnsureAntiforgeryTokenAsync(cancellationToken);
        return await PostAsync<TRequest, TResponse>(
            requestUri,
            requestBody,
            request => request.Headers.Add("RequestVerificationToken", antiforgeryToken),
            cancellationToken);
    }

    private async Task<HttpRequestMessage> CreateRequestWithAntiforgeryAsync(
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken)
    {
        await EnsureAntiforgeryTokenAsync(cancellationToken);
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Add("RequestVerificationToken", antiforgeryToken);
        return request;
    }

    private async Task EnsureAntiforgeryTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(antiforgeryToken))
        {
            var response = await GetAsync<AntiforgeryResponse>($"{Endpoint}/antiforgery", cancellationToken);
            antiforgeryToken = response.Token;
        }
    }
}