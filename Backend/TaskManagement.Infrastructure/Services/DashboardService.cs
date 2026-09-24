using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs.Dashboard;
using TaskManagement.Application.DTOs.Notifications;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(Guid currentUserId, string currentUserRole)
    {
        IQueryable<TaskItem> taskQuery = _dbContext.Tasks;

        if (currentUserRole == UserRole.Admin.ToString())
        {
            // Admins see all tasks in dashboard
        }
        else if (currentUserRole == UserRole.Manager.ToString())
        {
            var managedTeamIds = await _dbContext.Teams
                .Where(t => t.ManagerId == currentUserId || t.Members.Any(m => m.UserId == currentUserId))
                .Select(t => t.Id)
                .ToListAsync();

            taskQuery = taskQuery.Where(t => t.CreatedById == currentUserId ||
                                             t.AssignedToId == currentUserId ||
                                             (t.TeamId.HasValue && managedTeamIds.Contains(t.TeamId.Value)));
        }
        else
        {
            var userTeamIds = await _dbContext.TeamMembers
                .Where(tm => tm.UserId == currentUserId)
                .Select(tm => tm.TeamId)
                .ToListAsync();

            taskQuery = taskQuery.Where(t => t.AssignedToId == currentUserId ||
                                             t.CreatedById == currentUserId ||
                                             (t.TeamId.HasValue && userTeamIds.Contains(t.TeamId.Value)));
        }

        var totalTasks = await taskQuery.CountAsync();
        var toDoCount = await taskQuery.CountAsync(t => t.Status == TaskItemStatus.ToDo);
        var inProgressCount = await taskQuery.CountAsync(t => t.Status == TaskItemStatus.InProgress);
        var doneCount = await taskQuery.CountAsync(t => t.Status == TaskItemStatus.Done);

        var now = DateTime.UtcNow;
        var overdueCount = await taskQuery.CountAsync(t => t.Deadline < now && t.Status != TaskItemStatus.Done);

        var lowPriority = await taskQuery.CountAsync(t => t.Priority == TaskPriority.Low);
        var mediumPriority = await taskQuery.CountAsync(t => t.Priority == TaskPriority.Medium);
        var highPriority = await taskQuery.CountAsync(t => t.Priority == TaskPriority.High);
        var urgentPriority = await taskQuery.CountAsync(t => t.Priority == TaskPriority.Urgent);

        var prioritySummary = new TaskPrioritySummary(lowPriority, mediumPriority, highPriority, urgentPriority);

        // Per-user breakdown
        var users = await _dbContext.Users.Where(u => u.IsActive).ToListAsync();
        var perUserSummaries = new List<UserTaskSummary>();

        foreach (var user in users)
        {
            var userAssignedTasks = taskQuery.Where(t => t.AssignedToId == user.Id);
            var assignedCount = await userAssignedTasks.CountAsync();
            if (assignedCount > 0 || currentUserRole == UserRole.Admin.ToString())
            {
                var userToDo = await userAssignedTasks.CountAsync(t => t.Status == TaskItemStatus.ToDo);
                var userInProgress = await userAssignedTasks.CountAsync(t => t.Status == TaskItemStatus.InProgress);
                var userDone = await userAssignedTasks.CountAsync(t => t.Status == TaskItemStatus.Done);

                perUserSummaries.Add(new UserTaskSummary(
                    user.Id, user.FullName, assignedCount, userToDo, userInProgress, userDone
                ));
            }
        }

        // Recent Notifications
        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto(
                n.Id, n.UserId, n.Message, n.Type, n.IsRead, n.TaskId, n.CreatedAt
            ))
            .ToListAsync();

        return new DashboardOverviewDto(
            totalTasks,
            toDoCount,
            inProgressCount,
            doneCount,
            overdueCount,
            prioritySummary,
            perUserSummaries,
            notifications
        );
    }
}
