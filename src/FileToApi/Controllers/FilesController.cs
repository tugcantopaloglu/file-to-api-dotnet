using FileToApi.Attributes;
using FileToApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileToApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[ConditionalAuthorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileService fileService, ILogger<FilesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFiles()
    {
        try
        {
            var files = await _fileService.GetAllFilesAsync();
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files");
            return StatusCode(500, "An error occurred while retrieving files");
        }
    }

    [HttpGet("{*filePath}")]
    public async Task<IActionResult> GetFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return BadRequest(new { message = "File path is required" });
        }

        if (filePath.EndsWith("/metadata", StringComparison.OrdinalIgnoreCase))
        {
            var actualPath = filePath.Substring(0, filePath.Length - "/metadata".Length);
            return await GetFileMetadataInternal(actualPath);
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

    private async Task<IActionResult> GetFileMetadataInternal(string filePath)
    {
        try
        {
            var metadata = await _fileService.GetFileMetadataAsync(filePath);

            if (metadata == null)
            {
                return NotFound(new { message = "File not found" });
            }

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file metadata: {FilePath}", filePath);
            return StatusCode(500, "An error occurred while retrieving file metadata");
        }
    }
}
