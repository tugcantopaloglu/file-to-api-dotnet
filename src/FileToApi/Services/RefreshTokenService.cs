using FileToApi.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace FileToApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ConcurrentDictionary<string, RefreshToken> _refreshTokens = new();
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(ILogger<RefreshTokenService> logger)
    {
        _logger = logger;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Task StoreRefreshTokenAsync(string token, string username, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            Username = username,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _refreshTokens[token] = refreshToken;
        _logger.LogInformation("Refresh token stored for user {Username}", username);

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        if (_refreshTokens.TryGetValue(token, out var refreshToken))
        {
            if (!refreshToken.IsRevoked && refreshToken.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<RefreshToken?>(refreshToken);
            }

            _logger.LogWarning("Attempted to use expired or revoked refresh token");
        }

        return Task.FromResult<RefreshToken?>(null);
    }

    public Task RevokeRefreshTokenAsync(string token)
    {
        if (_refreshTokens.TryGetValue(token, out var refreshToken))
        {
            refreshToken.IsRevoked = true;
            _logger.LogInformation("Refresh token revoked for user {Username}", refreshToken.Username);
        }

        return Task.CompletedTask;
    }

    public Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = _refreshTokens
            .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow || kvp.Value.IsRevoked)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in expiredTokens)
        {
            _refreshTokens.TryRemove(token, out _);
        }

        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
        }

        return Task.CompletedTask;
    }
}
