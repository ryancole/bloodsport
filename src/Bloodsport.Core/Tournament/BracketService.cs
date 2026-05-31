using Bloodsport.Core.Models;
using Bloodsport.Core.Tdl;

namespace Bloodsport.Core.Tournament;

public class BracketService
{
    // Generates a single-elimination bracket from a seeded player list.
    // Seeds are ordered by TrueSkill display rating (highest = seed 1).
    // Bracket follows standard seeding: 1v8, 2v7, 3v6, 4v5 in an 8-player bracket.
    public List<Match> GenerateBracket(Models.Tournament tournament, List<Player> seededPlayers)
    {
        int count = seededPlayers.Count;
        if ((count & (count - 1)) != 0)
            throw new ArgumentException("Player count must be a power of 2.");

        var matches = new List<Match>();
        int totalRounds = (int)Math.Log2(count);

        // Build all match shells for all rounds
        for (int round = 1; round <= totalRounds; round++)
        {
            int matchesInRound = count / (int)Math.Pow(2, round);
            for (int i = 0; i < matchesInRound; i++)
            {
                matches.Add(new Match
                {
                    Id = Guid.NewGuid(),
                    TournamentId = tournament.Id,
                    Round = round,
                    MatchNumber = i + 1
                });
            }
        }

        // Seed round 1 using standard bracket seeding pattern
        var round1 = matches.Where(m => m.Round == 1).OrderBy(m => m.MatchNumber).ToList();
        var seeds = StandardSeedOrder(count);

        for (int i = 0; i < round1.Count; i++)
        {
            round1[i].Player1Id = seededPlayers[seeds[i * 2] - 1].Id;
            round1[i].Player2Id = seededPlayers[seeds[i * 2 + 1] - 1].Id;
            round1[i].Player1 = seededPlayers[seeds[i * 2] - 1];
            round1[i].Player2 = seededPlayers[seeds[i * 2 + 1] - 1];
        }

        // Wire advancement: each match's winner slot points to the next round match
        for (int round = 1; round < totalRounds; round++)
        {
            var currentRound = matches.Where(m => m.Round == round).OrderBy(m => m.MatchNumber).ToList();
            var nextRound = matches.Where(m => m.Round == round + 1).OrderBy(m => m.MatchNumber).ToList();

            for (int i = 0; i < currentRound.Count; i++)
            {
                currentRound[i].NextMatchId = nextRound[i / 2].Id;
                currentRound[i].NextMatchSlot = (i % 2 == 0) ? 1 : 2;
            }
        }

        return matches;
    }

    public void AdvanceBracket(List<Match> allMatches, Match completedMatch)
    {
        if (completedMatch.WinnerId == null || completedMatch.NextMatchId == null)
            return;

        var nextMatch = allMatches.First(m => m.Id == completedMatch.NextMatchId);
        if (completedMatch.NextMatchSlot == 1)
        {
            nextMatch.Player1Id = completedMatch.WinnerId;
            nextMatch.Player1 = completedMatch.Winner;
        }
        else
        {
            nextMatch.Player2Id = completedMatch.WinnerId;
            nextMatch.Player2 = completedMatch.Winner;
        }
    }

    // Returns 1-based seed positions for a bracket of given size.
    // Standard single-elim seeding: top seed always in top half, 1v(n), 2v(n-1), etc.
    private static List<int> StandardSeedOrder(int n)
    {
        var seeds = new List<int> { 1, 2 };
        while (seeds.Count < n)
        {
            var next = new List<int>();
            int total = seeds.Count * 2 + 1;
            foreach (var s in seeds)
            {
                next.Add(s);
                next.Add(total - s);
            }
            seeds = next;
        }
        return seeds;
    }
}
