using System.ComponentModel.DataAnnotations;

namespace TaskManager.Applab.Web.Models;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email")]
    public string Email { get; set; } = string.Empty;
}