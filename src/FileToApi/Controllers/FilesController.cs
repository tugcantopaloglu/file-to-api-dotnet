using FileToApi.Attributes;
using FileToApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileToApi.Controllers;

[ApiController]
[Route("img")]
/*[ConditionalAuthorize]
[Authorize(Policy = "UserGroupPolicy")]*/
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

        if (filePath.EndsWith("/base64", StringComparison.OrdinalIgnoreCase))
        {
            var actualPath = filePath.Substring(0, filePath.Length - "/base64".Length);

            try
            {
                var result = await _fileService.GetFileAsBase64Async(actualPath);

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
                _logger.LogError(ex, "Error retrieving file as base64: {FilePath}", actualPath);
                return StatusCode(500, "An error occurred while retrieving the file");
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
