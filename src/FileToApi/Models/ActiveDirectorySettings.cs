namespace FileToApi.Models;

public class ActiveDirectorySettings
{
    public string Domain { get; set; } = string.Empty;
    public string LdapPath { get; set; } = string.Empty;
    public string Container { get; set; } = string.Empty;
}
