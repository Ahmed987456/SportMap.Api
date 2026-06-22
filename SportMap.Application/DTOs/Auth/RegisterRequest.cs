using System.ComponentModel.DataAnnotations;

namespace SportMap.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [RegularExpression(@"^01[0125][0-9]{8}$",
        ErrorMessage = "Phone number must be 11 digits")]
    public string Phone { get; set; } = string.Empty;
}