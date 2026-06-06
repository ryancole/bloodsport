$project = Resolve-Path "$PSScriptRoot\..\..\src\Websites\BloodsportSite\BloodsportSite\BloodsportSite.csproj"

Start-Process pwsh -ArgumentList "-NoExit", "-Command", "dotnet run --project `"$project`""
