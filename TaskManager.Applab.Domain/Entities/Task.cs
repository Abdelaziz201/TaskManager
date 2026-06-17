using TaskManager.Applab.Domain.Enums;


namespace TaskManager.Applab.Domain.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

   
}
