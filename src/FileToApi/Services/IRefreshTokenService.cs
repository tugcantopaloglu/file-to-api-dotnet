using FileToApi.Models;

namespace FileToApi.Services;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(string token, string username, DateTime expiresAt);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task CleanupExpiredTokensAsync();
}
