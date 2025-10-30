using FileToApi.Attributes;
using System.ComponentModel.DataAnnotations;

namespace FileToApi.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [ValidUsername]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 256 characters")]
    public string Password { get; set; } = string.Empty;
}
