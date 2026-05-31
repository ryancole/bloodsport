namespace Bloodsport.Core.Rating;

/// <summary>
/// Raw performance data from a single match pulled from the Riot API.
/// All fields are populated after a match completes via the tournament callback.
/// During beta, this is mocked — wire RiotMatchId to real data when Tournament API key arrives.
/// </summary>
public class MatchPerformance
{
    public Guid PlayerId { get; set; }
    public Guid MatchId { get; set; }
    public string RiotMatchId { get; set; } = string.Empty;
    public bool Won { get; set; }
    public double GameDurationMinutes { get; set; }

    // Role
    public string Role { get; set; } = "MID"; // TOP | JUNGLE | MID | ADC | SUPPORT

    // Core stats
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public double KDA => Deaths == 0 ? Kills + Assists : (Kills + Assists) / (double)Deaths;

    public double CSPerMinute { get; set; }
    public double VisionScore { get; set; }
    public double VisionScorePerMinute => GameDurationMinutes > 0 ? VisionScore / GameDurationMinutes : 0;
    public double DamageDealt { get; set; }
    public double GoldEarned { get; set; }
    public double DamagePerGold => GoldEarned > 0 ? DamageDealt / GoldEarned : 0;
    public double ObjectiveParticipationRate { get; set; } // 0.0 - 1.0
    public double CCTimeDealt { get; set; }                // seconds — support metric

    // Build data (from Riot API item IDs)
    public List<int> FinalItems { get; set; } = new();
    public string ChampionId { get; set; } = string.Empty;
    public double GoldEfficiencyScore { get; set; }       // 0.0 - 1.0 calculated by BuildAnalyzer
    public double BuildTimingScore { get; set; }           // 0.0 - 1.0 how close to optimal timing
    public double SituationalAdaptationScore { get; set; } // 0.0 - 1.0 did build respond to game state
    public List<ItemPurchaseEvent> ItemTimeline { get; set; } = new();

    // Expression data
    public double ChampionPickRate { get; set; }   // 0.0 - 1.0 population pick rate this patch
    public double BuildDivergenceScore { get; set; } // 0.0 - 1.0 how different from top build
    public double PlayPatternDeviation { get; set; } // 0.0 - 1.0 how unique positioning/patterns
}

public class ItemPurchaseEvent
{
    public int ItemId { get; set; }
    public double MinuteCompleted { get; set; }
    public int GoldSpent { get; set; }
}

/// <summary>
/// Role-specific performance benchmarks.
/// These represent approximate Platinum/Diamond level play.
/// Ryan: tune these values as tournament data accumulates.
/// </summary>
public class RoleBenchmark
{
    public string Role { get; set; } = string.Empty;
    public double KDA { get; set; }
    public double CSPerMinute { get; set; }
    public double VisionScorePerMinute { get; set; }
    public double DamagePerGold { get; set; }
    public double ObjectiveParticipationRate { get; set; }
    public double CCTimePerMinute { get; set; } // support only

    public static Dictionary<string, RoleBenchmark> Defaults => new()
    {
        ["TOP"] = new()
        {
            Role = "TOP", KDA = 2.5, CSPerMinute = 7.5,
            VisionScorePerMinute = 0.8, DamagePerGold = 0.85,
            ObjectiveParticipationRate = 0.55, CCTimePerMinute = 0.3
        },
        ["JUNGLE"] = new()
        {
            Role = "JUNGLE", KDA = 3.0, CSPerMinute = 5.5,
            VisionScorePerMinute = 1.0, DamagePerGold = 0.80,
            ObjectiveParticipationRate = 0.75, CCTimePerMinute = 0.4
        },
        ["MID"] = new()
        {
            Role = "MID", KDA = 2.8, CSPerMinute = 7.8,
            VisionScorePerMinute = 0.9, DamagePerGold = 0.90,
            ObjectiveParticipationRate = 0.60, CCTimePerMinute = 0.2
        },
        ["ADC"] = new()
        {
            Role = "ADC", KDA = 3.2, CSPerMinute = 8.2,
            VisionScorePerMinute = 0.7, DamagePerGold = 0.95,
            ObjectiveParticipationRate = 0.65, CCTimePerMinute = 0.1
        },
        ["SUPPORT"] = new()
        {
            Role = "SUPPORT", KDA = 2.5, CSPerMinute = 0.5,
            VisionScorePerMinute = 2.5, DamagePerGold = 0.40,
            ObjectiveParticipationRate = 0.80, CCTimePerMinute = 1.8
        }
    };
}
