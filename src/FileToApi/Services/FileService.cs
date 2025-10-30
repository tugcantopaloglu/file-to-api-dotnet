using FileToApi.Models;
using Microsoft.Extensions.Options;

namespace FileToApi.Services;

public class FileService : IFileService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileService> _logger;
    private readonly string _storagePath;

    public FileService(IOptions<FileStorageSettings> settings, ILogger<FileService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), _settings.RootPath);

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<List<FileMetadata>> GetAllFilesAsync()
    {
        var files = new List<FileMetadata>();

        if (!Directory.Exists(_storagePath))
        {
            return files;
        }

        var fileInfos = new DirectoryInfo(_storagePath).GetFiles();

        foreach (var fileInfo in fileInfos)
        {
            files.Add(CreateFileMetadata(fileInfo));
        }

        return await Task.FromResult(files);
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        var fileInfo = new FileInfo(filePath);
        return await Task.FromResult(CreateFileMetadata(fileInfo));
    }

    public async Task<(byte[] content, string contentType)?> GetFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(filePath);
        var contentType = GetContentType(fileName);

        return (bytes, contentType);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty");
        }

        if (file.Length > _settings.MaxFileSize)
        {
            throw new ArgumentException($"File size exceeds maximum allowed size of {_settings.MaxFileSize} bytes");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_settings.AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"File extension {extension} is not allowed");
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_storagePath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        _logger.LogInformation("File uploaded: {FileName}", fileName);

        return fileName;
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_storagePath, fileName);

        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        _logger.LogInformation("File deleted: {FileName}", fileName);

        return await Task.FromResult(true);
    }

    private FileMetadata CreateFileMetadata(FileInfo fileInfo)
    {
        return new FileMetadata
        {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            ContentType = GetContentType(fileInfo.Name),
            CreatedAt = fileInfo.CreationTimeUtc,
            ModifiedAt = fileInfo.LastWriteTimeUtc
        };
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }
}
