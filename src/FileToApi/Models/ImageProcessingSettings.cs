namespace FileToApi.Models;

public class ImageProcessingSettings
{
    public int ThumbnailMaxWidth { get; set; } = 150;
    public int ThumbnailMaxHeight { get; set; } = 150;
    public int MobileMaxWidth { get; set; } = 800;
    public int MobileMaxHeight { get; set; } = 800;
    public int CompressionQuality { get; set; } = 75;
    public int CacheDurationSeconds { get; set; } = 3600;
    public bool EnableResponseCaching { get; set; } = true;
}
