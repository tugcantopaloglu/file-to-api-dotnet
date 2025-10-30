namespace FileToApi.Services;

public interface IActiveDirectoryService
{
    Task<bool> ValidateCredentialsAsync(string username, string password);
    Task<Dictionary<string, string>> GetUserInfoAsync(string username);
}
