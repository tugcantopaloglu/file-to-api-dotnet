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

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetFile(string fileName)
    {
        try
        {
            var result = await _fileService.GetFileAsync(fileName);

            if (result == null)
            {
                return NotFound(new { message = "File not found" });
            }

            return File(result.Value.content, result.Value.contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FileName}", fileName);
            return StatusCode(500, "An error occurred while retrieving the file");
        }
    }

    [HttpGet("{fileName}/metadata")]
    public async Task<IActionResult> GetFileMetadata(string fileName)
    {
        try
        {
            var metadata = await _fileService.GetFileMetadataAsync(fileName);

            if (metadata == null)
            {
                return NotFound(new { message = "File not found" });
            }

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file metadata: {FileName}", fileName);
            return StatusCode(500, "An error occurred while retrieving file metadata");
        }
    }
}
