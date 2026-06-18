using System.ComponentModel.DataAnnotations;

namespace TaskManager.Applab.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(20, ErrorMessage = "Username must be at most 20 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    //[Matches("[A-Z]"), ErrorMessage = "password must contain at least one uppercase letter"]
    //[Matches("[0-9]"), ErrorMessage = "password must contain at least one number"]
    public string Password { get; set; } = string.Empty;
}