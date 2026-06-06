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

# ── 1. Query all matchups for the season ──────────────────────────────────────

$sqlParams = @{
    ServerInstance        = $ServerInstance
    Database              = $Database
    TrustServerCertificate = $true
}

$matchupRows = Invoke-Sqlcmd @sqlParams -Query "
    SELECT m.Id, m.TournamentCode, m.TeamOneId, m.TeamTwoId
    FROM SeasonWeekMatchups m
    INNER JOIN SeasonWeeks w ON w.Id = m.SeasonWeekId
    WHERE w.SeasonId = $SeasonId
      AND m.TournamentCode IS NOT NULL
"

if (-not $matchupRows) {
    Write-Error "No matchups with a TournamentCode found for season $SeasonId."
    exit 1
}

Write-Host "Found $(@($matchupRows).Count) matchup(s) for season $SeasonId."

# ── 2. Query all team member PUUIDs for the season ────────────────────────────

$memberRows = Invoke-Sqlcmd @sqlParams -Query "
    SELECT tm.TeamId, ra.Puuid
    FROM TeamMemberships tm
    INNER JOIN RiotAccounts ra ON ra.Id = tm.RiotAccountId
    INNER JOIN SeasonRegistrations sr ON sr.TeamId = tm.TeamId
    WHERE sr.SeasonId = $SeasonId
"

# Group PUUIDs by TeamId for fast lookup
$puuidsByTeam = @{}
foreach ($row in $memberRows) {
    $tid = $row.TeamId
    if (-not $puuidsByTeam.ContainsKey($tid)) { $puuidsByTeam[$tid] = @() }
    $puuidsByTeam[$tid] += $row.Puuid
}

# ── 3. POST a result for each matchup ─────────────────────────────────────────

$headers = @{ "Content-Type" = "application/json" }
if ($FunctionKey) { $headers["x-functions-key"] = $FunctionKey }

foreach ($matchup in @($matchupRows)) {
    $matchupId      = $matchup.Id
    $tournamentCode = $matchup.TournamentCode
    $teamOneId      = $matchup.TeamOneId
    $teamTwoId      = $matchup.TeamTwoId

    # Randomly pick a winner
    $winnerId = @($teamOneId, $teamTwoId) | Get-Random
    $loserId  = if ($winnerId -eq $teamOneId) { $teamTwoId } else { $teamOneId }

    $participants = @()

    foreach ($puuid in $puuidsByTeam[$winnerId]) {
        $participants += @{ puuid = $puuid; team = 100; win = $true }
    }
    foreach ($puuid in $puuidsByTeam[$loserId]) {
        $participants += @{ puuid = $puuid; team = 200; win = $false }
    }

    if ($participants.Count -eq 0) {
        Write-Warning "Matchup $matchupId ($tournamentCode): no participants found, skipping."
        continue
    }

    $payload = @{
        shortCode    = $tournamentCode
        participants = $participants
    } | ConvertTo-Json -Depth 3

    Write-Host "Matchup $matchupId — winner team $winnerId — posting..."

    try {
        $response = Invoke-WebRequest -Uri $FunctionUrl -Method POST -Headers $headers -Body $payload
        Write-Host "  Response: $($response.StatusCode) $($response.StatusDescription)"
    }
    catch {
        Write-Warning "  Failed: $_"
    }
}
