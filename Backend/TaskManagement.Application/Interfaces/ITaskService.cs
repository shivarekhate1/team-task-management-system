using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId, string currentUserRole);
    Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid currentUserId, string currentUserRole);
    Task<List<TaskDto>> GetTasksAsync(TaskFilterParams filter, Guid currentUserId, string currentUserRole);
    Task<TaskDetailDto> GetTaskByIdAsync(Guid taskId, Guid currentUserId, string currentUserRole);
    Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, TaskItemStatus status, Guid currentUserId, string currentUserRole);
    Task<TaskDto> AssignTaskAsync(Guid taskId, Guid? assignedToId, Guid currentUserId, string currentUserRole);
    Task DeleteTaskAsync(Guid taskId, Guid currentUserId, string currentUserRole);
}
