using System.ComponentModel.DataAnnotations;

namespace JobTracker.API.Models;

/// <summary>
/// DTO for user registration requests.
/// Validation attributes enforce password criteria on the server side.
/// </summary>
public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,}$",
        ErrorMessage = "Password must contain at least 1 uppercase letter, 1 lowercase letter, and 1 special character.")]
    public required string Password { get; set; }
}

/// <summary>
/// DTO for user login requests.
/// </summary>
public class LoginRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}

/// <summary>
/// DTO for auth responses — never exposes the password hash.
/// </summary>
public class AuthResponse
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
