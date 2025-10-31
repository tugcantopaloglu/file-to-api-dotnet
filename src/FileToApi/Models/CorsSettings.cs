namespace FileToApi.Models;

public class CorsSettings
{
    public bool AllowAnyOrigin { get; set; } = true;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowAnyMethod { get; set; } = true;
    public bool AllowAnyHeader { get; set; } = true;
    public bool AllowCredentials { get; set; } = false;
}
