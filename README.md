# Bloodsport

A web application for organizing competitive gaming sessions — teams, players, and Riot account linking.

## Stack

- **ASP.NET Core / Blazor** (server + WebAssembly, .NET 10)
- **Entity Framework Core** with SQL Server
- **Microsoft Entra ID** (Azure AD) for authentication via MSAL / OpenID Connect
- **Riot Games account linking** for player identity

## Project layout

```
src/
  Libraries/
    Bloodsport.Entity/          # Domain models (User, Team, RiotAccount, TeamInvite, …)
    Bloodsport.Data.Sql/        # EF Core DbContext, entity configs, migrations
  Websites/
    BloodsportSite/
      BloodsportSite/           # ASP.NET Core host — API endpoints, Blazor SSR pages
      BloodsportSite.Client/    # Blazor WebAssembly client
```

## Getting started

### Prerequisites

- .NET 10 SDK
- SQL Server (local or remote)
- An Entra ID (Azure AD) app registration for auth
- A Riot Games API key for account linking

### Configuration

Create `appsettings.Development.json` (or use user secrets) with:

```json
{
  "ConnectionStrings": {
    "Bloodsport": "<your SQL Server connection string>"
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<tenant-id>",
    "ClientId": "<client-id>",
    "ClientSecret": "<client-secret>",
    "CallbackPath": "/signin-oidc"
  }
}
```

### Database

Apply migrations from the repo root:

```bash
dotnet ef database update --project src/Libraries/Bloodsport.Data.Sql --startup-project src/Websites/BloodsportSite/BloodsportSite
```

### Run

```bash
dotnet run --project src/Websites/BloodsportSite/BloodsportSite
```

## Admin roles

Admin access is controlled via an **Entra ID App Role** (`Bloodsport.Admin`) defined on the BloodsportSite app registration. The portal UI does not support assigning roles to users on external tenants — use the Azure CLI instead.

### Assigning the Bloodsport.Admin role to a user

```powershell
# Log into the external tenant first (no subscriptions is expected)
az login --tenant "<external-tenant-id>" --allow-no-subscriptions

# Assign the role
az rest --method POST `
  --uri "https://graph.microsoft.com/v1.0/servicePrincipals/<service-principal-object-id>/appRoleAssignedTo" `
  --body '{\"principalId\": \"<user-object-id>\", \"resourceId\": \"<service-principal-object-id>\", \"appRoleId\": \"<app-role-id>\"}'
```

| Value | Where to find it |
|---|---|
| `external-tenant-id` | Entra → Overview → Tenant ID |
| `service-principal-object-id` | Enterprise applications → BloodsportSite → Overview → Object ID |
| `user-object-id` | Entra → Users → click user → Object ID |
| `app-role-id` | App registrations → BloodsportSite → App roles → Bloodsport Admin → ID |

After assigning, the user must sign out and back in for the `roles` claim to appear in their token.

## Azure SQL managed identity setup

Each Azure service that connects to SQL uses its **system-assigned managed identity** rather than a SQL login. After enabling the managed identity on a service (App Service or Function App), connect to the SQL database as an Entra admin and run:

```sql
-- Blazor App Service
CREATE USER [bloodsport-app] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [bloodsport-app];
ALTER ROLE db_datawriter ADD MEMBER [bloodsport-app];

-- Function App (if it also needs SQL access)
CREATE USER [bloodsport-functions] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [bloodsport-functions];
ALTER ROLE db_datawriter ADD MEMBER [bloodsport-functions];
```

```sql
-- GitHub Actions deploy identity (needs DDL access to run migrations)
CREATE USER [bloodsport-github-deploy] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [bloodsport-github-deploy];
ALTER ROLE db_datawriter ADD MEMBER [bloodsport-github-deploy];
ALTER ROLE db_ddladmin ADD MEMBER [bloodsport-github-deploy];
```

Replace the bracketed names with the exact names of your App Service, Function App, and Entra app registration resources. These commands must be run against the target **database** (not `master`) while connected as an Entra admin.

## Deployment

Two manual GitHub Actions workflows handle deployment:

- `.github/workflows/deploy-functions.yml` — Azure Functions app
- `.github/workflows/deploy-site.yml` — Blazor site (App Service)

Both use OIDC (federated identity) to authenticate with Azure — no publish profiles required.

### GitHub secrets

Add these to the repo under **Settings → Secrets and variables → Actions**:

| Secret | Where to find it |
|---|---|
| `AZURE_CLIENT_ID` | App Registration → Overview → Application (client) ID |
| `AZURE_TENANT_ID` | App Registration → Overview → Directory (tenant) ID |
| `AZURE_SUBSCRIPTION_ID` | Azure Portal → Subscriptions |
| `AZURE_FUNCTION_APP_NAME` | Name of the Azure Function App resource |
| `AZURE_SITE_APP_NAME` | Name of the Azure App Service resource |

### Azure App Registration setup

1. Create (or reuse) an **App Registration** in Entra ID.
2. Add a **Federated credential** for each branch you'll deploy from:
   - Go to the App Registration → **Certificates & secrets → Federated credentials → Add credential**
   - Choose **GitHub Actions**
   - Set **Organization** to `ryancole`, **Repository** to `bloodsport`, **Entity type** to `Branch`, and **Branch** to `master`
3. Grant the App Registration **Website Contributor** role on both the Function App and the App Service:
   - Go to each resource → **Access control (IAM) → Add role assignment**
   - Role: `Website Contributor`, assign to the App Registration (search by name)
