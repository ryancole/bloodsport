namespace Bloodsport.Core.Models;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string SummonerName { get; set; } = string.Empty;
    public string RiotPuuid { get; set; } = string.Empty;
    public double TrueSkillMu { get; set; } = 25.0;
    public double TrueSkillSigma { get; set; } = 8.333;
    public int GamesPlayed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Halo 3 style: conservative display rating = mu - 3*sigma
    // Shown as "Unranked" until enough games played to be meaningful
    public double DisplayRating => TrueSkillMu - 3 * TrueSkillSigma;
    public bool IsRanked => GamesPlayed >= 5;
}
