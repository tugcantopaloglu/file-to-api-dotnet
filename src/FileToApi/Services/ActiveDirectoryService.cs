using FileToApi.Models;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace FileToApi.Services;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly ActiveDirectorySettings _settings;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(
        IOptions<ActiveDirectorySettings> settings,
        ILogger<ActiveDirectoryService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<bool> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                _settings.Domain,
                _settings.Container);

            var isValid = context.ValidateCredentials(username, password);

            if (isValid)
            {
                _logger.LogInformation("User {Username} authenticated successfully", username);
            }
            else
            {
                _logger.LogWarning("Failed authentication attempt for user {Username}", username);
            }

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for user {Username}", username);
            return Task.FromResult(false);
        }
    }

    public Task<Dictionary<string, string>> GetUserInfoAsync(string username)
    {
        var userInfo = new Dictionary<string, string>();

        try
        {
            using var context = new PrincipalContext(
                ContextType.Domain,
                _settings.Domain,
                _settings.Container);

            using var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);

            if (user != null)
            {
                userInfo["Username"] = user.SamAccountName ?? string.Empty;
                userInfo["DisplayName"] = user.DisplayName ?? string.Empty;
                userInfo["Email"] = user.EmailAddress ?? string.Empty;
                userInfo["GivenName"] = user.GivenName ?? string.Empty;
                userInfo["Surname"] = user.Surname ?? string.Empty;

                var groups = user.GetAuthorizationGroups();
                var groupNames = new List<string>();
                foreach (var group in groups)
                {
                    if (group is GroupPrincipal groupPrincipal)
                    {
                        groupNames.Add(groupPrincipal.Name);
                    }
                }
                userInfo["Groups"] = string.Join(",", groupNames);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info for {Username}", username);
        }

        return Task.FromResult(userInfo);
    }
}
