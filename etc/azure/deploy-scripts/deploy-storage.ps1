param(
    [Parameter(Mandatory)]
    [string]$ResourceGroup,

    [Parameter(Mandatory)]
    [string]$StorageAccountName,

    [string]$Location = 'eastus'
)

$ErrorActionPreference = 'Stop'

# Ensure logged in
$account = az account show 2>$null | ConvertFrom-Json
if (-not $account) {
    Write-Host "Not logged in. Running az login..."
    az login
}

Write-Host "Subscription: $($account.name) ($($account.id))"

# Ensure resource group exists
$rg = az group show --name $ResourceGroup 2>$null | ConvertFrom-Json
if (-not $rg) {
    Write-Host "Creating resource group '$ResourceGroup' in '$Location'..."
    az group create --name $ResourceGroup --location $Location | Out-Null
}

$bicepFile = Join-Path $PSScriptRoot "..\bicep\storage-account.bicep"

$deployArgs = @(
    'deployment', 'group', 'create'
    '--resource-group', $ResourceGroup
    '--template-file', $bicepFile
    '--parameters', "storageAccountName=$StorageAccountName"
    '--output', 'json'
)

Write-Host "Deploying '$bicepFile' to resource group '$ResourceGroup'..."

$result = az @deployArgs | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment failed."
    exit 1
}

Write-Host "Deployment succeeded: $($result.properties.provisioningState)"

if ($result.properties.outputs) {
    Write-Host "`nOutputs:"
    $result.properties.outputs.PSObject.Properties | ForEach-Object {
        Write-Host "  $($_.Name) = $($_.Value.value)"
    }
}
