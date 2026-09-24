using TaskManagement.Application.DTOs.Notifications;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task CreateNotificationAsync(Guid userId, string message, NotificationType type, Guid? taskId = null);
}
