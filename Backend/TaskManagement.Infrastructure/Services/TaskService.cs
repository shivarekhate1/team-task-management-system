using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Comments;
using TaskManagement.Application.DTOs.Tasks;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public TaskService(ApplicationDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, Guid currentUserId, string currentUserRole)
    {
        ValidateTaskRequest(request.Title, request.AssignedToId, request.TeamId);

        if (request.AssignedToId.HasValue)
        {
            await ValidateAssigneeBelongsToTeamAsync(request.AssignedToId.Value, request.TeamId);
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = TaskItemStatus.ToDo,
            Deadline = request.Deadline,
            TeamId = request.TeamId,
            AssignedToId = request.AssignedToId,
            CreatedById = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Tasks.AddAsync(task);
        await _dbContext.SaveChangesAsync();

        // Trigger notification if assigned
        if (task.AssignedToId.HasValue && task.AssignedToId.Value != currentUserId)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToId.Value,
                $"You have been assigned a new task: {task.Title}",
                NotificationType.TaskAssigned,
                task.Id
            );
        }

        return await GetTaskDtoByIdAsync(task.Id);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanManageTask(task, currentUserId, currentUserRole);

        ValidateTaskRequest(request.Title, request.AssignedToId, request.TeamId);

        var previousAssignee = task.AssignedToId;

        if (request.AssignedToId.HasValue && request.AssignedToId != previousAssignee)
        {
            await ValidateAssigneeBelongsToTeamAsync(request.AssignedToId.Value, request.TeamId);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.Deadline = request.Deadline;
        task.TeamId = request.TeamId;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Trigger notification if newly assigned to someone else
        if (task.AssignedToId.HasValue && task.AssignedToId != previousAssignee && task.AssignedToId.Value != currentUserId)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToId.Value,
                $"You have been assigned a task: {task.Title}",
                NotificationType.TaskAssigned,
                task.Id
            );
        }

        return await GetTaskDtoByIdAsync(task.Id);
    }

    public async Task<List<TaskDto>> GetTasksAsync(TaskFilterParams filter, Guid currentUserId, string currentUserRole)
    {
        IQueryable<TaskItem> query = _dbContext.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments);

        // Scope access based on role
        if (currentUserRole == UserRole.Admin.ToString())
        {
            // Admins see all tasks
        }
        else if (currentUserRole == UserRole.Manager.ToString())
        {
            // Managers see tasks created by them, assigned to them, or belonging to teams they manage
            var managedTeamIds = await _dbContext.Teams
                .Where(tm => tm.ManagerId == currentUserId || tm.Members.Any(m => m.UserId == currentUserId))
                .Select(tm => tm.Id)
                .ToListAsync();

            query = query.Where(t => t.CreatedById == currentUserId ||
                                     t.AssignedToId == currentUserId ||
                                     (t.TeamId.HasValue && managedTeamIds.Contains(t.TeamId.Value)));
        }
        else
        {
            // Regular users see tasks assigned to them, created by them, or in teams they belong to
            var userTeamIds = await _dbContext.TeamMembers
                .Where(tm => tm.UserId == currentUserId)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            query = query.Where(t => t.AssignedToId == currentUserId ||
                                     t.CreatedById == currentUserId ||
                                     (t.TeamId.HasValue && userTeamIds.Contains(t.TeamId.Value)));
        }

        // Apply filters
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        if (filter.TeamId.HasValue)
        {
            query = query.Where(t => t.TeamId == filter.TeamId.Value);
        }

        if (filter.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);
        }

        if (filter.CreatedById.HasValue)
        {
            query = query.Where(t => t.CreatedById == filter.CreatedById.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) || (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        if (filter.FromDeadline.HasValue)
        {
            query = query.Where(t => t.Deadline >= filter.FromDeadline.Value);
        }

        if (filter.ToDeadline.HasValue)
        {
            query = query.Where(t => t.Deadline <= filter.ToDeadline.Value);
        }

        var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

        return tasks.Select(MapToTaskDto).ToList();
    }

    public async Task<TaskDetailDto> GetTaskByIdAsync(Guid taskId, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks
            .Include(t => t.Team)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanAccessTask(task, currentUserId, currentUserRole);

        var comments = task.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentDto(
            c.Id, c.TaskId, c.UserId, c.User.FullName, c.CommentText, c.CreatedAt, c.UpdatedAt
        )).ToList();

        return new TaskDetailDto(
            task.Id, task.Title, task.Description, task.Priority, task.Status, task.Deadline,
            task.TeamId, task.Team?.Name, task.AssignedToId, task.AssignedTo?.FullName,
            task.CreatedById, task.CreatedBy.FullName, task.CreatedAt, task.UpdatedAt, comments
        );
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(Guid taskId, TaskItemStatus status, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks.Include(t => t.AssignedTo).Include(t => t.CreatedBy).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanAccessTask(task, currentUserId, currentUserRole);

        var oldStatus = task.Status;
        if (oldStatus == status)
        {
            return await GetTaskDtoByIdAsync(taskId);
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Create notification for status update
        var statusMessage = $"Task '{task.Title}' status changed to {FormatStatus(status)}.";

        // Notify task creator if creator is not the person making the change
        if (task.CreatedById != currentUserId)
        {
            await _notificationService.CreateNotificationAsync(
                task.CreatedById,
                statusMessage,
                NotificationType.StatusUpdated,
                task.Id
            );
        }

        // Notify assignee if assignee is not the person making the change
        if (task.AssignedToId.HasValue && task.AssignedToId.Value != currentUserId && task.AssignedToId.Value != task.CreatedById)
        {
            await _notificationService.CreateNotificationAsync(
                task.AssignedToId.Value,
                statusMessage,
                NotificationType.StatusUpdated,
                task.Id
            );
        }

        return await GetTaskDtoByIdAsync(taskId);
    }

    public async Task<TaskDto> AssignTaskAsync(Guid taskId, Guid? assignedToId, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanManageTask(task, currentUserId, currentUserRole);

        if (assignedToId.HasValue)
        {
            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == assignedToId.Value);
            if (!userExists)
            {
                throw new NotFoundException($"User with ID '{assignedToId.Value}' not found.");
            }

            await ValidateAssigneeBelongsToTeamAsync(assignedToId.Value, task.TeamId);
        }

        var previousAssignee = task.AssignedToId;
        task.AssignedToId = assignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Send Notification if newly assigned
        if (assignedToId.HasValue && assignedToId != previousAssignee && assignedToId.Value != currentUserId)
        {
            await _notificationService.CreateNotificationAsync(
                assignedToId.Value,
                $"You have been assigned a task: {task.Title}",
                NotificationType.TaskAssigned,
                task.Id
            );
        }

        return await GetTaskDtoByIdAsync(taskId);
    }

    public async Task DeleteTaskAsync(Guid taskId, Guid currentUserId, string currentUserRole)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        if (task == null)
        {
            throw new NotFoundException($"Task with ID '{taskId}' not found.");
        }

        EnsureCanManageTask(task, currentUserId, currentUserRole);

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();
    }

    private static void ValidateTaskRequest(string title, Guid? assignedToId, Guid? teamId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ValidationException("Task title cannot be empty.");
        }
    }

    private async Task ValidateAssigneeBelongsToTeamAsync(Guid userId, Guid? teamId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException($"Assigned user '{userId}' does not exist.");
        }

        if (teamId.HasValue)
        {
            var isMember = await _dbContext.TeamMembers.AnyAsync(tm => tm.TeamId == teamId.Value && tm.UserId == userId);
            var isManager = await _dbContext.Teams.AnyAsync(t => t.Id == teamId.Value && t.ManagerId == userId);

            if (!isMember && !isManager && user.Role != UserRole.Admin)
            {
                throw new ValidationException($"User '{user.FullName}' does not belong to the assigned team.");
            }
        }
    }

    private static void EnsureCanManageTask(TaskItem task, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.Admin.ToString()) return;
        if (currentUserRole == UserRole.Manager.ToString() && (task.CreatedById == currentUserId || task.AssignedToId == currentUserId)) return;

        throw new ForbiddenException("You do not have permission to manage this task.");
    }

    private void EnsureCanAccessTask(TaskItem task, Guid currentUserId, string currentUserRole)
    {
        if (currentUserRole == UserRole.Admin.ToString()) return;
        if (task.CreatedById == currentUserId || task.AssignedToId == currentUserId) return;

        if (task.TeamId.HasValue)
        {
            var isMember = _dbContext.TeamMembers.Any(tm => tm.TeamId == task.TeamId.Value && tm.UserId == currentUserId);
            var isManager = _dbContext.Teams.Any(t => t.Id == task.TeamId.Value && t.ManagerId == currentUserId);

            if (isMember || isManager) return;
        }

        throw new ForbiddenException("You are not authorized to view or modify this task.");
    }

    private async Task<TaskDto> GetTaskDtoByIdAsync(Guid taskId)
    {
        var t = await _dbContext.Tasks
            .Include(task => task.Team)
            .Include(task => task.AssignedTo)
            .Include(task => task.CreatedBy)
            .Include(task => task.Comments)
            .FirstAsync(task => task.Id == taskId);

        return MapToTaskDto(t);
    }

    private static TaskDto MapToTaskDto(TaskItem t) =>
        new(
            t.Id, t.Title, t.Description, t.Priority, t.Status, t.Deadline,
            t.TeamId, t.Team?.Name, t.AssignedToId, t.AssignedTo?.FullName,
            t.CreatedById, t.CreatedBy?.FullName ?? "Unknown",
            t.CreatedAt, t.UpdatedAt, t.Comments.Count
        );

    private static string FormatStatus(TaskItemStatus status) => status switch
    {
        TaskItemStatus.ToDo => "To Do",
        TaskItemStatus.InProgress => "In Progress",
        TaskItemStatus.Done => "Done",
        _ => status.ToString()
    };
}
