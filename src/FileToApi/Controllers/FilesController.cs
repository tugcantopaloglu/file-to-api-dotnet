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

    /// <summary>
    /// Retrieves a thumbnail version of an image (150x150 max by default).
    /// </summary>
    /// <param name="filePath">The relative path to the image file</param>
    /// <returns>Resized image maintaining aspect ratio</returns>
    /// <response code="200">Thumbnail retrieved successfully</response>
    /// <response code="400">File path is required</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /img/thumbnail/photo.jpg
    ///
    /// Returns a smaller version of the image optimized for thumbnails.
    /// Non-image files are returned as-is without processing.
    /// </remarks>
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

    /// <summary>
    /// Retrieves a compressed/resized version of an image optimized for mobile devices.
    /// </summary>
    /// <param name="filePath">The relative path to the image file</param>
    /// <param name="maxWidth">Optional maximum width (default: 800px from config)</param>
    /// <param name="maxHeight">Optional maximum height (default: 800px from config)</param>
    /// <param name="quality">Optional JPEG quality 1-100 (default: 75 from config)</param>
    /// <returns>Compressed and resized image maintaining aspect ratio</returns>
    /// <response code="200">Compressed image retrieved successfully</response>
    /// <response code="400">File path is required or invalid parameters</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /img/mobile/photo.jpg
    ///     Uses default settings (800x800 max, quality 75)
    ///
    ///     GET /img/mobile/photo.jpg?maxWidth=600&amp;maxHeight=600&amp;quality=80
    ///     Custom size and quality
    ///
    /// Perfect for mobile apps to reduce bandwidth and improve loading times.
    /// Images are only resized if they exceed the specified dimensions.
    /// Non-image files are returned as-is without processing.
    /// </remarks>
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

    /// <summary>
    /// Retrieves a file by its path. Supports raw file or metadata mode.
    /// </summary>
    /// <param name="filePath">The relative path to the file. Can include suffix:
    /// - No suffix: Returns the raw file
    /// - /metadata: Returns file metadata (size, content type, timestamps)</param>
    /// <returns>
    /// - Raw file: Binary file stream with appropriate content type
    /// - Metadata mode: JSON object with file information
    /// </returns>
    /// <response code="200">File retrieved successfully</response>
    /// <response code="400">File path is required</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample requests:
    ///
    ///     GET /img/photo.jpg
    ///     Returns the raw image file
    ///
    ///     GET /img/photo.jpg/metadata
    ///     Returns: { "fileName": "photo.jpg", "fileSize": 12345, "contentType": "image/jpeg", ... }
    ///
    /// If the file path doesn't include an extension, the API will automatically try allowed extensions (.jpg, .png, etc.)
    /// </remarks>
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

    /// <summary>
    /// Retrieves multiple files as base64 encoded JSON in a single request.
    /// </summary>
    /// <param name="request">List of file paths to retrieve</param>
    /// <returns>Batch response with all requested files</returns>
    /// <response code="200">Batch operation completed (individual files may be not found)</response>
    /// <response code="400">Invalid request or empty file paths</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /img/batch/base64
    ///     {
    ///       "filePaths": ["photo1.jpg", "photo2", "subfolder/photo3.png"]
    ///     }
    ///
    /// Returns:
    ///
    ///     {
    ///       "files": [
    ///         {
    ///           "requestedPath": "photo1.jpg",
    ///           "fileName": "photo1.jpg",
    ///           "contentType": "image/jpeg",
    ///           "base64Data": "iVBORw0KG...",
    ///           "found": true,
    ///           "error": null
    ///         },
    ///         {
    ///           "requestedPath": "photo2",
    ///           "fileName": "photo2.png",
    ///           "contentType": "image/png",
    ///           "base64Data": "iVBORw0KG...",
    ///           "found": true,
    ///           "error": null
    ///         }
    ///       ],
    ///       "totalRequested": 2,
    ///       "totalFound": 2,
    ///       "totalNotFound": 0
    ///     }
    ///
    /// Supports automatic extension detection for files without extensions.
    /// Perfect for mobile apps to load multiple images in one network call.
    /// </remarks>
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

    /// <summary>
    /// Retrieves multiple thumbnail images as base64 encoded JSON in a single request.
    /// </summary>
    /// <param name="request">List of file paths to retrieve as thumbnails</param>
    /// <returns>Batch response with all requested thumbnails</returns>
    /// <response code="200">Batch operation completed</response>
    /// <response code="400">Invalid request or empty file paths</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /img/batch/thumbnail
    ///     {
    ///       "filePaths": ["photo1.jpg", "photo2", "photo3.png"]
    ///     }
    ///
    /// Returns 150x150px thumbnails for all requested files.
    /// Ideal for displaying image galleries or lists in mobile apps.
    /// </remarks>
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

    /// <summary>
    /// Retrieves multiple compressed/mobile-optimized images as base64 encoded JSON in a single request.
    /// </summary>
    /// <param name="request">List of file paths to retrieve</param>
    /// <param name="maxWidth">Optional maximum width for all images (default: 800px from config)</param>
    /// <param name="maxHeight">Optional maximum height for all images (default: 800px from config)</param>
    /// <param name="quality">Optional JPEG quality 1-100 for all images (default: 75 from config)</param>
    /// <returns>Batch response with all requested compressed images</returns>
    /// <response code="200">Batch operation completed</response>
    /// <response code="400">Invalid request, empty file paths, or invalid quality parameter</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /img/batch/mobile?maxWidth=600&amp;maxHeight=600&amp;quality=80
    ///     {
    ///       "filePaths": ["photo1.jpg", "photo2", "photo3.png"]
    ///     }
    ///
    /// Perfect for loading multiple mobile-optimized images in one request.
    /// Significantly reduces network overhead for mobile apps.
    /// All images will use the same compression settings.
    /// </remarks>
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
