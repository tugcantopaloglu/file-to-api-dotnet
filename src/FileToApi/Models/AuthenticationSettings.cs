namespace FileToApi.Models;

public class AuthenticationSettings
{
    public bool Enabled { get; set; }
    public string Type { get; set; } = "AzureAD";
}
