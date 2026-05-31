namespace Bloodsport.Core.Models;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string SummonerName { get; set; } = string.Empty;
    public string RiotPuuid { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = "MID"; // TOP | JUNGLE | MID | ADC | SUPPORT

    // ── TrueSkill Foundation ────────────────────────────────────────────────
    public double TrueSkillMu { get; set; } = 25.0;
    public double TrueSkillSigma { get; set; } = 8.333;
    public double DisplayRating => TrueSkillMu - 3 * TrueSkillSigma;

    // ── BSR Components (updated after every match) ──────────────────────────
    public double BSR { get; set; }                          // final composite score
    public double PerformanceMultiplierRolling { get; set; } = 1.0;
    public double ConsistencyScore { get; set; } = 50.0;    // 0-100
    public double BuildIntelligenceScore { get; set; } = 50.0; // 0-100
    public double ExpressionIndex { get; set; }              // 0-100
    public string SignatureStyle { get; set; } = string.Empty;
    public int HonorsReceived { get; set; }

    // ── History ────────────────────────────────────────────────────────────
    public int GamesPlayed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMatchAt { get; set; }

    public bool IsRanked => GamesPlayed >= 5;
}
