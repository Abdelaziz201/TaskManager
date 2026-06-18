using System.ComponentModel.DataAnnotations;

namespace TaskManager.Applab.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is Required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is Required")]
    //[MinLength(6, ErrorMessage"Password must be at least 6 characters long.")]
    public string Password { get; set; } = string.Empty;

}

