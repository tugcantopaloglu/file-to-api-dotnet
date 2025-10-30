using System.Collections.Concurrent;

namespace FileToApi.Services;

public class RateLimitingService : IRateLimitingService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimitStore = new();
    private readonly ILogger<RateLimitingService> _logger;

    public RateLimitingService(ILogger<RateLimitingService> logger)
    {
        _logger = logger;
    }

    public Task<bool> IsAllowedAsync(string key, int maxAttempts, TimeSpan window)
    {
        var now = DateTime.UtcNow;

        var entry = _rateLimitStore.AddOrUpdate(
            key,
            _ => new RateLimitEntry
            {
                Count = 1,
                WindowStart = now,
                WindowEnd = now.Add(window)
            },
            (_, existing) =>
            {
                if (now > existing.WindowEnd)
                {
                    return new RateLimitEntry
                    {
                        Count = 1,
                        WindowStart = now,
                        WindowEnd = now.Add(window)
                    };
                }

                existing.Count++;
                return existing;
            });

        var isAllowed = entry.Count <= maxAttempts;

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for key: {Key}. Attempts: {Count}", key, entry.Count);
        }

        return Task.FromResult(isAllowed);
    }

    public Task ResetAsync(string key)
    {
        _rateLimitStore.TryRemove(key, out _);
        _logger.LogInformation("Rate limit reset for key: {Key}", key);
        return Task.CompletedTask;
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
    }
}
