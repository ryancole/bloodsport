param(
    [ValidateRange(1, 100)]
    [int] $Count = 5
)

$projectPath = "$PSScriptRoot\..\..\src\Tools\Bloodsport.Tools.SeedRecruitment\Bloodsport.Tools.SeedRecruitment.csproj"

$dotnetArgs = @(
    "run", "--project", $projectPath, "--",
    "--count", $Count
)

dotnet @dotnetArgs
