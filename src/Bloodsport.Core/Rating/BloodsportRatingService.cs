using Moserware.Skills;
using Bloodsport.Core.Models;

namespace Bloodsport.Core.Rating;

/// <summary>
/// The Bloodsport Rating Service — BSR.
///
/// A six-component rating system designed to express true player skill
/// more honestly than any binary win/loss system can.
///
/// Components:
///   1. TrueSkill Base        — Bayesian skill estimate (Halo 3 model)
///   2. Performance Multiplier — Individual performance vs role benchmark
///   3. Consistency Index     — Reliability across recent games
///   4. Build Intelligence    — Gold efficiency and adaptive building
///   5. Expression Index      — Off-meta creativity that wins
///   6. Honor Coefficient     — Sportsmanship within the tournament
///
/// All components are public and transparent on the player profile.
/// Nothing is hidden. The bracket finds the truth.
/// </summary>
public class BloodsportRatingService
{
    private readonly GameInfo _gameInfo;
    private readonly Dictionary<string, RoleBenchmark> _benchmarks;

    public BloodsportRatingService()
    {
        _gameInfo = new GameInfo(
            initialMean: 25.0,
            initialStandardDeviation: 25.0 / 3.0,
            beta: 25.0 / 6.0,
            dynamicsFactor: 25.0 / 300.0,
            drawProbability: 0.0);

        _benchmarks = RoleBenchmark.Defaults;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 1 — TrueSkill Update
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Updates TrueSkill ratings after a match.
    /// The performance multiplier from Component 2 scales the magnitude
    /// of the update — a dominant win moves ratings more than a narrow one.
    /// </summary>
    public (RatingUpdate Winner, RatingUpdate Loser) CalculateTrueSkillUpdate(
        Player winner, Player loser,
        double winnerPerformanceMultiplier = 1.0,
        double loserPerformanceMultiplier = 1.0)
    {
        var winnerRating = new Moserware.Skills.Rating(winner.TrueSkillMu, winner.TrueSkillSigma);
        var loserRating  = new Moserware.Skills.Rating(loser.TrueSkillMu, loser.TrueSkillSigma);

        var winnerTeam = new Team(new Moserware.Skills.Player(winner.Id), winnerRating);
        var loserTeam  = new Team(new Moserware.Skills.Player(loser.Id), loserRating);

        var teams      = Teams.Concat(winnerTeam, loserTeam);
        var newRatings = TrueSkillCalculator.CalculateNewRatings(_gameInfo, teams, 1, 2);

        var nw = newRatings[new Moserware.Skills.Player(winner.Id)];
        var nl = newRatings[new Moserware.Skills.Player(loser.Id)];

        // Apply performance multiplier — scales the delta, not the final value
        double winnerDeltaMu    = (nw.Mean - winner.TrueSkillMu) * winnerPerformanceMultiplier;
        double winnerDeltaSigma = (nw.StandardDeviation - winner.TrueSkillSigma);

        double loserDeltaMu    = (nl.Mean - loser.TrueSkillMu) * loserPerformanceMultiplier;
        double loserDeltaSigma = (nl.StandardDeviation - loser.TrueSkillSigma);

        return (
            new RatingUpdate(winner.Id, winner.TrueSkillMu + winnerDeltaMu, winner.TrueSkillSigma + winnerDeltaSigma),
            new RatingUpdate(loser.Id, loser.TrueSkillMu + loserDeltaMu, loser.TrueSkillSigma + loserDeltaSigma)
        );
    }

    public double WinProbability(Player player, Player opponent)
    {
        var deltaMu = player.TrueSkillMu - opponent.TrueSkillMu;
        var sumSigmaSquared = player.TrueSkillSigma * player.TrueSkillSigma
                            + opponent.TrueSkillSigma * opponent.TrueSkillSigma;
        var denom = Math.Sqrt(2 * _gameInfo.Beta * _gameInfo.Beta + sumSigmaSquared);
        return Phi(deltaMu / denom);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 2 — Performance Multiplier
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates how well a player performed relative to the benchmark for their role.
    /// Returns a multiplier 0.6–1.4 applied to the TrueSkill rating delta.
    ///
    /// Won and outperformed  → 1.4 (earn more for dominance)
    /// Won at benchmark      → 1.0 (standard gain)
    /// Won but underperformed→ 0.7 (you won but didn't play well)
    /// Lost but outperformed → 0.7 (you played well but lost — reduced penalty)
    /// Lost at benchmark     → 1.0 (standard loss)
    /// Lost and underperformed → 1.4 (compounded loss)
    /// </summary>
    public (double Multiplier, double PerformanceScore, string Breakdown) CalculatePerformanceMultiplier(
        MatchPerformance perf)
    {
        if (!_benchmarks.TryGetValue(perf.Role, out var bench))
            return (1.0, 50.0, "No benchmark available for role.");

        var scores = new List<(string Label, double Score)>();

        // KDA score
        double kdaScore = ScoreAgainstBenchmark(perf.KDA, bench.KDA);
        scores.Add(("KDA", kdaScore));

        // CS score (not weighted for support)
        if (perf.Role != "SUPPORT")
        {
            double csScore = ScoreAgainstBenchmark(perf.CSPerMinute, bench.CSPerMinute);
            scores.Add(("CS/min", csScore));
        }

        // Vision score
        double visionScore = ScoreAgainstBenchmark(perf.VisionScorePerMinute, bench.VisionScorePerMinute);
        scores.Add(("Vision", visionScore));

        // Damage efficiency (not for support)
        if (perf.Role != "SUPPORT")
        {
            double dmgScore = ScoreAgainstBenchmark(perf.DamagePerGold, bench.DamagePerGold);
            scores.Add(("Dmg/gold", dmgScore));
        }

        // Objective participation
        double objScore = ScoreAgainstBenchmark(perf.ObjectiveParticipationRate, bench.ObjectiveParticipationRate);
        scores.Add(("Objectives", objScore));

        // CC time for supports
        if (perf.Role == "SUPPORT" && perf.GameDurationMinutes > 0)
        {
            double ccPerMin = perf.CCTimeDealt / perf.GameDurationMinutes;
            double ccScore = ScoreAgainstBenchmark(ccPerMin, bench.CCTimePerMinute);
            scores.Add(("CC time", ccScore));
        }

        double avgScore = scores.Average(s => s.Score);

        // Build the breakdown string
        string breakdown = string.Join(", ", scores.Select(s =>
            $"{s.Label}: {(s.Score >= 60 ? "+" : s.Score >= 40 ? "~" : "-")}"));

        // Convert to multiplier — centered at 1.0, range 0.6–1.4
        double multiplier = perf.Won
            ? 0.7 + (avgScore / 100.0) * 0.7   // won: 0.7 → 1.4
            : 1.4 - (avgScore / 100.0) * 0.8;   // lost: 0.6 → 1.4 (inverted — better play = lower penalty)

        multiplier = Math.Clamp(multiplier, 0.6, 1.4);

        return (multiplier, avgScore, breakdown);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 3 — Consistency Index
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Measures how reliably a player performs across recent games.
    /// Uses coefficient of variation on performance scores over the last 10 games.
    /// A streaky player (15/0 one game, 0/12 next) scores lower than a steady one.
    /// Returns a modifier -2.0 to +2.0 added directly to BSR display.
    /// </summary>
    public (double Modifier, double ConsistencyScore) CalculateConsistencyModifier(
        IEnumerable<double> recentPerformanceScores)
    {
        var scores = recentPerformanceScores.Take(10).ToList();
        if (scores.Count < 3)
            return (0.0, 50.0); // not enough data

        double mean = scores.Average();
        double variance = scores.Average(s => Math.Pow(s - mean, 2));
        double stdDev = Math.Sqrt(variance);

        // Coefficient of variation — normalized measure of variance
        double cv = mean > 0 ? stdDev / mean : 1.0;

        // Consistency score 0-100: lower CV = more consistent = higher score
        double consistencyScore = Math.Max(0, 100 - (cv * 100));

        // Map to -2 → +2 modifier
        double modifier = (consistencyScore - 50.0) / 25.0; // centered at 0
        modifier = Math.Clamp(modifier, -2.0, 2.0);

        return (modifier, consistencyScore);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 4 — Build Intelligence Score (BIS)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Measures how efficiently and intelligently a player builds items.
    ///
    /// Three sub-scores:
    ///   Gold Efficiency      — Did purchased items maximize stat value per gold?
    ///   Build Timing         — Were core items completed at optimal windows?
    ///   Situational Adaption — Did the build respond to the enemy composition?
    ///
    /// Returns a score 0-100. Contributes up to +5 BSR points.
    ///
    /// NOTE: In beta mode, these scores are passed in directly from mock data.
    /// When Tournament API key arrives, wire RiotBuildAnalyzer to populate them.
    /// </summary>
    public double CalculateBuildIntelligenceScore(MatchPerformance perf)
    {
        // Weighted composite of three sub-scores
        double bis =
            (perf.GoldEfficiencyScore * 100 * 0.40) +   // 40% weight — core efficiency
            (perf.BuildTimingScore * 100 * 0.35) +       // 35% weight — timing matters
            (perf.SituationalAdaptationScore * 100 * 0.25); // 25% — adaptation is hardest

        return Math.Clamp(bis, 0, 100);
    }

    /// <summary>
    /// Rolling average BIS across recent matches.
    /// </summary>
    public double CalculateRollingBIS(IEnumerable<double> recentBISScores) =>
        recentBISScores.Any() ? recentBISScores.Take(10).Average() : 50.0;

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 5 — Expression Index
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// The most unique component in competitive gaming rating.
    ///
    /// Measures individual creative expression — off-meta choices that WIN.
    /// Formula: Expression = MetaDeviation × OutcomeCorrelation
    ///
    /// High deviation + wins   = authentic mastery. Rewarded.
    /// High deviation + losses = creative but ineffective. Not rewarded.
    /// Low deviation + wins    = skilled but conventional. TrueSkill handles it.
    ///
    /// This rewards the player who has internalized the game deeply enough
    /// to break the meta intentionally and productively — like Dux fighting blind.
    /// Frank Dux did not follow the textbook in the final match.
    /// He played something that was completely his own. And it worked.
    ///
    /// Returns a score 0-100. Contributes up to +3 BSR points.
    /// Also generates a "Signature Style" label for the player profile.
    /// </summary>
    public (double ExpressionIndex, double MetaDeviation, double OutcomeCorrelation, string Signature)
        CalculateExpressionIndex(IEnumerable<MatchPerformance> recentMatches)
    {
        var matches = recentMatches.Take(20).ToList();
        if (matches.Count < 5)
            return (0, 0, 0, "Not enough games to establish signature.");

        // Meta Deviation — how far from population averages
        double avgChampDeviation = matches.Average(m => 1.0 - m.ChampionPickRate);
        double avgBuildDeviation = matches.Average(m => m.BuildDivergenceScore);
        double avgPlayDeviation  = matches.Average(m => m.PlayPatternDeviation);

        double metaDeviation = (avgChampDeviation * 0.35) +
                               (avgBuildDeviation * 0.40) +
                               (avgPlayDeviation  * 0.25);
        metaDeviation = Math.Clamp(metaDeviation, 0, 1.0);

        // Outcome Correlation — does the deviation produce wins?
        // Only count high-deviation games in the outcome check
        var highDeviationGames = matches.Where(m =>
            (1 - m.ChampionPickRate) > 0.5 ||
            m.BuildDivergenceScore > 0.5 ||
            m.PlayPatternDeviation > 0.5).ToList();

        double outcomeCorrelation = highDeviationGames.Count > 0
            ? highDeviationGames.Count(m => m.Won) / (double)highDeviationGames.Count
            : 0.5;

        // Expression Index — deviation only counts if it works
        double expressionIndex = metaDeviation * outcomeCorrelation * 100;
        expressionIndex = Math.Clamp(expressionIndex, 0, 100);

        // Generate signature label
        string signature = GenerateSignatureLabel(
            avgChampDeviation, avgBuildDeviation, avgPlayDeviation,
            outcomeCorrelation, matches);

        return (expressionIndex, metaDeviation * 100, outcomeCorrelation * 100, signature);
    }

    private string GenerateSignatureLabel(
        double champDeviation, double buildDeviation, double playDeviation,
        double outcomeCorrelation, List<MatchPerformance> matches)
    {
        if (outcomeCorrelation < 0.4)
            return "Experimenting"; // creative but not yet effective

        var labels = new List<string>();

        if (champDeviation > 0.7)
            labels.Add("Off-meta specialist");
        else if (champDeviation > 0.4)
            labels.Add("Niche pick player");

        if (buildDeviation > 0.7)
            labels.Add("Unconventional builder");
        else if (buildDeviation > 0.4)
            labels.Add("Adaptive builder");

        if (playDeviation > 0.7)
            labels.Add("Unpredictable");
        else if (playDeviation > 0.4)
            labels.Add("Pattern breaker");

        // Role-specific signature
        var primaryRole = matches.GroupBy(m => m.Role)
            .OrderByDescending(g => g.Count())
            .First().Key;

        if (!labels.Any())
            return primaryRole switch
            {
                "JUNGLE"  => "Methodical jungler",
                "SUPPORT" => "Visionary support",
                "ADC"     => "Precision marksman",
                "MID"     => "Calculated mid laner",
                "TOP"     => "Disciplined top laner",
                _         => "Consistent performer"
            };

        return string.Join(" / ", labels.Take(2));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // COMPONENT 6 — Honor Coefficient
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sportsmanship coefficient from post-match honors within Bloodsport tournaments.
    /// Cannot be gamed — only tournament participants can honor each other.
    /// Small contribution (0.5% per honor) but visible and permanent on the profile.
    /// Consistent with the Kumite code: you honor the opponent regardless of outcome.
    /// </summary>
    public double CalculateHonorCoefficient(int honorsReceived, int gamesPlayed)
    {
        if (gamesPlayed == 0) return 0;
        double honorsPerGame = honorsReceived / (double)gamesPlayed;
        // Cap at +2.0 — meaningful but not dominant
        return Math.Min(honorsPerGame * 0.5, 2.0);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // FULL BSR CALCULATION
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Computes the full BloodsportRating from a player's history.
    /// Called after every match completes.
    /// </summary>
    public BloodsportRating ComputeFullRating(
        Player player,
        IEnumerable<MatchPerformance> recentMatches,
        IEnumerable<double> recentPerformanceScores,
        int honorsReceived)
    {
        var matches = recentMatches.ToList();
        var perfScores = recentPerformanceScores.ToList();

        // Latest match performance
        var latestMatch = matches.FirstOrDefault();
        var (perfMultiplier, perfScore, perfBreakdown) = latestMatch != null
            ? CalculatePerformanceMultiplier(latestMatch)
            : (1.0, 50.0, "No match data");

        // Consistency
        var (consistency, consistencyScore) = CalculateConsistencyModifier(perfScores);

        // Build intelligence
        double rollingBIS = matches.Any()
            ? CalculateRollingBIS(matches.Select(m => CalculateBuildIntelligenceScore(m)))
            : 50.0;

        double goldEffAvg = matches.Any() ? matches.Average(m => m.GoldEfficiencyScore * 100) : 50;
        double timingAvg  = matches.Any() ? matches.Average(m => m.BuildTimingScore * 100) : 50;
        double adaptAvg   = matches.Any() ? matches.Average(m => m.SituationalAdaptationScore * 100) : 50;

        // Expression index
        var (expressionIdx, metaDev, outcomeCor, signature) = matches.Count >= 5
            ? CalculateExpressionIndex(matches)
            : (0.0, 0.0, 0.0, "Not enough games to establish signature.");

        // Honor
        double honor = CalculateHonorCoefficient(honorsReceived, player.GamesPlayed);

        return new BloodsportRating
        {
            PlayerId                  = player.Id,
            TrueSkillMu               = player.TrueSkillMu,
            TrueSkillSigma            = player.TrueSkillSigma,
            PerformanceMultiplier     = perfMultiplier,
            LastMatchPerformanceScore = perfScore,
            PerformanceBreakdown      = perfBreakdown,
            ConsistencyModifier       = consistency,
            ConsistencyScore          = consistencyScore,
            GamesInConsistencyWindow  = Math.Min(perfScores.Count, 10),
            BuildIntelligenceScore    = rollingBIS,
            GoldEfficiencyAvg         = goldEffAvg,
            BuildTimingAvg            = timingAvg,
            SituationalAdaptationAvg  = adaptAvg,
            ExpressionIndex           = expressionIdx,
            MetaDeviationScore        = metaDev,
            OutcomeCorrelation        = outcomeCor,
            SignatureStyle            = signature,
            HonorCoefficient          = honor,
            HonorsReceived            = honorsReceived,
            TournamentGamesPlayed     = player.GamesPlayed
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // MOCK DATA — for beta testing before Riot API key arrives
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates a realistic mock MatchPerformance for beta testing.
    /// Ryan: replace calls to this with real Riot API data when Tournament API is live.
    /// </summary>
    public static MatchPerformance GenerateMockPerformance(
        Guid playerId, Guid matchId, bool won, string role = "MID")
    {
        var rng = new Random();
        double quality = 0.3 + rng.NextDouble() * 0.7; // 0.3–1.0

        return new MatchPerformance
        {
            PlayerId             = playerId,
            MatchId              = matchId,
            RiotMatchId          = $"MOCK_{matchId}",
            Won                  = won,
            Role                 = role,
            GameDurationMinutes  = 25 + rng.NextDouble() * 20,
            Kills                = (int)(quality * 10 * rng.NextDouble()),
            Deaths               = (int)((1 - quality) * 8 * rng.NextDouble()) + 1,
            Assists              = (int)(quality * 12 * rng.NextDouble()),
            CSPerMinute          = 5.0 + quality * 4.0 + rng.NextDouble(),
            VisionScore          = quality * 40 + rng.NextDouble() * 20,
            DamageDealt          = 15000 + quality * 25000 * rng.NextDouble(),
            GoldEarned           = 8000 + quality * 8000 * rng.NextDouble(),
            ObjectiveParticipationRate = 0.3 + quality * 0.5 * rng.NextDouble(),
            ChampionId           = "MOCK_CHAMPION",
            ChampionPickRate     = rng.NextDouble(),
            GoldEfficiencyScore  = 0.5 + quality * 0.4,
            BuildTimingScore     = 0.4 + quality * 0.5,
            SituationalAdaptationScore = 0.3 + quality * 0.6,
            BuildDivergenceScore = rng.NextDouble(),
            PlayPatternDeviation = rng.NextDouble(),
            FinalItems           = new List<int> { 3153, 3006, 3031, 3072, 3036, 3026 }
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scores a metric against a benchmark.
    /// 100 = double the benchmark (exceptional)
    /// 50  = at benchmark (solid)
    /// 0   = zero or far below benchmark
    /// </summary>
    private static double ScoreAgainstBenchmark(double value, double benchmark)
    {
        if (benchmark <= 0) return 50;
        double ratio = value / benchmark;
        return Math.Clamp(ratio * 50, 0, 100);
    }

    private static double Phi(double x)
    {
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2.0)));
    }

    private static double Erf(double x)
    {
        const double a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741;
        const double a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911;
        double sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
        return sign * y;
    }
}

public record RatingUpdate(Guid PlayerId, double NewMu, double NewSigma);
