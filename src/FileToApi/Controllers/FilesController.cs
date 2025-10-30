using FileToApi.Attributes;
using FileToApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileToApi.Controllers;

[ApiController]
[Route("img")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
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

    /// <summary>
    /// Retrieves a file as base64 encoded JSON.
    /// </summary>
    /// <param name="filePath">The relative path to the file (e.g., "photo.jpg" or "subfolder/image.png")</param>
    /// <returns>JSON object containing fileName, contentType, and base64Data</returns>
    /// <response code="200">File retrieved successfully as base64</response>
    /// <response code="400">File path is required</response>
    /// <response code="404">File not found</response>
    /// <response code="500">Server error occurred</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /img/base64/photo.jpg
    ///
    /// Returns:
    ///
    ///     {
    ///       "fileName": "photo.jpg",
    ///       "contentType": "image/jpeg",
    ///       "base64Data": "iVBORw0KGgoAAAANSUhEUgAA..."
    ///     }
    ///
    /// If the file path doesn't include an extension, the API will automatically try allowed extensions (.jpg, .png, etc.)
    /// </remarks>
    [HttpGet("base64/{*filePath}")]
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

            var fileName = Path.GetFileName(filePath);
            return File(result.Value.content, result.Value.contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving the file");
        }
    }

    //private async Task<IActionResult> GetFileMetadataInternal(string filePath)
    //{
    //    try
    //    {
    //        var metadata = await _fileService.GetFileMetadataAsync(filePath);

    //        if (metadata == null)
    //        {
    //            return NotFound(new { message = "File not found" });
    //        }

    //        return Ok(metadata);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving file metadata: {FilePath}", filePath);
    //        return StatusCode(500, "An error occurred while retrieving file metadata");
    //    }
    //}
}
