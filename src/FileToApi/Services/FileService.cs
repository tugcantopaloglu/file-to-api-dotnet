using FileToApi.Models;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Gif;

namespace FileToApi.Services;

public class FileService : IFileService
{
    private readonly FileStorageSettings _settings;
    private readonly ImageProcessingSettings _imageSettings;
    private readonly ILogger<FileService> _logger;
    private readonly string _storagePath;

    public FileService(IOptions<FileStorageSettings> settings, IOptions<ImageProcessingSettings> imageSettings, ILogger<FileService> logger)
    {
        _settings = settings.Value;
        _imageSettings = imageSettings.Value;
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

        // Check if file exists with exact path first
        if (IsPathSafe(filePath) && File.Exists(filePath))
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(fileName);
            return (bytes, contentType);
        }

        // If file doesn't exist and has no extension, try with allowed extensions
        if (IsPathSafe(filePath) && string.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            foreach (var extension in _settings.AllowedExtensions)
            {
                var filePathWithExtension = filePath + extension;

                if (IsPathSafe(filePathWithExtension) && File.Exists(filePathWithExtension))
                {
                    var bytes = await File.ReadAllBytesAsync(filePathWithExtension);
                    var contentType = GetContentType(filePathWithExtension);
                    _logger.LogInformation("File found with auto-detected extension: {Extension} for {FileName}", extension, fileName);
                    return (bytes, contentType);
                }
            }
        }

