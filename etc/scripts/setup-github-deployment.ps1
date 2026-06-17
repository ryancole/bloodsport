param(
    [Parameter(Mandatory)][string] $SubscriptionId,
    [Parameter(Mandatory)][string] $FunctionAppName,
    [Parameter(Mandatory)][string] $SiteAppName,
    [Parameter(Mandatory)][string] $ResourceGroupName
)

$AppRegistrationName = "bloodsport-github-deploy"

$GitHubOrg  = "ryancole"
$GitHubRepo = "bloodsport"
$GitHubBranch = "master"

Write-Host "Logging in to Azure..."
az login --output none
az account set --subscription $SubscriptionId

# Create or retrieve the app registration
Write-Host "Resolving app registration '$AppRegistrationName'..."
$appId = az ad app list --display-name $AppRegistrationName --query "[0].appId" --output tsv

if (-not $appId) {
    Write-Host "Creating app registration..."
    $appId = az ad app create --display-name $AppRegistrationName --query "appId" --output tsv
}

Write-Host "App ID: $appId"

# Ensure a service principal exists for the app registration
$spId = az ad sp list --filter "appId eq '$appId'" --query "[0].id" --output tsv

if (-not $spId) {
    Write-Host "Creating service principal..."
    $spId = az ad sp create --id $appId --query "id" --output tsv
}

Write-Host "Service Principal ID: $spId"

# Add federated credential for the GitHub branch
Write-Host "Adding federated credential for branch '$GitHubBranch'..."
$credentialName = "github-$GitHubRepo-$GitHubBranch"
$existingCred = az ad app federated-credential list --id $appId --query "[?name=='$credentialName'].id" --output tsv

if (-not $existingCred) {
    $tempFile = [System.IO.Path]::GetTempFileName()
    @{
        name        = $credentialName
        issuer      = "https://token.actions.githubusercontent.com"
        subject     = "repo:${GitHubOrg}/${GitHubRepo}:ref:refs/heads/${GitHubBranch}"
        audiences   = @("api://AzureADTokenExchange")
        description = "GitHub Actions deployment from $GitHubOrg/$GitHubRepo branch $GitHubBranch"
    } | ConvertTo-Json | Set-Content -Path $tempFile -Encoding UTF8

    az ad app federated-credential create --id $appId --parameters "@$tempFile" --output none
    Remove-Item $tempFile
    Write-Host "Federated credential created."
} else {
    Write-Host "Federated credential already exists, skipping."
}

# Grant Contributor on the Function App
Write-Host "Assigning Contributor role on Function App '$FunctionAppName'..."
$functionAppId = az functionapp show --name $FunctionAppName --resource-group $ResourceGroupName --query "id" --output tsv
az role assignment create --assignee $spId --role "Website Contributor" --scope $functionAppId --output none

# Grant Website Contributor on the App Service
Write-Host "Assigning Website Contributor role on App Service '$SiteAppName'..."
$siteAppId = az webapp show --name $SiteAppName --resource-group $ResourceGroupName --query "id" --output tsv
az role assignment create --assignee $spId --role "Website Contributor" --scope $siteAppId --output none

# Print the values needed for GitHub secrets
$tenantId = az account show --query "tenantId" --output tsv

Write-Host ""
Write-Host "Setup complete. Add the following secrets to your GitHub repository:"
Write-Host "  AZURE_CLIENT_ID       = $appId"
Write-Host "  AZURE_TENANT_ID       = $tenantId"
Write-Host "  AZURE_SUBSCRIPTION_ID = $SubscriptionId"
Write-Host "  AZURE_FUNCTION_APP_NAME = $FunctionAppName"
Write-Host "  AZURE_SITE_APP_NAME     = $SiteAppName"
