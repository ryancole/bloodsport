$projectDir = Resolve-Path "$PSScriptRoot\..\..\src\Compute\BloodsportFunctions"

Start-Process pwsh -ArgumentList "-NoExit", "-WorkingDirectory", $projectDir, "-Command", "dotnet run"
