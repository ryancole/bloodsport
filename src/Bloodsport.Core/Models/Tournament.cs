namespace Bloodsport.Core.Models;

public enum TournamentStatus { Draft, Active, Completed }
public enum MatchStatus { Pending, InProgress, Completed }

public class Tournament
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TournamentStatus Status { get; set; } = TournamentStatus.Draft;
    public int RiotTournamentId { get; set; }
    public string DefinitionYaml { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public List<TournamentPlayer> Players { get; set; } = new();
    public List<Match> Matches { get; set; } = new();

    public Player? Champion => Matches
        .Where(m => m.Round == Matches.Max(x => x.Round) && m.Status == MatchStatus.Completed)
        .Select(m => m.Winner)
        .FirstOrDefault();
}

public class TournamentPlayer
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public Guid PlayerId { get; set; }
    public int Seed { get; set; }
    public double MuAtRegistration { get; set; }
    public double SigmaAtRegistration { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Player Player { get; set; } = null!;
}

public class Match
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public int Round { get; set; }
    public int MatchNumber { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Pending;
    public string? RiotTournamentCode { get; set; }
    public string? RiotMatchId { get; set; }

    public Guid? Player1Id { get; set; }
    public Guid? Player2Id { get; set; }
    public Guid? WinnerId { get; set; }

    public Player? Player1 { get; set; }
    public Player? Player2 { get; set; }
    public Player? Winner { get; set; }
    public Tournament Tournament { get; set; } = null!;

    // Which match slot the winner advances to
    public Guid? NextMatchId { get; set; }
    public int? NextMatchSlot { get; set; } // 1 = Player1 slot, 2 = Player2 slot
}
