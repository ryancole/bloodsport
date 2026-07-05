# C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe

$azurite = "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe"

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
$dataFolder = Join-Path $repoRoot ".azurite"

# HTTPS cert produced by new-azurite-cert.ps1. Required so the emulator serves
# HTTPS and its blob SAS URLs aren't blocked as mixed content on the dev site.
$certPath = Join-Path $repoRoot ".certs\azurite.pfx"
$certPassword = "azurite"

if (-not (Test-Path $certPath)) {
    throw "Azurite HTTPS cert not found at '$certPath'. Run etc\scripts\new-azurite-cert.ps1 first."
}

if (-not (Test-Path $dataFolder)) {
    New-Item -ItemType Directory -Path $dataFolder | Out-Null
}

Start-Process -FilePath $azurite -ArgumentList "--skipApiVersionCheck --cert `"$certPath`" --pwd `"$certPassword`" --location `"$dataFolder`""
