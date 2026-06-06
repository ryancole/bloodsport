param(
    [Parameter(Mandatory)]
    [long] $MatchupId,

    [Parameter(Mandatory)]
    [long] $WinnerTeamId,

    [Parameter(Mandatory)]
    [string] $ServerInstance,

    [Parameter(Mandatory)]
    [string] $Database,

    [string] $FunctionUrl = "http://localhost:7262/api/HandleMatchupResult",

    [string] $FunctionKey = ""
)

# ── 1. Query the matchup and winning team memberships ──────────────────────────

$sqlParams = @{
    ServerInstance = $ServerInstance
    Database       = $Database
    TrustServerCertificate = $true
}

$matchupRow = Invoke-Sqlcmd @sqlParams -Query "
    SELECT TournamentCode FROM SeasonWeekMatchups WHERE Id = $MatchupId
"

if (-not $matchupRow -or [string]::IsNullOrEmpty($matchupRow.TournamentCode)) {
    Write-Error "No matchup found with Id $MatchupId, or its TournamentCode is null."
    exit 1
}

$tournamentCode = $matchupRow.TournamentCode

$memberRows = Invoke-Sqlcmd @sqlParams -Query "
    SELECT ra.Puuid, tm.TeamId
    FROM TeamMemberships tm
    INNER JOIN RiotAccounts ra ON ra.Id = tm.RiotAccountId
    WHERE tm.TeamId IN (
        SELECT TeamOneId FROM SeasonWeekMatchups WHERE Id = $MatchupId
        UNION
        SELECT TeamTwoId FROM SeasonWeekMatchups WHERE Id = $MatchupId
    )
"

if (-not $memberRows) {
    Write-Error "No team memberships found for matchup $MatchupId."
    exit 1
}

$participants = @($memberRows | ForEach-Object {
    @{
        puuid = $_.Puuid
        team  = if ($_.TeamId -eq $WinnerTeamId) { 100 } else { 200 }
        win   = ($_.TeamId -eq $WinnerTeamId)
    }
})

# ── 2. Build the callback payload ──────────────────────────────────────────────

$payload = @{
    shortCode    = $tournamentCode
    participants = $participants
} | ConvertTo-Json -Depth 3

Write-Host "Posting to $FunctionUrl"
Write-Host "Payload:`n$payload"

# ── 3. POST to the function ────────────────────────────────────────────────────

$headers = @{ "Content-Type" = "application/json" }

if ($FunctionKey) {
    $headers["x-functions-key"] = $FunctionKey
}

$response = Invoke-WebRequest -Uri $FunctionUrl -Method POST -Headers $headers -Body $payload
Write-Host "Response: $($response.StatusCode) $($response.StatusDescription)"
