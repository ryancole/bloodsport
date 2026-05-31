namespace Bloodsport.Core.Teams;

/// <summary>
/// The Bloodsport Team Formation Service.
///
/// Teams are statistically-guided random assignments.
/// They feel random — and they are random — but the algorithm
/// ensures every team has:
///   1. Full role coverage (Top/Jungle/Mid/ADC/Support)
///   2. Internal playstyle chemistry (aggression balanced with utility)
///   3. Champion pool depth (multiple viable win conditions)
///   4. Competitive balance between teams (similar average BSR)
///
/// No player chooses their team. No team is rigged.
/// The data decides. Your job is to play.
///
/// This is the Kumite principle applied to team format:
/// Frank Dux did not choose who he fought alongside.
/// He played with whoever was in the bracket.
/// </summary>
public class TeamFormationService
{
    private readonly Random _rng = new();

    private static readonly string[] TeamNames =
    {
        "Iron Serpent", "Jade Dragon", "Ghost Blade", "Blood Lotus",
        "Shadow Crane", "Steel Phoenix", "Crimson Tiger", "Storm Wolf",
        "Silent Fang", "Burning Hawk", "White Viper", "Dark Mantis",
        "Rogue Koi", "Void Monk", "Thunder Ox", "Pale Sword"
    };

