using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Message,
    NotificationType Type,
    bool IsRead,
    Guid? TaskId,
    DateTime CreatedAt
);
