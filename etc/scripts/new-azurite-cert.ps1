# Creates a trusted self-signed TLS certificate so Azurite can serve HTTPS.
#
# Azurite listens over plain HTTP by default, so the blob SAS URLs it generates
# come out as http:// and get blocked as mixed content on the HTTPS dev site.
# This produces a cert valid for localhost + 127.0.0.1 (1 year), exports it as a
# PFX for Azurite, and installs it into the Trusted Root store so .NET and the
# browser trust the emulator's HTTPS endpoint.
#
# Run once (re-run after a year, or whenever the cert expires). Trusting the
# cert into the machine root store requires an elevated shell; otherwise it is
# trusted for the current user and Windows shows a one-time confirmation prompt.

param(
    [string] $OutputDirectory,
    [string] $PfxPassword = "azurite"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."

if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $repoRoot ".certs"
}

if (-not (Test-Path $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory | Out-Null
}

$pfxPath = Join-Path $OutputDirectory "azurite.pfx"
$cerPath = Join-Path $OutputDirectory "azurite.cer"
$subject = "CN=Azurite Emulator"

# Remove any cert this script created previously so re-runs stay clean.
Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq $subject } |
    ForEach-Object {
        Write-Host "Removing existing certificate $($_.Thumbprint)"
        Remove-Item $_.PSPath -Force
    }

# Create the cert: server-auth EKU, SAN covering localhost and the loopback IP,
# valid for one year from now. IPAddress in the SAN is what lets browsers accept
# the https://127.0.0.1 host (a DNS-only SAN is rejected for IP literals).
$cert = New-SelfSignedCertificate `
    -Subject $subject `
    -FriendlyName "Azurite Local HTTPS" `
    -Type SSLServerAuthentication `
    -TextExtension @("2.5.29.17={text}DNS=localhost&IPAddress=127.0.0.1") `
    -KeyAlgorithm RSA -KeyLength 2048 `
    -KeyExportPolicy Exportable `
    -NotBefore (Get-Date) `
    -NotAfter  (Get-Date).AddYears(1) `
    -CertStoreLocation "Cert:\CurrentUser\My"

Write-Host "Created certificate $($cert.Thumbprint), expires $($cert.NotAfter.ToString('yyyy-MM-dd'))"

# Export the PFX Azurite consumes, plus the public cert used to trust it.
$securePwd = ConvertTo-SecureString -String $PfxPassword -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $securePwd | Out-Null
Export-Certificate   -Cert $cert -FilePath $cerPath | Out-Null
Write-Host "Wrote PFX:         $pfxPath"
Write-Host "Wrote public cert: $cerPath"

# Trust the cert. Prefer the machine root store when elevated (no prompt, trusted
# for every user); otherwise fall back to the current-user root store.
$principal = New-Object Security.Principal.WindowsPrincipal(
    [Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
$rootStore = if ($isAdmin) { "Cert:\LocalMachine\Root" } else { "Cert:\CurrentUser\Root" }

Write-Host "Installing into $rootStore (accept the prompt if one appears)..."
Import-Certificate -FilePath $cerPath -CertStoreLocation $rootStore | Out-Null
Write-Host "Certificate trusted."

Write-Host ""
Write-Host "Done. Start Azurite over HTTPS with:"
Write-Host "  azurite --cert `"$pfxPath`" --pwd `"$PfxPassword`" --location `"$(Join-Path $repoRoot '.azurite')`""
