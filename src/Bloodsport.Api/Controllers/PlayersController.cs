using Bloodsport.Core.Models;
using Bloodsport.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloodsport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly BloodsportDbContext _db;

    public PlayersController(BloodsportDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Leaderboard() =>
        Ok(await _db.Players
            .Where(p => p.IsRanked)
            .OrderByDescending(p => p.TrueSkillMu - 3 * p.TrueSkillSigma)
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var player = await _db.Players.FindAsync(id);
        return player == null ? NotFound() : Ok(player);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request)
    {
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            SummonerName = request.SummonerName,
            RiotPuuid = request.RiotPuuid
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        return Ok(player);
    }
}

public record CreatePlayerRequest(string Username, string SummonerName, string RiotPuuid);
