namespace FileToApi.Models;

public class BatchFileResponse
{
    public List<BatchFileItem> Files { get; set; } = new();
    public int TotalRequested { get; set; }
    public int TotalFound { get; set; }
    public int TotalNotFound { get; set; }
}

public class BatchFileItem
{
    public string RequestedPath { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Base64Data { get; set; }
    public bool Found { get; set; }
    public string? Error { get; set; }
}
