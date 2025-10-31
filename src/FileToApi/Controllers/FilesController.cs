using FileToApi.Attributes;
using FileToApi.Models;
using FileToApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FileToApi.Controllers;

[ApiController]
[Route("img")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;
    private readonly ImageProcessingSettings _imageSettings;

    public FilesController(IFileService fileService, ILogger<FilesController> logger, IOptions<ImageProcessingSettings> imageSettings)
    {
        _fileService = fileService;
        _logger = logger;
        _imageSettings = imageSettings.Value;
    }

    //[HttpGet]
    //public async Task<IActionResult> GetAllFiles()
    //{
    //    try
    //    {
    //        var files = await _fileService.GetAllFilesAsync();
    //        return Ok(files);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving files");
    //        return StatusCode(500, "An error occurred while retrieving files");
    //    }
    //}

    [HttpGet("base64/{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFileAsBase64(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        try
        {
            var result = await _fileService.GetFileAsBase64Async(filePath);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(new
            {
                fileName = result.Value.fileName,
                contentType = result.Value.contentType,
                base64Data = result.Value.base64Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file as base64: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the file");
        }
    }

    [HttpGet("base64/thumbnail/{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetThumbnailAsBase64(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        try
        {
            var result = await _fileService.GetThumbnailAsBase64Async(filePath);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(new
            {
                fileName = result.Value.fileName,
                contentType = result.Value.contentType,
                base64Data = result.Value.base64Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thumbnail as base64: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the thumbnail");
        }
    }

    [HttpGet("base64/mobile/{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath", "maxWidth", "maxHeight", "quality" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMobileImageAsBase64(string filePath, [FromQuery] int? maxWidth, [FromQuery] int? maxHeight, [FromQuery] int? quality)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        if (quality.HasValue && (quality.Value < 1 || quality.Value > 100))
        {
            return BadRequest(new { message = "Quality must be between 1 and 100" });
        }

        try
        {
            var result = await _fileService.GetCompressedImageAsBase64Async(filePath, maxWidth, maxHeight, quality);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(new
            {
                fileName = result.Value.fileName,
                contentType = result.Value.contentType,
                base64Data = result.Value.base64Data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mobile image as base64: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the mobile image");
        }
    }

    [HttpGet("thumbnail/{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetThumbnail(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        try
        {
            var result = await _fileService.GetThumbnailAsync(filePath);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            var fileName = Path.GetFileName(filePath);
            return File(result.Value.content, result.Value.contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thumbnail: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the thumbnail");
        }
    }

    [HttpGet("mobile/{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath", "maxWidth", "maxHeight", "quality" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMobileImage(string filePath, [FromQuery] int? maxWidth, [FromQuery] int? maxHeight, [FromQuery] int? quality)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        if (quality.HasValue && (quality.Value < 1 || quality.Value > 100))
        {
            return BadRequest(new { message = "Quality must be between 1 and 100" });
        }

        try
        {
            var result = await _fileService.GetCompressedImageAsync(filePath, maxWidth, maxHeight, quality);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            var fileName = Path.GetFileName(filePath);
            return File(result.Value.content, result.Value.contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mobile image: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the mobile image");
        }
    }

    [HttpGet("{*filePath}")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "filePath" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        if (filePath.EndsWith("/metadata", StringComparison.OrdinalIgnoreCase))
        {
            var actualPath = filePath.Substring(0, filePath.Length - "/metadata".Length);

            try
            {
                var metadata = await _fileService.GetFileMetadataAsync(actualPath);

                if (metadata == null)
                {
                    return NotFound(new { message = "File not found" });
                }

                if (_imageSettings.EnableResponseCaching)
                {
                    Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
                }

                return Ok(metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file metadata: {FilePath}", actualPath);
                return StatusCode(500, "An error occurred while retrieving file metadata");
            }
        }

        try
        {
            var result = await _fileService.GetFileAsync(filePath);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            var fileName = Path.GetFileName(filePath);
            return File(result.Value.content, result.Value.contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the file");
        }
    }

    [HttpPost("batch/base64")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "request" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBatchFilesAsBase64([FromBody] BatchFileRequest request)
    {
        if (request?.FilePaths == null || !request.FilePaths.Any())
        {
            return BadRequest(new { message = "File paths are required" });
        }

        try
        {
            var result = await _fileService.GetBatchFilesAsBase64Async(request.FilePaths);

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch files");
            return StatusCode(500, "An error occurred while retrieving batch files");
        }
    }


    [HttpPost("batch/thumbnail")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "request" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBatchThumbnailsAsBase64([FromBody] BatchFileRequest request)
    {
        if (request?.FilePaths == null || !request.FilePaths.Any())
        {
            return BadRequest(new { message = "File paths are required" });
        }

        try
        {
            var result = await _fileService.GetBatchThumbnailsAsBase64Async(request.FilePaths);

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch thumbnails");
            return StatusCode(500, "An error occurred while retrieving batch thumbnails");
        }
    }


    [HttpPost("batch/mobile")]
    [ResponseCache(Duration = 3600, VaryByQueryKeys = new[] { "request", "maxWidth", "maxHeight", "quality" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBatchMobileImagesAsBase64([FromBody] BatchFileRequest request, [FromQuery] int? maxWidth, [FromQuery] int? maxHeight, [FromQuery] int? quality)
    {
        if (request?.FilePaths == null || !request.FilePaths.Any())
        {
            return BadRequest(new { message = "File paths are required" });
        }

        if (quality.HasValue && (quality.Value < 1 || quality.Value > 100))
        {
            return BadRequest(new { message = "Quality must be between 1 and 100" });
        }

        try
        {
            var result = await _fileService.GetBatchMobileImagesAsBase64Async(request.FilePaths, maxWidth, maxHeight, quality);

            if (_imageSettings.EnableResponseCaching)
            {
                Response.Headers.CacheControl = $"public, max-age={_imageSettings.CacheDurationSeconds}";
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving batch mobile images");
            return StatusCode(500, "An error occurred while retrieving batch mobile images");
        }
    }
}
