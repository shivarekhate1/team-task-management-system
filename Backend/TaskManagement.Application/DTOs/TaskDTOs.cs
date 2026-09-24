using TaskManagement.Domain.Enums;
using TaskManagement.Application.DTOs.Comments;

namespace TaskManagement.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime Deadline,
    Guid? TeamId,
    Guid? AssignedToId
);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime Deadline,
    Guid? TeamId,
    Guid? AssignedToId
);

public record UpdateTaskStatusRequest(
    TaskItemStatus Status
);

public record AssignTaskRequest(
    Guid? AssignedToId
);

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime Deadline,
    Guid? TeamId,
    string? TeamName,
    Guid? AssignedToId,
    string? AssignedToName,
    Guid CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int CommentCount
);

public record TaskDetailDto(
    Guid Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime Deadline,
    Guid? TeamId,
    string? TeamName,
    Guid? AssignedToId,
    string? AssignedToName,
    Guid CreatedById,
    string CreatedByName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<CommentDto> Comments
);

public class TaskFilterParams
{
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? CreatedById { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDeadline { get; set; }
    public DateTime? ToDeadline { get; set; }
}
