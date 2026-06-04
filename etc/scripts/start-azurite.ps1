# C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe

$azurite = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe"

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
$dataFolder = Join-Path $repoRoot ".azurite"

if (-not (Test-Path $dataFolder)) {
    New-Item -ItemType Directory -Path $dataFolder | Out-Null
}

Start-Process -FilePath $azurite -ArgumentList "--location `"$dataFolder`""
