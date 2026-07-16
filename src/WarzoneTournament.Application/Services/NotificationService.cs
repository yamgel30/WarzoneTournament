using WarzoneTournament.Application.Common.Interfaces;
using WarzoneTournament.Application.DTOs.Notification;
using WarzoneTournament.Domain.Entities;
using WarzoneTournament.Domain.Interfaces;

namespace WarzoneTournament.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly ISignalRNotificationService _signalR;

    public NotificationService(IUnitOfWork uow, ISignalRNotificationService signalR)
    {
        _uow = uow;
        _signalR = signalR;
    }

    public async Task CreateAsync(Guid userId, string title, string body, string type, string? link = null, CancellationToken ct = default)
    {
        var notification = new AppNotification
        {
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            Link = link,
            IsRead = false
        };
        await _uow.Notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        var dto = ToDto(notification);
        await _signalR.NotifyUserAsync(userId.ToString(), dto, ct);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(Guid userId, int limit = 20, CancellationToken ct = default)
    {
        var all = await _uow.Notifications.FindAsync(n => n.UserId == userId, ct);
        return all.OrderByDescending(n => n.CreatedAt)
                  .Take(limit)
                  .Select(ToDto)
                  .ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        => await _uow.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public async Task MarkReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var n = await _uow.Notifications.GetByIdAsync(notificationId, ct);
        if (n is null) return;
        n.IsRead = true;
        _uow.Notifications.Update(n);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await _uow.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead, ct);
        foreach (var n in unread) { n.IsRead = true; _uow.Notifications.Update(n); }
        if (unread.Any()) await _uow.SaveChangesAsync(ct);
    }

    private static NotificationDto ToDto(AppNotification n) =>
        new(n.Id, n.Title, n.Body, n.Link, n.Type, n.IsRead, n.CreatedAt);
}
