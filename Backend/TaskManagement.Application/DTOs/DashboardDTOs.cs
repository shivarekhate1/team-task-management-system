namespace TaskManagement.Application.DTOs.Dashboard;

public record TaskPrioritySummary(
    int Low,
    int Medium,
    int High,
    int Urgent
);

public record UserTaskSummary(
    Guid UserId,
    string FullName,
    int TotalAssigned,
    int ToDoCount,
    int InProgressCount,
    int DoneCount
);

public record DashboardOverviewDto(
    int TotalTasks,
    int ToDoCount,
    int InProgressCount,
    int DoneCount,
    int OverdueCount,
    TaskPrioritySummary TasksByPriority,
    List<UserTaskSummary> PerUserSummaries,
    List<Notifications.NotificationDto> RecentNotifications
);
