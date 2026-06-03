using WarzoneTournament.Application.DTOs.Notification;

namespace WarzoneTournament.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateAsync(Guid userId, string title, string body, string type, string? link = null, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(Guid userId, int limit = 20, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task MarkReadAsync(Guid notificationId, CancellationToken ct = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
