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

        if (Path.IsPathRooted(_settings.RootPath))
        {
            _storagePath = _settings.RootPath;
        }
        else
        {
            _storagePath = Path.Combine(Directory.GetCurrentDirectory(), _settings.RootPath);
        }

        if (!Directory.Exists(_storagePath))
        {
            try
            {
                Directory.CreateDirectory(_storagePath);
                _logger.LogInformation("Created storage directory: {StoragePath}", _storagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage directory: {StoragePath}", _storagePath);
                throw;
            }
        }

        _logger.LogInformation("File storage initialized at: {StoragePath}", _storagePath);
    }

    public async Task<List<FileMetadata>> GetAllFilesAsync()
    {
        var files = new List<FileMetadata>();

        if (!Directory.Exists(_storagePath))
        {
            return files;
        }

        GetFilesRecursive(_storagePath, _storagePath, files);

        return await Task.FromResult(files);
    }

    public async Task<FileMetadata?> GetFileMetadataAsync(string fileName)
    {
        var sanitizedPath = SanitizePath(fileName);
        var filePath = Path.Combine(_storagePath, sanitizedPath);

        if (!IsPathSafe(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        var fileInfo = new FileInfo(filePath);
        var relativePath = GetRelativePath(_storagePath, filePath);
        return await Task.FromResult(CreateFileMetadata(fileInfo, relativePath));
    }

    public async Task<(byte[] content, string contentType)?> GetFileAsync(string fileName)
    {
        var sanitizedPath = SanitizePath(fileName);
        var filePath = Path.Combine(_storagePath, sanitizedPath);

        if (!IsPathSafe(filePath) || !File.Exists(filePath))
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

    private void GetFilesRecursive(string currentPath, string basePath, List<FileMetadata> files)
    {
        var directoryInfo = new DirectoryInfo(currentPath);

        foreach (var fileInfo in directoryInfo.GetFiles())
        {
            var relativePath = GetRelativePath(basePath, fileInfo.FullName);
            files.Add(CreateFileMetadata(fileInfo, relativePath));
        }

        foreach (var subdirectory in directoryInfo.GetDirectories())
        {
            GetFilesRecursive(subdirectory.FullName, basePath, files);
        }
    }

    private string GetRelativePath(string basePath, string fullPath)
    {
        var baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? basePath
            : basePath + Path.DirectorySeparatorChar);
        var fullUri = new Uri(fullPath);
        var relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString());

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private string SanitizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        return path.Replace('/', Path.DirectorySeparatorChar)
                   .Replace('\\', Path.DirectorySeparatorChar);
    }

    private bool IsPathSafe(string fullPath)
    {
        var normalizedPath = Path.GetFullPath(fullPath);
        var normalizedBasePath = Path.GetFullPath(_storagePath);

        return normalizedPath.StartsWith(normalizedBasePath, StringComparison.OrdinalIgnoreCase);
    }

    private FileMetadata CreateFileMetadata(FileInfo fileInfo, string relativePath)
    {
        return new FileMetadata
        {
            FileName = relativePath,
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
