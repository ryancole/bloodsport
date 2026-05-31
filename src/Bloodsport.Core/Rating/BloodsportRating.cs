namespace Bloodsport.Core.Rating;

/// <summary>
/// The full Bloodsport Rating (BSR) for a player.
/// Every component is public and transparent — no hidden numbers.
/// Shown in full on the player profile page.
/// </summary>
public class BloodsportRating
{
    public Guid PlayerId { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    // ── Component 1: TrueSkill Foundation ───────────────────────────────────
    // The conservative Bayesian skill estimate. Same system as Halo 3.
    // Display = mu - 3*sigma. You earn every point visibly.
    public double TrueSkillMu { get; set; }
    public double TrueSkillSigma { get; set; }
    public double TrueSkillBase => TrueSkillMu - 3 * TrueSkillSigma;

    // ── Component 2: Performance Multiplier ─────────────────────────────────
    // How well you played relative to your role benchmark.
    // Scales TrueSkill update magnitude: 0.6 (underperformed) → 1.4 (dominated).
    // Prevents winning badly from giving the same gain as winning brilliantly.
    public double PerformanceMultiplier { get; set; } = 1.0;
    public double LastMatchPerformanceScore { get; set; }  // 0-100, shown on profile
    public string PerformanceBreakdown { get; set; } = string.Empty; // "KDA: +, CS: +, Vision: -"

    // ── Component 3: Consistency Index ──────────────────────────────────────
    // Rolling variance of performance over last 10 games.
    // A player who performs reliably every game is more valuable than a streaky one.
    // Range: -2.0 (highly volatile) to +2.0 (extremely consistent)
    public double ConsistencyModifier { get; set; } = 0.0;
    public double ConsistencyScore { get; set; }   // 0-100, shown on profile
    public int GamesInConsistencyWindow { get; set; }

    // ── Component 4: Build Intelligence Score (BIS) ─────────────────────────
    // How efficiently you converted gold into power.
    // Measures: gold efficiency, item timing, situational adaptation.
    // 0-100. Contributes up to +5 BSR points for near-perfect building.
    public double BuildIntelligenceScore { get; set; }
    public double GoldEfficiencyAvg { get; set; }
    public double BuildTimingAvg { get; set; }
    public double SituationalAdaptationAvg { get; set; }

    // ── Component 5: Expression Index ───────────────────────────────────────
    // The most unique component in competitive gaming rating.
    // Measures individual creative expression: off-meta choices that WIN.
    // High deviation from meta + positive outcomes = high expression.
    // Following the meta efficiently = normal TrueSkill handles it.
    // 0-100. Contributes up to +3 BSR points.
    public double ExpressionIndex { get; set; }
    public double MetaDeviationScore { get; set; }    // how different from meta
    public double OutcomeCorrelation { get; set; }    // does the deviation work
    public string SignatureStyle { get; set; } = string.Empty; // e.g. "Off-meta builder", "Signature jungler"

    // ── Component 6: Honor Coefficient ──────────────────────────────────────
    // Sportsmanship. Post-match honors from Bloodsport participants only.
    // Small but real. Cannot be gamed — only other tournament players can honor you.
    // Consistent with the Kumite code: you honor the opponent whether you win or lose.
    public double HonorCoefficient { get; set; } = 0.0;
    public int HonorsReceived { get; set; }
    public int TournamentGamesPlayed { get; set; }

    // ── Final BSR ────────────────────────────────────────────────────────────
    public double BSR =>
        (TrueSkillBase * PerformanceMultiplier)
        + ConsistencyModifier
        + (BuildIntelligenceScore * 0.05)
        + (ExpressionIndex * 0.03)
        + HonorCoefficient;

    public bool IsRanked => TournamentGamesPlayed >= 5;

    public BloodsportTier Tier => IsRanked ? BloodsportTier.FromBSR(BSR) : BloodsportTier.Unranked;
}

/// <summary>
/// The Bloodsport ranking tiers.
/// Named for the Kumite — you do not choose your tier. The bracket assigns it.
/// </summary>
public class BloodsportTier
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;

    public static readonly BloodsportTier Unranked = new()
    {
        Name = "Unranked",
        Description = "Not yet tested. Play 5 matches to earn your rank.",
        CssClass = "tier-unranked"
    };

    public static BloodsportTier FromBSR(double bsr) => bsr switch
    {
        < 0    => new() { Name = "Initiate",  Description = "You have entered the arena.",                         CssClass = "tier-initiate"  },
        < 10   => new() { Name = "Initiate",  Description = "You have entered the arena.",                         CssClass = "tier-initiate"  },
        < 18   => new() { Name = "Warrior",   Description = "Proven in combat. The bracket respects you.",         CssClass = "tier-warrior"   },
        < 26   => new() { Name = "Veteran",   Description = "Tested across brackets. Consistency is your mark.",   CssClass = "tier-veteran"   },
        < 34   => new() { Name = "Elite",     Description = "Few reach this. Fewer stay.",                         CssClass = "tier-elite"     },
        < 42   => new() { Name = "Champion",  Description = "Tournament winners. The bracket said so.",            CssClass = "tier-champion"  },
        _      => new() { Name = "Kumite",    Description = "The final test. You did not choose this. It chose you.", CssClass = "tier-kumite" }
    };
}