    // ═══════════════════════════════════════════════════════════════════════
    // MAIN ENTRY POINT
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Forms N/5 teams from the registered player pool.
    /// Player count must be a multiple of 5.
    ///
    /// Algorithm:
    ///   1. Assign each player their best available role (greedy role assignment)
    ///   2. Generate candidate team groupings (random but role-covered)
    ///   3. Score each grouping for chemistry and inter-team balance
    ///   4. Return the highest-scoring valid assignment
    ///
    /// Runs multiple iterations and picks the best result.
    /// Players see a random-feeling assignment. The data made it coherent.
    /// </summary>
    public List<FormedTeam> FormTeams(List<PlayerProfile> players, int iterations = 500)
    {
        if (players.Count % 5 != 0)
            throw new ArgumentException("Player count must be a multiple of 5.");

        int teamCount = players.Count / 5;
        List<FormedTeam>? bestAssignment = null;
        double bestScore = double.MinValue;

        for (int i = 0; i < iterations; i++)
        {
            var candidate = GenerateCandidateAssignment(players, teamCount);
            if (candidate == null) continue;

            double score = ScoreAssignment(candidate);
            if (score > bestScore)
            {
                bestScore = score;
                bestAssignment = candidate;
            }
        }

        if (bestAssignment == null)
            throw new InvalidOperationException("Could not form valid teams from player pool.");

        // Assign names
        var shuffledNames = TeamNames.OrderBy(_ => _rng.Next()).ToList();
        for (int i = 0; i < bestAssignment.Count; i++)
            bestAssignment[i].TeamName = shuffledNames[i % shuffledNames.Count];

        return bestAssignment;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // TEAM GENERATION
    // ═══════════════════════════════════════════════════════════════════════

    private List<FormedTeam>? GenerateCandidateAssignment(
        List<PlayerProfile> players, int teamCount)
    {
        // Shuffle players randomly — this is the "random" element
        var shuffled = players.OrderBy(_ => _rng.Next()).ToList();
        var teams = new List<FormedTeam>();

        for (int t = 0; t < teamCount; t++)
        {
            var teamPlayers = shuffled.Skip(t * 5).Take(5).ToList();
            var team = AssignRoles(teamPlayers);
            if (team == null) return null; // cannot cover all 5 roles
            teams.Add(team);
        }

        return teams;
    }

    /// <summary>
    /// Assigns the 5 roles to 5 players using a greedy best-fit approach.
    /// Tries all 120 possible role permutations and picks the highest total fit.
    /// </summary>
    private FormedTeam? AssignRoles(List<PlayerProfile> players)
    {
        if (players.Count != 5) return null;

        var roles = new[] { "TOP", "JUNGLE", "MID", "ADC", "SUPPORT" };
        var bestPermutation = GetPermutations(roles)
            .Select(perm => new
            {
                Perm = perm,
                Score = perm.Select((role, i) =>
                    players[i].RoleProficiency.TryGetValue(role, out var v) ? v : 0)
                    .Sum()
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestPermutation == null) return null;

        var team = new FormedTeam();
        for (int i = 0; i < 5; i++)
        {
            var role = bestPermutation.Perm[i];
            team.Slots.Add(new TeamSlot
            {
                Player = players[i],
                AssignedRole = role,
                RoleFitScore = players[i].RoleProficiency.TryGetValue(role, out var v) ? v : 0
            });
        }

        return team;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SCORING
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Scores a full team assignment on two dimensions:
    ///   1. Average team chemistry (internal cohesion)
    ///   2. Inter-team balance (competitive fairness between teams)
    ///
    /// A perfect score has every team being chemically coherent AND
    /// all teams being roughly equal in average BSR.
    /// </summary>
    private double ScoreAssignment(List<FormedTeam> teams)
    {
        foreach (var team in teams)
            ComputeTeamChemistry(team);

        double avgChemistry = teams.Average(t => t.TeamChemistryScore);

        // Penalize BSR variance between teams (we want balanced brackets)
        double avgBSR = teams.Average(t => t.AverageBSR);
        double bsrVariance = teams.Average(t => Math.Pow(t.AverageBSR - avgBSR, 2));
        double balancePenalty = Math.Sqrt(bsrVariance) * 0.3;

        return avgChemistry - balancePenalty;
    }

    /// <summary>
    /// Computes all chemistry sub-scores for a single team.
    /// </summary>
    private void ComputeTeamChemistry(FormedTeam team)
    {
        var players = team.Slots.Select(s => s.Player).ToList();

        // 1. Role coverage — how well do assigned roles match player proficiency?
        team.RoleCoverageScore = team.Slots.Average(s => s.RoleFitScore) * 100;

        // 2. Playstyle balance — is aggression balanced with utility?
        double avgAggression = players.Average(p => p.AggressionIndex);
        double avgUtility    = players.Average(p => p.UtilityIndex);
        double avgCarry      = players.Average(p => p.CarryCapacity);
        // Ideal: ~0.5 aggression, ~0.5 utility, ~0.5 carry — no single extreme
        team.PlaystyleBalanceScore = 100 - (
            Math.Abs(avgAggression - 0.5) * 50 +
            Math.Abs(avgUtility    - 0.5) * 30 +
            Math.Abs(avgCarry      - 0.5) * 20
        );
        team.PlaystyleBalanceScore = Math.Max(0, team.PlaystyleBalanceScore);

        // 3. Champion pool depth — how many win conditions can this team run?
        double engageDepth    = players.Max(p => p.EngagePotential);
        double peelDepth      = players.Max(p => p.PeelPotential);
        double pokeDepth      = players.Max(p => p.PokePotential);
        double splitDepth     = players.Max(p => p.SplitPushPotential);
        double teamfightDepth = players.Max(p => p.TeamfightPotential);
        // More viable strategies = higher score
        double strategies = new[] { engageDepth, peelDepth, pokeDepth, splitDepth, teamfightDepth }
            .Count(d => d > 0.4);
        team.ChampionPoolDepthScore = (strategies / 5.0) * 100;

        // 4. Win condition diversity — can they win multiple ways?
        // High if the team has at least one strong option in multiple categories
        team.WinConditionDiversityScore = new[] { engageDepth, peelDepth, pokeDepth, splitDepth, teamfightDepth }
            .Where(d => d > 0.5).Count() / 5.0 * 100;

        // 5. Skill variance — lower is better (more even teams play better together)
        double bsrMean     = players.Average(p => p.BSR);
        double bsrVariance = players.Average(p => Math.Pow(p.BSR - bsrMean, 2));
        double bsrStdDev   = Math.Sqrt(bsrVariance);
        team.SkillVarianceScore = Math.Max(0, 100 - (bsrStdDev * 5));

        // Final TCS — weighted composite
        team.TeamChemistryScore =
            (team.RoleCoverageScore        * 0.30) +
            (team.PlaystyleBalanceScore    * 0.25) +
            (team.ChampionPoolDepthScore   * 0.20) +
            (team.WinConditionDiversityScore * 0.15) +
            (team.SkillVarianceScore       * 0.10);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // MOCK PROFILES — for beta before op.gg / Riot API integration
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates a realistic mock PlayerProfile for testing.
    /// Ryan: replace with OpggDataService.FetchProfile() when ready.
    /// </summary>
    public static PlayerProfile GenerateMockProfile(Guid playerId, string summonerName)
    {
        var rng = new Random();
        var roles = new[] { "TOP", "JUNGLE", "MID", "ADC", "SUPPORT" };
        var primaryRole = roles[rng.Next(roles.Length)];

        var proficiency = new Dictionary<string, double>();
        foreach (var r in roles)
            proficiency[r] = r == primaryRole ? 0.6 + rng.NextDouble() * 0.4
                           : rng.NextDouble() * 0.4;

        return new PlayerProfile
        {
            PlayerId           = playerId,
            SummonerName       = summonerName,
            RoleProficiency    = proficiency,
            AggressionIndex    = rng.NextDouble(),
            UtilityIndex       = rng.NextDouble(),
            CarryCapacity      = rng.NextDouble(),
            ObjectiveFocus     = rng.NextDouble(),
            ConsistencyRating  = rng.NextDouble(),
            EngagePotential    = rng.NextDouble(),
            PeelPotential      = rng.NextDouble(),
            PokePotential      = rng.NextDouble(),
            SplitPushPotential = rng.NextDouble(),
            TeamfightPotential = rng.NextDouble(),
            AverageKDA         = 1.5 + rng.NextDouble() * 3.5,
            AverageCSPerMin    = 4.0 + rng.NextDouble() * 5.0,
            AverageVisionScore = 15 + rng.NextDouble() * 35,
            AverageDamageShare = 0.15 + rng.NextDouble() * 0.25,
            WinRateLast30      = 0.40 + rng.NextDouble() * 0.25,
            AverageGameImpact  = rng.NextDouble(),
            RankedGamesAnalyzed= 30 + rng.Next(70),
            BSR                = 5 + rng.NextDouble() * 35,
            TrueSkillMu        = 15 + rng.NextDouble() * 20,
            TrueSkillSigma     = 3 + rng.NextDouble() * 5,
            ProfileBuiltAt     = DateTime.UtcNow,
            DataSource         = "mock"
        };
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static IEnumerable<string[]> GetPermutations(string[] items)
    {
        if (items.Length <= 1) { yield return items; yield break; }
        foreach (var item in items)
        {
            var rest = items.Where(i => i != item).ToArray();
            foreach (var perm in GetPermutations(rest))
                yield return new[] { item }.Concat(perm).ToArray();
        }
    }
}
