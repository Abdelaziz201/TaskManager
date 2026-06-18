using TaskManager.Applab.Domain.Enums;

namespace TaskManager.Applab.Application.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public TaskItemStatus Status { get; set; }

    }
}
