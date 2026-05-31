using Microsoft.AspNetCore.SignalR;

namespace Bloodsport.Api.Hubs;

// Clients connect to this hub to receive live bracket updates.
// When a match result is recorded, the server broadcasts to all watchers of that tournament.
public class BracketHub : Hub
{
    public async Task JoinTournament(string tournamentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");
    }

    public async Task LeaveTournament(string tournamentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"tournament-{tournamentId}");
    }
}
