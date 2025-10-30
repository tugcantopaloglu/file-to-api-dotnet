using FileToApi.Models;

namespace FileToApi.Services;

public interface IFileService
{
    Task<List<FileMetadata>> GetAllFilesAsync();
    Task<FileMetadata?> GetFileMetadataAsync(string fileName);
    Task<(byte[] content, string contentType)?> GetFileAsync(string fileName);
    Task<(string base64Data, string contentType, string fileName)?> GetFileAsBase64Async(string fileName);
    Task<string> UploadFileAsync(IFormFile file);
    Task<bool> DeleteFileAsync(string fileName);
}