        return null;
    }

    public async Task<(string base64Data, string contentType, string fileName)?> GetFileAsBase64Async(string fileName)
    {
        var sanitizedPath = SanitizePath(fileName);
        var filePath = Path.Combine(_storagePath, sanitizedPath);
        string? actualFilePath = null;
        string? actualFileName = fileName;

        // Check if file exists with exact path first
        if (IsPathSafe(filePath) && File.Exists(filePath))
        {
            actualFilePath = filePath;
        }
        // If file doesn't exist and has no extension, try with allowed extensions
        else if (IsPathSafe(filePath) && string.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            foreach (var extension in _settings.AllowedExtensions)
            {
                var filePathWithExtension = filePath + extension;

                if (IsPathSafe(filePathWithExtension) && File.Exists(filePathWithExtension))
                {
                    actualFilePath = filePathWithExtension;
                    actualFileName = fileName + extension;
                    _logger.LogInformation("File found with auto-detected extension: {Extension} for {FileName}", extension, fileName);
                    break;
                }
            }
        }

        if (actualFilePath == null)
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(actualFilePath);
        var base64Data = Convert.ToBase64String(bytes);
        var contentType = GetContentType(actualFileName);
        var fileNameOnly = Path.GetFileName(actualFileName);

        return (base64Data, contentType, fileNameOnly);
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
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };
    }

    private async Task SaveImageAsync(Image image, MemoryStream ms, string contentType, int quality)
    {
        switch (contentType)
        {
            case "image/png":
                await image.SaveAsPngAsync(ms);
                break;
            case "image/webp":
                await image.SaveAsWebpAsync(ms, new WebpEncoder { Quality = quality });
                break;
            case "image/gif":
                await image.SaveAsGifAsync(ms);
                break;
            default: // jpeg
                await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = quality });
                break;
        }
    }

    public async Task<(byte[] content, string contentType)?> GetThumbnailAsync(string fileName)
    {
        var fileResult = await GetFileAsync(fileName);
        if (fileResult == null)
        {
            return null;
        }

        var (bytes, contentType) = fileResult.Value;

        // Only process image files
        if (!contentType.StartsWith("image/"))
        {
            return fileResult;
        }

        try
        {
            using var image = Image.Load(bytes);

            // Calculate new dimensions maintaining aspect ratio
            var ratioX = (double)_imageSettings.ThumbnailMaxWidth / image.Width;
            var ratioY = (double)_imageSettings.ThumbnailMaxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            using var ms = new MemoryStream();
            await SaveImageAsync(image, ms, contentType, _imageSettings.CompressionQuality);

            return (ms.ToArray(), contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for: {FileName}", fileName);
            return fileResult; // Return original if processing fails
        }
    }

    public async Task<(byte[] content, string contentType)?> GetCompressedImageAsync(string fileName, int? maxWidth = null, int? maxHeight = null, int? quality = null)
    {
        var fileResult = await GetFileAsync(fileName);
        if (fileResult == null)
        {
            return null;
        }

        var (bytes, contentType) = fileResult.Value;

        // Only process image files
        if (!contentType.StartsWith("image/"))
        {
            return fileResult;
        }

        try
        {
            using var image = Image.Load(bytes);

            var targetWidth = maxWidth ?? _imageSettings.MobileMaxWidth;
            var targetHeight = maxHeight ?? _imageSettings.MobileMaxHeight;
            var targetQuality = quality ?? _imageSettings.CompressionQuality;

            // Only resize if image is larger than target dimensions
            if (image.Width > targetWidth || image.Height > targetHeight)
            {
                var ratioX = (double)targetWidth / image.Width;
                var ratioY = (double)targetHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x.Resize(newWidth, newHeight));
            }

            using var ms = new MemoryStream();
            await SaveImageAsync(image, ms, contentType, targetQuality);

            _logger.LogInformation("Compressed image {FileName}: Original size: {OriginalSize} bytes, Compressed size: {CompressedSize} bytes",
                fileName, bytes.Length, ms.Length);

            return (ms.ToArray(), contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compressing image: {FileName}", fileName);
            return fileResult; // Return original if processing fails
        }
    }

    public async Task<(string base64Data, string contentType, string fileName)?> GetThumbnailAsBase64Async(string fileName)
    {
        var result = await GetThumbnailAsync(fileName);
        if (result == null)
        {
            return null;
        }

        var (bytes, contentType) = result.Value;
        var base64Data = Convert.ToBase64String(bytes);
        var fileNameOnly = Path.GetFileName(fileName);

        // If no extension, try to get the actual filename from the file that was found
        if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            var actualFile = await GetFileMetadataAsync(fileName);
            if (actualFile != null)
            {
                fileNameOnly = Path.GetFileName(actualFile.FileName);
            }
        }

        return (base64Data, contentType, fileNameOnly);
    }

    public async Task<(string base64Data, string contentType, string fileName)?> GetCompressedImageAsBase64Async(string fileName, int? maxWidth = null, int? maxHeight = null, int? quality = null)
    {
        var result = await GetCompressedImageAsync(fileName, maxWidth, maxHeight, quality);
        if (result == null)
        {
            return null;
        }

        var (bytes, contentType) = result.Value;
        var base64Data = Convert.ToBase64String(bytes);
        var fileNameOnly = Path.GetFileName(fileName);

        // If no extension, try to get the actual filename from the file that was found
        if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            var actualFile = await GetFileMetadataAsync(fileName);
            if (actualFile != null)
            {
                fileNameOnly = Path.GetFileName(actualFile.FileName);
            }
        }

        return (base64Data, contentType, fileNameOnly);
    }

    public async Task<BatchFileResponse> GetBatchFilesAsBase64Async(List<string> filePaths)
    {
        var response = new BatchFileResponse
        {
            TotalRequested = filePaths.Count
        };

        var tasks = filePaths.Select(async filePath =>
        {
            var item = new BatchFileItem
            {
                RequestedPath = filePath
            };

            try
            {
                var result = await GetFileAsBase64Async(filePath);
                if (result != null)
                {
                    item.FileName = result.Value.fileName;
                    item.ContentType = result.Value.contentType;
                    item.Base64Data = result.Value.base64Data;
                    item.Found = true;
                }
                else
                {
                    item.Found = false;
                    item.Error = "File not found";
                }
            }
            catch (Exception ex)
            {
                item.Found = false;
                item.Error = ex.Message;
                _logger.LogError(ex, "Error processing batch file: {FilePath}", filePath);
            }

            return item;
        });

        response.Files = (await Task.WhenAll(tasks)).ToList();
        response.TotalFound = response.Files.Count(f => f.Found);
        response.TotalNotFound = response.Files.Count(f => !f.Found);

        _logger.LogInformation("Batch request completed: {TotalRequested} requested, {TotalFound} found, {TotalNotFound} not found",
            response.TotalRequested, response.TotalFound, response.TotalNotFound);

        return response;
    }

    public async Task<BatchFileResponse> GetBatchThumbnailsAsBase64Async(List<string> filePaths)
    {
        var response = new BatchFileResponse
        {
            TotalRequested = filePaths.Count
        };

        var tasks = filePaths.Select(async filePath =>
        {
            var item = new BatchFileItem
            {
                RequestedPath = filePath
            };

            try
            {
                var result = await GetThumbnailAsBase64Async(filePath);
                if (result != null)
                {
                    item.FileName = result.Value.fileName;
                    item.ContentType = result.Value.contentType;
                    item.Base64Data = result.Value.base64Data;
                    item.Found = true;
                }
                else
                {
                    item.Found = false;
                    item.Error = "File not found";
                }
            }
            catch (Exception ex)
            {
                item.Found = false;
                item.Error = ex.Message;
                _logger.LogError(ex, "Error processing batch thumbnail: {FilePath}", filePath);
            }

            return item;
        });

        response.Files = (await Task.WhenAll(tasks)).ToList();
        response.TotalFound = response.Files.Count(f => f.Found);
        response.TotalNotFound = response.Files.Count(f => !f.Found);

        _logger.LogInformation("Batch thumbnail request completed: {TotalRequested} requested, {TotalFound} found, {TotalNotFound} not found",
            response.TotalRequested, response.TotalFound, response.TotalNotFound);

        return response;
    }

    public async Task<BatchFileResponse> GetBatchMobileImagesAsBase64Async(List<string> filePaths, int? maxWidth = null, int? maxHeight = null, int? quality = null)
    {
        var response = new BatchFileResponse
        {
            TotalRequested = filePaths.Count
        };

        var tasks = filePaths.Select(async filePath =>
        {
            var item = new BatchFileItem
            {
                RequestedPath = filePath
            };

            try
            {
                var result = await GetCompressedImageAsBase64Async(filePath, maxWidth, maxHeight, quality);
                if (result != null)
                {
                    item.FileName = result.Value.fileName;
                    item.ContentType = result.Value.contentType;
                    item.Base64Data = result.Value.base64Data;
                    item.Found = true;
                }
                else
                {
                    item.Found = false;
                    item.Error = "File not found";
                }
            }
            catch (Exception ex)
            {
                item.Found = false;
                item.Error = ex.Message;
                _logger.LogError(ex, "Error processing batch mobile image: {FilePath}", filePath);
            }

            return item;
        });

        response.Files = (await Task.WhenAll(tasks)).ToList();
        response.TotalFound = response.Files.Count(f => f.Found);
        response.TotalNotFound = response.Files.Count(f => !f.Found);

        _logger.LogInformation("Batch mobile image request completed: {TotalRequested} requested, {TotalFound} found, {TotalNotFound} not found",
            response.TotalRequested, response.TotalFound, response.TotalNotFound);

        return response;
    }
}
