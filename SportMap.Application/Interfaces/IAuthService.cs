using SportMap.Application.DTOs.Auth;

namespace SportMap.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);

    Task<AuthResponse> RegisterOwnerAsync(RegisterOwnerRequest request);

    Task UpdatePaymentInfoAsync(int userId, UpdatePaymentInfoRequest request);
}