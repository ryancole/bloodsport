# Azure export — `Champions` resource group

Snapshot of the production Azure environment taken **2026-08-26**, immediately before
shutting the resources down. Subscription: `Bloodsport` (`ea3dd5ca-0133-4319-8bf7-eed71ca621d8`),
region: `southcentralus`.

## Files

| File | What it is |
|---|---|
| `champions-export.bicep` | Full infrastructure as Bicep (decompiled from the ARM export, hand-fixed — see below). This is the file to deploy from. |
| `champions-export.json` | Raw, unmodified `az group export` ARM template, kept for reference. Contains decompile-breaking self-references and ~770 auto-generated resources; don't deploy it directly. |
| `caleague.net.zonefile.txt` | BIND-format zone file for the `caleague.net` public DNS zone (records are also in the Bicep). |
| `role-assignments.json` | RBAC role assignments scoped to the resource group (was empty at export time). |
| `champions-app.appsettings.json` | **gitignored, contains secrets** — app settings for the web app. |
| `champions-func.appsettings.json` | **gitignored, contains secrets** — app settings for the function app. |
| `champions-app.connstrings.json` | **gitignored** — connection-string section (was empty; conn strings live in app settings). |

## How the Bicep was produced

```powershell
az group export -g Champions --include-parameter-default-value > champions-export.json
az bicep decompile --file champions-export.json
```

Then fixed by hand:

1. Removed self-referencing read-only `id:` properties the decompiler emits for
   subnet delegations, VNet inline subnets, and private-endpoint
   `privateLinkServiceConnections` (BCP079/BCP080 errors).
2. Deleted ~770 auto-generated child resources that pushed the template over the
   800-resource ARM limit and would be recreated automatically anyway:
   Log Analytics default tables (684) and `LogManagement(...)` saved searches (39),
   App Service deployment-history entries (20), Application Insights default
   `ProactiveDetectionConfigs` (13), and function entries (11) that come from code
   deployment, not infrastructure.

Verified with `az bicep build` — compiles with only lint warnings.

## What the Bicep captures

App Service plan + web app (`champions-app`) and function app (`champions-func`) with
site config, hostname bindings, and VNet integration; SQL server + `champions-sqldb`
(GP_S_Gen5_1 serverless) with auditing/advisor/security child resources; storage
account `caleague` with 4 blob containers; Key Vault `champions-kv` (RBAC-enabled,
plus 2 legacy access policies); Service Bus namespace with 6 queues; ACS +
email service + `caleague.net` sender domain; VNet with 5 subnets; 3 private
endpoints (KV, SQL, blob) with private DNS zones and VNet links; public DNS zone
`caleague.net` with A/CNAME/TXT records; Log Analytics + App Insights; the CIAM
(Entra External ID) directory resource shell.

## NOT captured — back these up before deleting

1. **Key Vault secrets** — secret *values* are never exported. The CLI identity had
   no RBAC data-plane role, so not even names could be listed. Grant yourself
   *Key Vault Secrets Officer* on `champions-kv` and save every secret before
   deletion. Note the vault is soft-deleted on removal; purge it (or wait out
   retention) before the name `champions-kv` can be reused.
2. **SQL data** — export a bacpac of `champions-sqldb` (Portal → database →
   Export, or `az sql db export`). The server has a private endpoint, so a
   portal export to the storage account is the easiest path.
3. **Blob data** — `bs-team-logo` and `bs-user-logo` hold user-uploaded content
   (plus `azure-webjobs-*` runtime containers you can skip). Download with
   `azcopy` or the portal before deletion.
4. **⚠️ CIAM / Entra External ID tenant** (`caleague.onmicrosoft.com`, tenant
   `f3b5affe-8fa4-4d75-bc2a-265603749644`) — this resource-group item is a *link*
   to a real directory holding all league user accounts and the app registration
   (`AzureAd:ClientId f992ba5b-021c-4ddd-bf7d-e2c1e4dcc3e7`). Deleting it deletes
   the tenant, its users, and the app registration. If you want sign-ins to
   survive a future resurrection, delete every *other* resource and keep this one
   (it's free at the Base/A0 tier), or accept re-onboarding users later.
5. **App settings** — dumped to the gitignored `*.appsettings.json` files. They
   contain live secrets (Entra client secret, storage/Service Bus keys, SQL
   credentials, Riot API key). Move them to a password manager; don't commit.
6. **App Service managed certificates** for `caleague.net` / `func.caleague.net` —
   not exportable, but free and auto-recreated when you re-bind the custom domains.
7. **Application code** — deployed by the GitHub workflows in `.github/workflows`;
   nothing to back up.

## Recreating later

```powershell
az group create -n Champions -l southcentralus
az deployment group create -g Champions --template-file champions-export.bicep
```

Expect some manual follow-up:

- The template prompts for `vulnerabilityAssessments_Default_storageContainerPath`
  (SQL vulnerability-assessment container; any blob container path works).
- SQL admin login/password aren't in the export — add `administratorLogin` /
  `administratorLoginPassword` (or Entra-admin config) to the SQL server resource.
- Key Vault access-policy `objectId`s and any RBAC assignments reference old
  managed-identity principals; re-grant after the new apps' identities exist.
- Restore app settings from the dumps, then re-verify the ACS email sender domain
  (the DNS TXT/CNAME verification records are in the zone/Bicep) and re-add
  custom-domain bindings + managed certs.
- Storage account (`caleague`) and other globally-unique names must still be
  available. Import the bacpac and re-upload blobs.
- Some resources deploy in a required order; if a first deployment pass reports
  missing dependencies, just run the same deployment a second time.
