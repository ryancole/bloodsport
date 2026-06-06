param(
    [Parameter(Mandatory)]
    [long] $MatchupId,

    [Parameter(Mandatory)]
    [long] $WinnerTeamId,

    [Parameter(Mandatory)]
    [string] $SqlConnectionString,

    [string] $FunctionUrl = "http://localhost:7071/api/HandleMatchupResult",

    [string] $FunctionKey = ""
)

# ── 1. Query the matchup and winning team memberships ──────────────────────────

$connection = New-Object System.Data.SqlClient.SqlConnection $SqlConnectionString
$connection.Open()

$matchupCmd = $connection.CreateCommand()
$matchupCmd.CommandText = "SELECT TournamentCode FROM SeasonWeekMatchups WHERE Id = @Id"
$matchupCmd.Parameters.AddWithValue("@Id", $MatchupId) | Out-Null
$tournamentCode = $matchupCmd.ExecuteScalar()

if (-not $tournamentCode) {
    Write-Error "No matchup found with Id $MatchupId, or its TournamentCode is null."
    $connection.Close()
    exit 1
}

$membersCmd = $connection.CreateCommand()
$membersCmd.CommandText = @"
SELECT ra.Puuid, tm.TeamId
FROM TeamMemberships tm
INNER JOIN RiotAccounts ra ON ra.Id = tm.RiotAccountId
WHERE tm.TeamId IN (
    SELECT TeamOneId FROM SeasonWeekMatchups WHERE Id = @Id
    UNION
    SELECT TeamTwoId FROM SeasonWeekMatchups WHERE Id = @Id
)
"@
$membersCmd.Parameters.AddWithValue("@Id", $MatchupId) | Out-Null

$reader = $membersCmd.ExecuteReader()
$participants = @()

while ($reader.Read()) {
    $puuid  = $reader["Puuid"]
    $teamId = $reader["TeamId"]
    $participants += @{
        puuid = $puuid
        team  = if ($teamId -eq $WinnerTeamId) { 100 } else { 200 }
        win   = ($teamId -eq $WinnerTeamId)
    }
}

$reader.Close()
$connection.Close()

if ($participants.Count -eq 0) {
    Write-Error "No team memberships found for matchup $MatchupId."
    exit 1
}

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
