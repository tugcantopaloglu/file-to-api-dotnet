using FileToApi.Models;
using FileToApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileToApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IActiveDirectoryService adService,
        IJwtTokenService jwtService,
        IRefreshTokenService refreshTokenService,
        IRateLimitingService rateLimitingService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AuthenticationSettings> authSettings,
        ILogger<AuthController> logger)
    {
        _adService = adService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _rateLimitingService = rateLimitingService;
        _jwtSettings = jwtSettings.Value;
        _authSettings = authSettings.Value;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!_authSettings.Enabled)
        {
            return BadRequest(new { message = "Authentication is disabled" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { message = "Validation failed", errors });
        }

        // Rate limiting by username
        var rateLimitKey = $"login:{request.Username}";
        var isAllowed = await _rateLimitingService.IsAllowedAsync(rateLimitKey, 5, TimeSpan.FromMinutes(15));

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for user {Username}", request.Username);
            return StatusCode(429, new { message = "Too many login attempts. Please try again later." });
        }

        try
        {
            var isValid = await _adService.ValidateCredentialsAsync(request.Username, request.Password);

            if (!isValid)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Reset rate limit on successful login
            await _rateLimitingService.ResetAsync(rateLimitKey);

            var userInfo = await _adService.GetUserInfoAsync(request.Username);
            var token = _jwtService.GenerateToken(request.Username, userInfo);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            // Generate refresh token
            var refreshToken = _refreshTokenService.GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _refreshTokenService.StoreRefreshTokenAsync(refreshToken, request.Username, refreshTokenExpiresAt);

            return Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                Username = request.Username,
                ExpiresAt = expiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!_authSettings.Enabled)
        {
            return BadRequest(new { message = "Authentication is disabled" });
        }

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            var storedToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);

            if (storedToken == null)
            {
                _logger.LogWarning("Invalid or expired refresh token used");
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            // Get fresh user info from AD
            var userInfo = await _adService.GetUserInfoAsync(storedToken.Username);
            var newToken = _jwtService.GenerateToken(storedToken.Username, userInfo);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            // Generate new refresh token and revoke old one
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);
            await _refreshTokenService.StoreRefreshTokenAsync(newRefreshToken, storedToken.Username, refreshTokenExpiresAt);

            _logger.LogInformation("Token refreshed for user {Username}", storedToken.Username);

            return Ok(new LoginResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                Username = storedToken.Username,
                ExpiresAt = expiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        if (!_authSettings.Enabled)
        {
            return BadRequest(new { message = "Authentication is disabled" });
        }

        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required" });
        }

        try
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            _logger.LogInformation("Refresh token revoked by user {Username}", User.Identity?.Name);
            return Ok(new { message = "Refresh token revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token");
            return StatusCode(500, new { message = "An error occurred while revoking the token" });
        }
    }

    [HttpGet("status")]
    public IActionResult GetAuthStatus()
    {
        return Ok(new
        {
            authenticationEnabled = _authSettings.Enabled,
            allowAnonymous = _authSettings.AllowAnonymous,
            authType = "ActiveDirectory",
            requiresAuth = _authSettings.Enabled && !_authSettings.AllowAnonymous
        });
    }
}
