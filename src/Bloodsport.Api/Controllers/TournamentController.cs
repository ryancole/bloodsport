using Bloodsport.Api.Hubs;
using Bloodsport.Api.Services;
using Bloodsport.Core.Models;
using Bloodsport.Core.Rating;
using Bloodsport.Core.Tdl;
using Bloodsport.Core.Tournament;
using Bloodsport.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Bloodsport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentController : ControllerBase
{
    private readonly BloodsportDbContext _db;
    private readonly BracketService _bracket;
    private readonly TrueSkillService _trueSkill;
    private readonly RiotTournamentService _riot;
    private readonly IHubContext<BracketHub> _hub;

    public TournamentController(BloodsportDbContext db, BracketService bracket,
        TrueSkillService trueSkill, RiotTournamentService riot, IHubContext<BracketHub> hub)
    {
        _db = db;
        _bracket = bracket;
        _trueSkill = trueSkill;
        _riot = riot;
        _hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> List() =>
        Ok(await _db.Tournaments.OrderByDescending(t => t.CreatedAt).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Players).ThenInclude(tp => tp.Player)
            .Include(t => t.Matches).ThenInclude(m => m.Player1)
            .Include(t => t.Matches).ThenInclude(m => m.Player2)
            .Include(t => t.Matches).ThenInclude(m => m.Winner)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament == null) return NotFound();
        return Ok(tournament);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateTournamentRequest request)
    {
        var definition = TournamentDefinitionParser.Parse(request.DefinitionYaml);

        var tournament = new Core.Models.Tournament
        {
            Id = Guid.NewGuid(),
            Name = definition.Name,
            DefinitionYaml = request.DefinitionYaml
        };

        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();
        return Ok(tournament);
    }

    [HttpPost("{id}/start")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Start(Guid id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Players).ThenInclude(tp => tp.Player)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tournament == null) return NotFound();
        if (tournament.Status != TournamentStatus.Draft)
            return BadRequest("Tournament already started.");

        var definition = TournamentDefinitionParser.Parse(tournament.DefinitionYaml);

        // Seed players by TrueSkill display rating
        var seeded = tournament.Players
            .Select(tp => tp.Player)
            .OrderByDescending(p => p.DisplayRating)
            .ToList();

        var matches = _bracket.GenerateBracket(tournament, seeded);

        // Get a Riot tournament ID and generate codes for round 1 matches
        var riotTournamentId = await _riot.CreateTournamentAsync(
            int.Parse(tournament.Id.ToString("N")[..8], System.Globalization.NumberStyles.HexNumber),
            tournament.Name);

        tournament.RiotTournamentId = riotTournamentId;
        tournament.Status = TournamentStatus.Active;
        tournament.StartedAt = DateTime.UtcNow;

        var round1Matches = matches.Where(m => m.Round == 1).ToList();
        var codes = await _riot.GenerateTournamentCodesAsync(riotTournamentId, round1Matches.Count);

        for (int i = 0; i < round1Matches.Count; i++)
            round1Matches[i].RiotTournamentCode = codes[i];

        _db.Matches.AddRange(matches);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group($"tournament-{id}").SendAsync("BracketUpdated", tournament);
        return Ok(tournament);
    }

    [HttpPost("{id}/matches/{matchId}/result")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<IActionResult> RecordResult(Guid id, Guid matchId, [FromBody] MatchResultRequest request)
    {
        var match = await _db.Matches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Tournament)
            .FirstOrDefaultAsync(m => m.Id == matchId && m.TournamentId == id);

        if (match == null) return NotFound();
        if (match.Status == MatchStatus.Completed) return BadRequest("Match already completed.");

        var winner = request.WinnerId == match.Player1Id ? match.Player1 : match.Player2;
        var loser = request.WinnerId == match.Player1Id ? match.Player2 : match.Player1;

        if (winner == null || loser == null) return BadRequest("Invalid winner.");

        // Update TrueSkill ratings
        var (winnerUpdate, loserUpdate) = _trueSkill.CalculateMatchOutcome(winner, loser);
        winner.TrueSkillMu = winnerUpdate.NewMu;
        winner.TrueSkillSigma = winnerUpdate.NewSigma;
        winner.GamesPlayed++;
        loser.TrueSkillMu = loserUpdate.NewMu;
        loser.TrueSkillSigma = loserUpdate.NewSigma;
        loser.GamesPlayed++;

        match.WinnerId = winner.Id;
        match.Winner = winner;
        match.Status = MatchStatus.Completed;
        match.RiotMatchId = request.RiotMatchId;

        // Advance bracket
        var allMatches = await _db.Matches.Where(m => m.TournamentId == id).ToListAsync();
        _bracket.AdvanceBracket(allMatches, match);

        // Generate Riot code for the next match if one exists
        if (match.NextMatchId.HasValue)
        {
            var nextMatch = allMatches.First(m => m.Id == match.NextMatchId);
            if (nextMatch.Player1Id.HasValue && nextMatch.Player2Id.HasValue)
            {
                var codes = await _riot.GenerateTournamentCodesAsync(match.Tournament.RiotTournamentId, 1);
                nextMatch.RiotTournamentCode = codes.FirstOrDefault();
            }
        }
        else
        {
            // Final match — tournament complete
            match.Tournament.Status = TournamentStatus.Completed;
            match.Tournament.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        var updated = await _db.Tournaments
            .Include(t => t.Matches).ThenInclude(m => m.Player1)
            .Include(t => t.Matches).ThenInclude(m => m.Player2)
            .Include(t => t.Matches).ThenInclude(m => m.Winner)
            .FirstAsync(t => t.Id == id);

        await _hub.Clients.Group($"tournament-{id}").SendAsync("BracketUpdated", updated);
        return Ok(updated);
    }

    // Riot calls this endpoint after every match via webhook
    [HttpPost("riot/callback")]
    public async Task<IActionResult> RiotCallback([FromBody] RiotMatchResult result)
    {
        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.RiotTournamentCode == result.TournamentCode);

        if (match == null) return Ok(); // Not our match, ignore

        match.RiotMatchId = result.GameId;
        await _db.SaveChangesAsync();
        return Ok();
    }
}

public record CreateTournamentRequest(string DefinitionYaml);
public record MatchResultRequest(Guid WinnerId, string? RiotMatchId);
