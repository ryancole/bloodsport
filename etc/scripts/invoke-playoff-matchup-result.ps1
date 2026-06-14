param(
    [Parameter(Mandatory)]
    [long] $PlayoffId,

    [Parameter(Mandatory)]
    [string] $ServerInstance,

    [Parameter(Mandatory)]
    [string] $Database,

    [string] $FunctionUrl = "http://localhost:7071/api/HandlePlayoffMatchup",

    [string] $FunctionKey = "",

    [ValidateRange(0, 100)]
    [int] $SkipPercent = 20
)

$projectPath = "$PSScriptRoot\..\..\src\Tools\Bloodsport.Tools.SeedPlayoffMatchupResults\Bloodsport.Tools.SeedPlayoffMatchupResults.csproj"

$dotnetArgs = @(
    "run", "--project", $projectPath, "--",
    "--playoff",  $PlayoffId,
    "--server",   $ServerInstance,
    "--database", $Database,
    "--url",      $FunctionUrl,
    "--skip",     $SkipPercent
)

if ($FunctionKey) {
    $dotnetArgs += "--key", $FunctionKey
}

dotnet @dotnetArgs
