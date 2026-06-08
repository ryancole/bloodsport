param(
    [Parameter(Mandatory)]
    [long] $SeasonId,

    [Parameter(Mandatory)]
    [string] $ServerInstance,

    [Parameter(Mandatory)]
    [string] $Database,

    [string] $FunctionUrl = "http://localhost:7262/api/HandleMatchupResult",

    [string] $FunctionKey = ""
)

# ── 1. Query matchups without results for the season ─────────────────────────

$sqlParams = @{
    ServerInstance         = $ServerInstance
    Database               = $Database
    TrustServerCertificate = $true
}

$matchupRows = Invoke-Sqlcmd @sqlParams -Query "
    SELECT m.Id, m.TournamentCode, m.TeamOneId, m.TeamTwoId
    FROM SeasonWeekMatchups m
    INNER JOIN SeasonWeeks w ON w.Id = m.SeasonWeekId
    WHERE w.SeasonId = $SeasonId
      AND m.TournamentCode IS NOT NULL
      AND NOT EXISTS (
          SELECT 1 FROM SeasonWeekMatchupResults r WHERE r.SeasonWeekMatchupId = m.Id
      )
"

if (-not $matchupRows) {
    Write-Host "No pending matchups (without results) found for season $SeasonId."
    exit 0
}

Write-Host "Found $(@($matchupRows).Count) matchup(s) without results."

# ── 2. Query all team member PUUIDs for the season ────────────────────────────

$memberRows = Invoke-Sqlcmd @sqlParams -Query "
    SELECT tm.TeamId, ra.Puuid
    FROM TeamMemberships tm
    INNER JOIN RiotAccounts ra ON ra.Id = tm.RiotAccountId
    INNER JOIN SeasonRegistrations sr ON sr.TeamId = tm.TeamId
    WHERE sr.SeasonId = $SeasonId
"

$puuidsByTeam = @{}
foreach ($row in @($memberRows)) {
    $tid = $row.TeamId
    if (-not $puuidsByTeam.ContainsKey($tid)) { $puuidsByTeam[$tid] = @() }
    $puuidsByTeam[$tid] += $row.Puuid
}

# ── 3. POST a TournamentGamesV5 result for each pending matchup ───────────────

$headers = @{ "Content-Type" = "application/json" }
if ($FunctionKey) { $headers["x-functions-key"] = $FunctionKey }

foreach ($matchup in @($matchupRows)) {
    $matchupId      = $matchup.Id
    $tournamentCode = $matchup.TournamentCode
    $teamOneId      = $matchup.TeamOneId
    $teamTwoId      = $matchup.TeamTwoId

    $winnerId = @($teamOneId, $teamTwoId) | Get-Random
    $loserId  = if ($winnerId -eq $teamOneId) { $teamTwoId } else { $teamOneId }

    $winnerPuuids = @($puuidsByTeam[$winnerId] | ForEach-Object { @{ puuid = $_ } })
    $loserPuuids  = @($puuidsByTeam[$loserId]  | ForEach-Object { @{ puuid = $_ } })

    if ($winnerPuuids.Count -eq 0 -and $loserPuuids.Count -eq 0) {
        Write-Warning "Matchup $matchupId ($tournamentCode): no team members found for either team, skipping."
        continue
    }

    $payload = @{
        shortCode   = $tournamentCode
        metaData    = ""
        gameId      = [long](Get-Random -Minimum 1000000000 -Maximum 9999999999)
        gameName    = "seed-game-$matchupId"
        gameType    = "Practice"
        gameMap     = 11
        gameMode    = "CLASSIC"
        region      = "NA1"
        startTime   = [long]([DateTimeOffset]::UtcNow.ToUnixTimeMilliseconds())
        winningTeam = $winnerPuuids
        losingTeam  = $loserPuuids
    } | ConvertTo-Json -Depth 3

    Write-Host "Matchup $matchupId ($tournamentCode) — winner team $winnerId — posting..."

    try {
        $response = Invoke-WebRequest -Uri $FunctionUrl -Method POST -Headers $headers -Body $payload
        Write-Host "  -> $($response.StatusCode) $($response.StatusDescription)"
    }
    catch {
        Write-Warning "  -> Failed: $_"
    }
}
