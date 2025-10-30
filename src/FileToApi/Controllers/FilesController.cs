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

    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new { message = "No file provided" });
            }

            var fileName = await _fileService.UploadFileAsync(file);

            return Ok(new
            {
                message = "File uploaded successfully",
                fileName = fileName,
                url = $"/api/files/{fileName}"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "An error occurred while uploading the file");
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteFile(string fileName)
    {
        try
        {
            var deleted = await _fileService.DeleteFileAsync(fileName);

            if (!deleted)
            {
                return NotFound(new { message = "File not found" });
            }

            return Ok(new { message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return StatusCode(500, "An error occurred while deleting the file");
        }
    }
}
