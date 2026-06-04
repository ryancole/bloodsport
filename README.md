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
