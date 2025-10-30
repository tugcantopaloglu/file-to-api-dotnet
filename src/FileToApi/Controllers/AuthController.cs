using FileToApi.Models;
using FileToApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileToApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IJwtTokenService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly AuthenticationSettings _authSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IActiveDirectoryService adService,
        IJwtTokenService jwtService,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AuthenticationSettings> authSettings,
        ILogger<AuthController> logger)
    {
        _adService = adService;
        _jwtService = jwtService;
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

        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        try
        {
            var isValid = await _adService.ValidateCredentialsAsync(request.Username, request.Password);

            if (!isValid)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var userInfo = await _adService.GetUserInfoAsync(request.Username);
            var token = _jwtService.GenerateToken(request.Username, userInfo);
            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            return Ok(new LoginResponse
            {
                Token = token,
                Username = request.Username,
                ExpiresAt = expiresAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(500, new { message = "An error occurred during authentication" });
        }
    }

    [HttpGet("status")]
    public IActionResult GetAuthStatus()
    {
        return Ok(new
        {
            authenticationEnabled = _authSettings.Enabled,
            authType = "ActiveDirectory"
        });
    }
}
