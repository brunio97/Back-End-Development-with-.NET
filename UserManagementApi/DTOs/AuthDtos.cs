using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    [RegularExpression(
        @"^[\p{L}\p{M}\s.'-]+$",
        ErrorMessage = "Name contains invalid characters.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Range(18, 120)]
    public int Age { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
