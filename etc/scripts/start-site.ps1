$project = Resolve-Path "$PSScriptRoot\..\..\src\Websites\BloodsportSite\BloodsportSite\BloodsportSite.csproj"

Start-Process pwsh -ArgumentList "-NoExit", "-Command", "dotnet watch --project `"$project`""
