using FileToApi.Models;

namespace FileToApi.Services;

public interface IJwtTokenService
{
    string GenerateToken(string username, Dictionary<string, string> userInfo);
}
