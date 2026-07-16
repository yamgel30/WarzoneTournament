namespace WarzoneTournament.Application.DTOs.Notification;

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string? Link,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);
