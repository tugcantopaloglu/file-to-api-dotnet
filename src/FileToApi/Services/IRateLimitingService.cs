namespace FileToApi.Services;

public interface IRateLimitingService
{
    Task<bool> IsAllowedAsync(string key, int maxAttempts, TimeSpan window);
    Task ResetAsync(string key);
}
