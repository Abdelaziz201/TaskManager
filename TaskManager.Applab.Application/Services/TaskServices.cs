using TaskManager.Applab.Application.Common;
using TaskManager.Applab.Application.DTOs;
using TaskManager.Applab.Application.Interfaces;
using TaskManager.Applab.Domain.Entities;

namespace TaskManager.Applab.Application.Services
{
    public class TaskServices
    {
        private readonly ITaskRepository _repository;
        private readonly ITaskAttachmentRepository _attachmentRepository;  
        private readonly IFileStorageService _fileStorage;

        public TaskServices(ITaskRepository repository, ITaskAttachmentRepository attachmentRepository, IFileStorageService fileStorage)
        {
            _repository = repository;
            _attachmentRepository = attachmentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<ApiResponse<List<TaskDto>>> GetAllTasksAsync()
        {
            var tasks = await _repository.GetAllAsync();

            var taskIds = tasks.Select(t => t.Id).ToList();
            var counts = await _attachmentRepository.GetCountsByTaskIdsAsync(taskIds);

            var result = tasks.Select(t => MapToDto(t, counts.GetValueOrDefault(t.Id, 0))).ToList();
            return ApiResponse<List<TaskDto>>.Ok(result, "Tasks retrieved successfully");
        }

        public async Task<ApiResponse<TaskDto>> GetTaskByIdAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return ApiResponse<TaskDto>.Fail($"Task with id {id} not found");

            var count = await _attachmentRepository.GetCountByTaskIdAsync(id);
            return ApiResponse<TaskDto>.Ok(MapToDto(task, count));
        }

        public async Task<ApiResponse<TaskDto>> CreateTaskAsync(CreateTaskDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = dto.Status
            };

            await _repository.AddAsync(task);

            // brand new task — always 0 attachments at creation time
            return ApiResponse<TaskDto>.Ok(MapToDto(task, 0), "Task created successfully");
        }

        public async Task<ApiResponse<TaskDto>> UpdateTaskAsync(int id, CreateTaskDto dto)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return ApiResponse<TaskDto>.Fail($"Task with id {id} not found");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            task.Status = dto.Status;

            await _repository.UpdateAsync(task);

            var count = await _attachmentRepository.GetCountByTaskIdAsync(task.Id);
            return ApiResponse<TaskDto>.Ok(MapToDto(task, count), "Task updated successfully");
        }

        public async Task<ApiResponse> DeleteTaskAsync(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null)
                return ApiResponse.Fail($"Task with id {id} not found");

            // delete physical files first, before the cascade delete removes the DB rows
            var attachments = await _attachmentRepository.GetByTaskIdAsync(id);
            foreach (var attachment in attachments)
            {
                await _fileStorage.DeleteFileAsync(attachment.StoredFileName);
            }

            await _repository.DeleteAsync(task);
            return ApiResponse.Ok("Task deleted successfully");
        }


        private static TaskDto MapToDto(TaskItem task, int attachmentCount) => new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            DueDate = task.DueDate,
            CreatedAt = task.CreatedAt,
            AttachmentCount = attachmentCount
        };
    }
}