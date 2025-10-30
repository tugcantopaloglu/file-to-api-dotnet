using FileToApi.Models;
using System.Security.Claims;

namespace FileToApi.Services;

public interface IJwtTokenService
{
    string GenerateToken(string username, Dictionary<string, string> userInfo);
    ClaimsPrincipal? ValidateToken(string token);
}
