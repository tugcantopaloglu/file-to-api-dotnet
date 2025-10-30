namespace FileToApi.Models;

public class FileStorageSettings
{
    public string RootPath { get; set; } = "Files";
    public long MaxFileSize { get; set; } = 52428800;
    public List<string> AllowedExtensions { get; set; } = new();
}
