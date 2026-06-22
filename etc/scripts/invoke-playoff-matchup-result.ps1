param(
    [Parameter(Mandatory)]
    [long] $PlayoffId,

    [string] $FunctionUrl = "http://localhost:7071/api/HandlePlayoffMatchup",

    [string] $FunctionKey = "",

    [ValidateRange(0, 100)]
    [int] $SkipPercent = 20
)

$projectPath = "$PSScriptRoot\..\..\src\Tools\Bloodsport.Tools.SeedPlayoffMatchupResults\Bloodsport.Tools.SeedPlayoffMatchupResults.csproj"

$dotnetArgs = @(
    "run", "--project", $projectPath, "--",
    "--playoff", $PlayoffId,
    "--url",     $FunctionUrl,
    "--skip",    $SkipPercent
)

if ($FunctionKey) {
    $dotnetArgs += "--key", $FunctionKey
}

dotnet @dotnetArgs
