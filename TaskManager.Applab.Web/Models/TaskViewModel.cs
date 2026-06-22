using System.ComponentModel.DataAnnotations;


namespace TaskManager.Applab.Web.Models;


public class TaskViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}


public class CreateTaskViewModel
{
    [Required(ErrorMessage = "Title is requierd")]
    [MinLength(3,ErrorMessage = "Title must be at least 3 characters")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due Date is required")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);
}