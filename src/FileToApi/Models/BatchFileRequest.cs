using System.ComponentModel.DataAnnotations;

namespace FileToApi.Models;

public class BatchFileRequest
{
    [Required(ErrorMessage = "File paths are required")]
    [MinLength(1, ErrorMessage = "At least one file path is required")]
    public List<string> FilePaths { get; set; } = new();
}
