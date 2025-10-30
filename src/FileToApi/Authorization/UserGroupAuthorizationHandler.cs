using FileToApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FileToApi.Authorization;

public class UserGroupAuthorizationHandler : AuthorizationHandler<UserGroupAuthorizationRequirement>
{
    private readonly AuthorizationSettings _settings;
    private readonly ILogger<UserGroupAuthorizationHandler> _logger;

    public UserGroupAuthorizationHandler(
        IOptions<AuthorizationSettings> settings,
        ILogger<UserGroupAuthorizationHandler> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UserGroupAuthorizationRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("User is not authenticated");
            return Task.CompletedTask;
        }

        var username = context.User.Identity.Name;

        if (_settings.AllowedUsers.Count == 0 && _settings.AllowedGroups.Count == 0)
        {
            _logger.LogInformation("No user/group restrictions configured. Allowing user: {Username}", username);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (_settings.AllowedUsers.Any(u => u.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogInformation("User {Username} is in allowed users list", username);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        foreach (var allowedGroup in _settings.AllowedGroups)
        {
            if (userRoles.Any(r => r.Equals(allowedGroup, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("User {Username} is member of allowed group: {Group}", username, allowedGroup);
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        _logger.LogWarning("User {Username} is not authorized. User roles: {Roles}", username, string.Join(", ", userRoles));
        return Task.CompletedTask;
    }
}
