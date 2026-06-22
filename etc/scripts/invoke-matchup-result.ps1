param(
    [Parameter(Mandatory)]
    [long] $SeasonId,

    [string] $FunctionUrl = "http://localhost:7071/api/HandleSeasonWeekMatchup",

    [string] $FunctionKey = "",

    [ValidateRange(0, 100)]
    [int] $SkipPercent = 20
)

$projectPath = "$PSScriptRoot\..\..\src\Tools\Bloodsport.Tools.SeedMatchupResults\Bloodsport.Tools.SeedMatchupResults.csproj"

$dotnetArgs = @(
    "run", "--project", $projectPath, "--",
    "--season", $SeasonId,
    "--url",    $FunctionUrl,
    "--skip",   $SkipPercent
)

if ($FunctionKey) {
    $dotnetArgs += "--key", $FunctionKey
}

dotnet @dotnetArgs
