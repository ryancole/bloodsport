$project = Resolve-Path "$PSScriptRoot\..\..\src\Compute\BloodsportFunctions\BloodsportFunctions.csproj"

Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project `"$project`""
