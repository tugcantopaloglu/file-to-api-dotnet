namespace FileToApi.Models;

public class AuthorizationSettings
{
    public List<string> AllowedUsers { get; set; } = new();
    public List<string> AllowedGroups { get; set; } = new();
}
