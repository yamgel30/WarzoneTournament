using Microsoft.AspNetCore.SignalR;

namespace WarzoneTournament.Web.Hubs;

public class TournamentHub : Hub
{
    private readonly ILogger<TournamentHub> _logger;

    public TournamentHub(ILogger<TournamentHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinTournamentGroup(string tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");
        _logger.LogDebug("Client {ConnectionId} joined tournament group {TournamentId}",
            Context.ConnectionId, tournamentId);
    }

    public async Task LeaveTournamentGroup(string tournamentId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");

    public async Task JoinMatchGroup(string matchId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");

    public async Task LeaveMatchGroup(string matchId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");

    public async Task JoinAdminGroup()
        => await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogDebug("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
