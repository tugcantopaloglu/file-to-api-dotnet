using FileToApi.Models;

namespace FileToApi.Services;

public interface IFileService
{
    Task<List<FileMetadata>> GetAllFilesAsync();
    Task<FileMetadata?> GetFileMetadataAsync(string fileName);
    Task<(byte[] content, string contentType)?> GetFileAsync(string fileName);
    Task<(string base64Data, string contentType, string fileName)?> GetFileAsBase64Async(string fileName);
    Task<(byte[] content, string contentType)?> GetThumbnailAsync(string fileName);
    Task<(string base64Data, string contentType, string fileName)?> GetThumbnailAsBase64Async(string fileName);
    Task<(byte[] content, string contentType)?> GetCompressedImageAsync(string fileName, int? maxWidth = null, int? maxHeight = null, int? quality = null);
    Task<(string base64Data, string contentType, string fileName)?> GetCompressedImageAsBase64Async(string fileName, int? maxWidth = null, int? maxHeight = null, int? quality = null);
    Task<string> UploadFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string fileName);
}
