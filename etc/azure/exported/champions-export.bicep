param CommunicationServices_champions_acs_name string = 'champions-acs'
param ciamDirectories_caleague_onmicrosoft_com_name string = 'caleague.onmicrosoft.com'
param components_champions_appi_name string = 'champions-appi'
param dnszones_caleague_net_name string = 'caleague.net'
param emailServices_champions_acs_email_name string = 'champions-acs-email'
param namespaces_champions_sbns_name string = 'champions-sbns'
param privateDnsZones_privatelink_blob_core_windows_net_name string = 'privatelink.blob.core.windows.net'
param privateDnsZones_privatelink_database_windows_net_name string = 'privatelink.database.windows.net'
param privateDnsZones_privatelink_vaultcore_azure_net_name string = 'privatelink.vaultcore.azure.net'
param privateEndpoints_champions_pep_kv_name string = 'champions-pep-kv'
param privateEndpoints_champions_pep_sql_name string = 'champions-pep-sql'
param privateEndpoints_champions_pep_st_b_name string = 'champions-pep-st-b'
param serverfarms_champions_asp_name string = 'champions-asp'
param servers_champions_sql_name string = 'champions-sql'
param sites_champions_app_name string = 'champions-app'
param sites_champions_func_name string = 'champions-func'
param storageAccounts_caleague_name string = 'caleague'
param vaults_champions_kv_name string = 'champions-kv'
param virtualNetworks_champions_vnet_name string = 'champions-vnet'

@secure()
param vulnerabilityAssessments_Default_storageContainerPath string
param workspaces_champions_log_name string = 'champions-log'

resource ciamDirectories_caleague_onmicrosoft_com_name_resource 'Microsoft.AzureActiveDirectory/ciamDirectories@2023-05-17-preview' = {
  location: 'United States'
  name: ciamDirectories_caleague_onmicrosoft_com_name
  properties: {
    createTenantProperties: {
      countryCode: 'US'
      displayName: 'champions'
    }
    tenantId: 'f3b5affe-8fa4-4d75-bc2a-265603749644'
  }
  sku: {
    name: 'Base'
    tier: 'A0'
  }
}

resource emailServices_champions_acs_email_name_resource 'Microsoft.Communication/emailServices@2026-03-18' = {
  location: 'global'
  name: emailServices_champions_acs_email_name
  properties: {
    dataLocation: 'United States'
  }
}

resource vaults_champions_kv_name_resource 'Microsoft.KeyVault/vaults@2026-03-01-preview' = {
  location: 'southcentralus'
  name: vaults_champions_kv_name
  properties: {
    accessPolicies: [
      {
        objectId: '4ec31381-1387-4062-8046-a058864eda34'
        permissions: {
          certificates: [
            'get'
            'update'
            'create'
            'import'
            'delete'
          ]
          secrets: [
            'get'
            'set'
            'delete'
          ]
        }
        tenantId: '1a4da99d-8f6f-49bb-9c7e-70cf2c842569'
      }
      {
        objectId: '332980cf-ac30-4249-a0bd-16af9e3948ec'
        permissions: {
          secrets: [
            'get'
          ]
        }
        tenantId: '1a4da99d-8f6f-49bb-9c7e-70cf2c842569'
      }
    ]
    enableRbacAuthorization: true
    enableSoftDelete: true
    enabledForDeployment: true
    enabledForDiskEncryption: true
    enabledForTemplateDeployment: true
    provisioningState: 'Succeeded'
    publicNetworkAccess: 'Enabled'
    sku: {
      family: 'A'
      name: 'standard'
    }
    softDeleteRetentionInDays: 90
    tenantId: '1a4da99d-8f6f-49bb-9c7e-70cf2c842569'
    vaultUri: 'https://${vaults_champions_kv_name}.vault.azure.net/'
  }
}

resource dnszones_caleague_net_name_resource 'Microsoft.Network/dnszones@2023-07-01-preview' = {
  location: 'global'
  name: dnszones_caleague_net_name
  properties: {
    zoneType: 'Public'
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  location: 'global'
  name: privateDnsZones_privatelink_blob_core_windows_net_name
  properties: {}
}

resource privateDnsZones_privatelink_database_windows_net_name_resource 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  location: 'global'
  name: privateDnsZones_privatelink_database_windows_net_name
  properties: {}
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_resource 'Microsoft.Network/privateDnsZones@2024-06-01' = {
  location: 'global'
  name: privateDnsZones_privatelink_vaultcore_azure_net_name
  properties: {}
}

resource virtualNetworks_champions_vnet_name_resource 'Microsoft.Network/virtualNetworks@2025-07-01' = {
  location: 'southcentralus'
  name: virtualNetworks_champions_vnet_name
  properties: {
    addressSpace: {
      addressPrefixes: [
        '10.0.0.0/16'
      ]
    }
    enableDdosProtection: false
    encryption: {
      enabled: false
      enforcement: 'AllowUnencrypted'
    }
    privateEndpointVNetPolicies: 'Disabled'
    subnets: [
      {
        name: 'champions-snet-st-b'
        properties: {
          addressPrefixes: [
            '10.0.1.0/28'
          ]
          defaultOutboundAccess: false
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'champions-snet-kv'
        properties: {
          addressPrefixes: [
            '10.0.4.0/26'
          ]
          defaultOutboundAccess: false
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'champions-snet-app'
        properties: {
          addressPrefixes: [
            '10.0.3.0/26'
          ]
          defaultOutboundAccess: false
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverfarms'
              }
              type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'champions-snet-sql'
        properties: {
          addressPrefixes: [
            '10.0.0.0/26'
          ]
          defaultOutboundAccess: false
          delegations: []
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
      {
        name: 'champions-snet-func'
        properties: {
          addressPrefixes: [
            '10.0.2.0/26'
          ]
          defaultOutboundAccess: false
          delegations: [
            {
              name: 'delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverfarms'
              }
              type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
            }
          ]
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
    virtualNetworkPeerings: []
  }
}

resource workspaces_champions_log_name_resource 'Microsoft.OperationalInsights/workspaces@2025-07-01' = {
  location: 'southcentralus'
  name: workspaces_champions_log_name
  properties: {
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
      legacy: 0
      searchVersion: 1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    retentionInDays: 30
    sku: {
      name: 'pergb2018'
    }
    workspaceCapping: {
      dailyQuotaGb: json('-1')
    }
  }
}

resource namespaces_champions_sbns_name_resource 'Microsoft.ServiceBus/namespaces@2026-01-01' = {
  location: 'southcentralus'
  name: namespaces_champions_sbns_name
  properties: {
    disableLocalAuth: false
    geoDataReplication: {
      locations: [
        {
          locationName: 'southcentralus'
          roleType: 'Primary'
        }
      ]
      maxReplicationLagDurationInSeconds: 0
    }
    minimumTlsVersion: '1.2'
    platformCapabilities: {
      confidentialCompute: {
        mode: 'Disabled'
      }
    }
    premiumMessagingPartitions: 0
    publicNetworkAccess: 'Enabled'
    zoneRedundant: true
  }
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource servers_champions_sql_name_resource 'Microsoft.Sql/servers@2025-02-01-preview' = {
  kind: 'v12.0'
  location: 'southcentralus'
  name: servers_champions_sql_name
  properties: {
    administratorLogin: 'CloudSA0a6a8838'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: 'ryan@rycole.com'
      principalType: 'User'
      sid: '4dc40583-4d9b-4ed1-938a-6fd4a4ca3c45'
      tenantId: '1a4da99d-8f6f-49bb-9c7e-70cf2c842569'
    }
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    restrictOutboundNetworkAccess: 'Disabled'
    retentionDays: -1
    version: '12.0'
  }
}

resource storageAccounts_caleague_name_resource 'Microsoft.Storage/storageAccounts@2026-04-01' = {
  kind: 'StorageV2'
  location: 'southcentralus'
  name: storageAccounts_caleague_name
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
    allowCrossTenantReplication: false
    allowSharedKeyAccess: true
    defaultToOAuthAuthentication: false
    dnsEndpointType: 'Standard'
    dualStackEndpointPreference: {
      publishIpv6Endpoint: false
    }
    encryption: {
      keySource: 'Microsoft.Storage'
      requireInfrastructureEncryption: false
      services: {
        blob: {
          enabled: true
          keyType: 'Account'
        }
        file: {
          enabled: true
          keyType: 'Account'
        }
      }
    }
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
      ipRules: []
      ipv6Rules: []
      virtualNetworkRules: []
    }
    publicNetworkAccess: 'Enabled'
    supportsHttpsTrafficOnly: true
  }
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
}

resource serverfarms_champions_asp_name_resource 'Microsoft.Web/serverfarms@2024-11-01' = {
  kind: 'app'
  location: 'South Central US'
  name: serverfarms_champions_asp_name
  properties: {
    asyncScalingEnabled: false
    elasticScaleEnabled: false
    hyperV: false
    isSpot: false
    isXenon: false
    maximumElasticWorkerCount: 1
    perSiteScaling: false
    reserved: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
    zoneRedundant: false
  }
  sku: {
    capacity: 1
    family: 'Pv3'
    name: 'P0v3'
    size: 'P0v3'
    tier: 'Premium0V3'
  }
}

resource CommunicationServices_champions_acs_name_resource 'Microsoft.Communication/CommunicationServices@2026-03-18' = {
  location: 'global'
  name: CommunicationServices_champions_acs_name
  properties: {
    dataLocation: 'United States'
    linkedDomains: [
      emailServices_champions_acs_email_name_caleague_net.id
    ]
  }
}

resource emailServices_champions_acs_email_name_caleague_net 'Microsoft.Communication/emailServices/domains@2026-03-18' = {
  parent: emailServices_champions_acs_email_name_resource
  location: 'global'
  name: 'caleague.net'
  properties: {
    domainManagement: 'CustomerManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource components_champions_appi_name_resource 'microsoft.insights/components@2020-02-02' = {
  kind: 'web'
  location: 'southcentralus'
  name: components_champions_appi_name
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    IngestionMode: 'LogAnalytics'
    Request_Source: 'IbizaAIExtension'
    RetentionInDays: 90
    WorkspaceResourceId: workspaces_champions_log_name_resource.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource vaults_champions_kv_name_champions_pep_kv 'Microsoft.KeyVault/vaults/privateEndpointConnections@2026-03-01-preview' = {
  parent: vaults_champions_kv_name_resource
  location: 'southcentralus'
  name: 'champions-pep-kv'
  properties: {
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      actionsRequired: 'None'
      status: 'Approved'
    }
  }
}

resource Microsoft_Network_dnszones_A_dnszones_caleague_net_name 'Microsoft.Network/dnszones/A@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: '@'
  properties: {
    ARecords: [
      {
        ipv4Address: '40.119.12.21'
      }
    ]
    TTL: 3600
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource dnszones_caleague_net_name_func 'Microsoft.Network/dnszones/CNAME@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: 'func'
  properties: {
    CNAMERecord: {
      cname: 'champions-func.azurewebsites.net'
    }
    TTL: 3600
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource dnszones_caleague_net_name_selector1_azurecomm_prod_net_domainkey 'Microsoft.Network/dnszones/CNAME@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: 'selector1-azurecomm-prod-net._domainkey'
  properties: {
    CNAMERecord: {
      cname: 'selector1-azurecomm-prod-net._domainkey.azurecomm.net'
    }
    TTL: 3600
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource dnszones_caleague_net_name_selector2_azurecomm_prod_net_domainkey 'Microsoft.Network/dnszones/CNAME@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: 'selector2-azurecomm-prod-net._domainkey'
  properties: {
    CNAMERecord: {
      cname: 'selector2-azurecomm-prod-net._domainkey.azurecomm.net'
    }
    TTL: 3600
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource Microsoft_Network_dnszones_NS_dnszones_caleague_net_name 'Microsoft.Network/dnszones/NS@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: '@'
  properties: {
    NSRecords: [
      {
        nsdname: 'ns1-09.azure-dns.com.'
      }
      {
        nsdname: 'ns2-09.azure-dns.net.'
      }
      {
        nsdname: 'ns3-09.azure-dns.org.'
      }
      {
        nsdname: 'ns4-09.azure-dns.info.'
      }
    ]
    TTL: 172800
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource Microsoft_Network_dnszones_SOA_dnszones_caleague_net_name 'Microsoft.Network/dnszones/SOA@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: '@'
  properties: {
    SOARecord: {
      email: 'azuredns-hostmaster.microsoft.com'
      expireTime: 2419200
      host: 'ns1-09.azure-dns.com.'
      minimumTTL: 300
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
    TTL: 3600
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource Microsoft_Network_dnszones_TXT_dnszones_caleague_net_name 'Microsoft.Network/dnszones/TXT@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: '@'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          'v=spf1 include:spf.protection.outlook.com -all'
        ]
      }
    ]
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource dnszones_caleague_net_name_asuid 'Microsoft.Network/dnszones/TXT@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: 'asuid'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          '88A2B5DD803EA97E5D0E71186E472C722217A6978ACCF3CB70886047C2C31E7E'
        ]
      }
    ]
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource dnszones_caleague_net_name_asuid_func 'Microsoft.Network/dnszones/TXT@2023-07-01-preview' = {
  parent: dnszones_caleague_net_name_resource
  name: 'asuid.func'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          '88A2B5DD803EA97E5D0E71186E472C722217A6978ACCF3CB70886047C2C31E7E'
        ]
      }
    ]
    targetResource: {}
    trafficManagementProfile: {}
  }
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_caleague 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: 'caleague'
  properties: {
    aRecords: [
      {
        ipv4Address: '10.0.1.4'
      }
    ]
    metadata: {
      creator: 'created by private endpoint champions-pep-st-b with resource guid 14ee0fe0-4761-4d43-a820-a650a26bd9f7'
    }
    ttl: 10
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_champions_kv 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: 'champions-kv'
  properties: {
    aRecords: [
      {
        ipv4Address: '10.0.4.4'
      }
    ]
    metadata: {
      creator: 'created by private endpoint champions-pep-kv with resource guid c47f3d85-42ff-46c4-b633-6bb0e589714e'
    }
    ttl: 10
  }
}

resource privateDnsZones_privatelink_database_windows_net_name_champions_sql 'Microsoft.Network/privateDnsZones/A@2024-06-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  name: 'champions-sql'
  properties: {
    aRecords: [
      {
        ipv4Address: '10.0.0.4'
      }
    ]
    metadata: {
      creator: 'created by private endpoint champions-pep-sql with resource guid 5713ff12-591b-4e7d-bbe3-f167236bde62'
    }
    ttl: 10
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_blob_core_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2024-06-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  name: '@'
  properties: {
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
    ttl: 3600
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_database_windows_net_name 'Microsoft.Network/privateDnsZones/SOA@2024-06-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  name: '@'
  properties: {
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
    ttl: 3600
  }
}

resource Microsoft_Network_privateDnsZones_SOA_privateDnsZones_privatelink_vaultcore_azure_net_name 'Microsoft.Network/privateDnsZones/SOA@2024-06-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  name: '@'
  properties: {
    soaRecord: {
      email: 'azureprivatedns-host.microsoft.com'
      expireTime: 2419200
      host: 'azureprivatedns.net'
      minimumTtl: 10
      refreshTime: 3600
      retryTime: 300
      serialNumber: 1
    }
    ttl: 3600
  }
}

resource virtualNetworks_champions_vnet_name_champions_snet_app 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${virtualNetworks_champions_vnet_name}/champions-snet-app'
  properties: {
    addressPrefixes: [
      '10.0.3.0/26'
    ]
    defaultOutboundAccess: false
    delegations: [
      {
        name: 'delegation'
        properties: {
          serviceName: 'Microsoft.Web/serverfarms'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
      }
    ]
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_champions_vnet_name_resource
  ]
}

resource virtualNetworks_champions_vnet_name_champions_snet_func 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${virtualNetworks_champions_vnet_name}/champions-snet-func'
  properties: {
    addressPrefixes: [
      '10.0.2.0/26'
    ]
    defaultOutboundAccess: false
    delegations: [
      {
        name: 'delegation'
        properties: {
          serviceName: 'Microsoft.Web/serverfarms'
        }
        type: 'Microsoft.Network/virtualNetworks/subnets/delegations'
      }
    ]
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_champions_vnet_name_resource
  ]
}

resource virtualNetworks_champions_vnet_name_champions_snet_kv 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${virtualNetworks_champions_vnet_name}/champions-snet-kv'
  properties: {
    addressPrefixes: [
      '10.0.4.0/26'
    ]
    defaultOutboundAccess: false
    delegations: []
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_champions_vnet_name_resource
  ]
}

resource virtualNetworks_champions_vnet_name_champions_snet_sql 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${virtualNetworks_champions_vnet_name}/champions-snet-sql'
  properties: {
    addressPrefixes: [
      '10.0.0.0/26'
    ]
    defaultOutboundAccess: false
    delegations: []
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_champions_vnet_name_resource
  ]
}

resource virtualNetworks_champions_vnet_name_champions_snet_st_b 'Microsoft.Network/virtualNetworks/subnets@2025-07-01' = {
  name: '${virtualNetworks_champions_vnet_name}/champions-snet-st-b'
  properties: {
    addressPrefixes: [
      '10.0.1.0/28'
    ]
    defaultOutboundAccess: false
    delegations: []
    privateEndpointNetworkPolicies: 'Disabled'
    privateLinkServiceNetworkPolicies: 'Enabled'
  }
  dependsOn: [
    virtualNetworks_champions_vnet_name_resource
  ]
}

resource namespaces_champions_sbns_name_RootManageSharedAccessKey 'Microsoft.ServiceBus/namespaces/authorizationrules@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'RootManageSharedAccessKey'
  properties: {
    rights: [
      'Listen'
      'Manage'
      'Send'
    ]
  }
}

resource namespaces_champions_sbns_name_default 'Microsoft.ServiceBus/namespaces/networkrulesets@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'default'
  properties: {
    defaultAction: 'Allow'
    ipRules: []
    publicNetworkAccess: 'Enabled'
    trustedServiceAccessEnabled: false
    virtualNetworkRules: []
  }
}

resource namespaces_champions_sbns_name_build_playoff_bracket 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'build-playoff-bracket'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource namespaces_champions_sbns_name_build_regular_season 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'build-regular-season'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource namespaces_champions_sbns_name_download_replay 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'download-replay'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P14D'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT1M'
    maxDeliveryCount: 1
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource namespaces_champions_sbns_name_fetch_riot_lobby_events 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'fetch-riot-lobby-events'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource namespaces_champions_sbns_name_start_playoff 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'start-playoff'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource namespaces_champions_sbns_name_start_regular_season 'Microsoft.ServiceBus/namespaces/queues@2026-01-01' = {
  parent: namespaces_champions_sbns_name_resource
  location: 'southcentralus'
  name: 'start-regular-season'
  properties: {
    autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
    deadLetteringOnMessageExpiration: false
    defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    enableBatchedOperations: true
    enableExpress: false
    enablePartitioning: false
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
    maxMessageSizeInKilobytes: 256
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    status: 'Active'
  }
}

resource servers_champions_sql_name_ActiveDirectory 'Microsoft.Sql/servers/administrators@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'ActiveDirectory'
  properties: {
    administratorType: 'ActiveDirectory'
    login: 'ryan@rycole.com'
    sid: '4dc40583-4d9b-4ed1-938a-6fd4a4ca3c45'
    tenantId: '1a4da99d-8f6f-49bb-9c7e-70cf2c842569'
  }
}

resource servers_champions_sql_name_Default 'Microsoft.Sql/servers/advancedThreatProtectionSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource servers_champions_sql_name_CreateIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_champions_sql_name_DbParameterization 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_champions_sql_name_DefragmentIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_champions_sql_name_DropIndex 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
}

resource servers_champions_sql_name_ForceLastGoodPlan 'Microsoft.Sql/servers/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
}

resource Microsoft_Sql_servers_auditingPolicies_servers_champions_sql_name_Default 'Microsoft.Sql/servers/auditingPolicies@2014-04-01' = {
  parent: servers_champions_sql_name_resource
  location: 'South Central US'
  name: 'Default'
  properties: {
    auditingState: 'Disabled'
  }
}

resource Microsoft_Sql_servers_auditingSettings_servers_champions_sql_name_Default 'Microsoft.Sql/servers/auditingSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'default'
  properties: {
    auditActionsAndGroups: []
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    isStorageSecondaryKeyInUse: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource Microsoft_Sql_servers_azureADOnlyAuthentications_servers_champions_sql_name_Default 'Microsoft.Sql/servers/azureADOnlyAuthentications@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    azureADOnlyAuthentication: true
  }
}

resource Microsoft_Sql_servers_connectionPolicies_servers_champions_sql_name_default 'Microsoft.Sql/servers/connectionPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  location: 'southcentralus'
  name: 'default'
  properties: {
    connectionType: 'Default'
  }
}

resource servers_champions_sql_name_servers_champions_sql_name_db 'Microsoft.Sql/servers/databases@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  kind: 'v12.0,user'
  location: 'southcentralus'
  name: '${servers_champions_sql_name}db'
  properties: {
    availabilityZone: 'NoPreference'
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    isLedgerOn: false
    maintenanceConfigurationId: '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/providers/Microsoft.Maintenance/publicMaintenanceConfigurations/SQL_Default'
    maxSizeBytes: 32212254720
    readScale: 'Disabled'
    requestedBackupStorageRedundancy: 'Local'
    zoneRedundant: false
  }
  sku: {
    capacity: 10
    name: 'S0'
    tier: 'Standard'
  }
}

resource servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  location: 'South Central US'
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/auditingSettings@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    isAzureMonitorTargetEnabled: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    isAzureMonitorTargetEnabled: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_master_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Current'
  properties: {}
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    disabledAlerts: [
      ''
    ]
    emailAccountAdmins: false
    emailAddresses: [
      ''
    ]
    retentionDays: 0
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_champions_sql_name_master_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Current'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_champions_sql_name_master_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2025-02-01-preview' = {
  name: '${servers_champions_sql_name}/master/Default'
  properties: {
    recurringScans: {
      emailSubscriptionAdmins: true
      isEnabled: false
    }
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_devOpsAuditingSettings_servers_champions_sql_name_Default 'Microsoft.Sql/servers/devOpsAuditingSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_champions_sql_name_current 'Microsoft.Sql/servers/encryptionProtector@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  kind: 'servicemanaged'
  name: 'current'
  properties: {
    autoRotationEnabled: false
    serverKeyName: 'ServiceManaged'
    serverKeyType: 'ServiceManaged'
  }
}

resource Microsoft_Sql_servers_extendedAuditingSettings_servers_champions_sql_name_Default 'Microsoft.Sql/servers/extendedAuditingSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'default'
  properties: {
    auditActionsAndGroups: []
    isAzureMonitorTargetEnabled: false
    isManagedIdentityInUse: false
    isStorageSecondaryKeyInUse: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
}

resource servers_champions_sql_name_AllowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'AllowAllWindowsAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

resource servers_champions_sql_name_ClientIPAddress_2026_6_24_21_30_5 'Microsoft.Sql/servers/firewallRules@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'ClientIPAddress_2026-6-24_21-30-5'
  properties: {
    endIpAddress: '47.188.77.25'
    startIpAddress: '47.188.77.25'
  }
}

resource servers_champions_sql_name_ServiceManaged 'Microsoft.Sql/servers/keys@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  kind: 'servicemanaged'
  name: 'ServiceManaged'
  properties: {
    serverKeyType: 'ServiceManaged'
  }
}

resource Microsoft_Sql_servers_securityAlertPolicies_servers_champions_sql_name_Default 'Microsoft.Sql/servers/securityAlertPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    disabledAlerts: [
      ''
    ]
    emailAccountAdmins: false
    emailAddresses: [
      ''
    ]
    retentionDays: 0
    state: 'Disabled'
  }
}

resource Microsoft_Sql_servers_sqlVulnerabilityAssessments_servers_champions_sql_name_Default 'Microsoft.Sql/servers/sqlVulnerabilityAssessments@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
}

resource Microsoft_Sql_servers_vulnerabilityAssessments_servers_champions_sql_name_Default 'Microsoft.Sql/servers/vulnerabilityAssessments@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'Default'
  properties: {
    recurringScans: {
      emailSubscriptionAdmins: true
      isEnabled: false
    }
    storageContainerPath: vulnerabilityAssessments_Default_storageContainerPath
  }
}

resource storageAccounts_caleague_name_default 'Microsoft.Storage/storageAccounts/blobServices@2026-04-01' = {
  parent: storageAccounts_caleague_name_resource
  name: 'default'
  properties: {
    containerDeleteRetentionPolicy: {
      days: 7
      enabled: true
    }
    cors: {
      corsRules: []
    }
    deleteRetentionPolicy: {
      allowPermanentDelete: false
      days: 7
      enabled: true
    }
    staticWebsite: {
      enabled: false
    }
  }
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
}

resource Microsoft_Storage_storageAccounts_fileServices_storageAccounts_caleague_name_default 'Microsoft.Storage/storageAccounts/fileServices@2026-04-01' = {
  parent: storageAccounts_caleague_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
    protocolSettings: {
      smb: {
        encryptionInTransit: {
          required: true
        }
      }
    }
    shareDeleteRetentionPolicy: {
      days: 7
      enabled: true
    }
  }
  sku: {
    name: 'Standard_LRS'
    tier: 'Standard'
  }
}

resource storageAccounts_caleague_name_storageAccounts_caleague_name_3af82b49_b3f6_4cbc_9b84_b7abd53f10e7 'Microsoft.Storage/storageAccounts/privateEndpointConnections@2026-04-01' = {
  parent: storageAccounts_caleague_name_resource
  name: '${storageAccounts_caleague_name}.3af82b49-b3f6-4cbc-9b84-b7abd53f10e7'
  properties: {
    privateEndpoint: {}
    privateLinkServiceConnectionState: {
      actionRequired: 'None'
      description: 'Auto-Approved'
      status: 'Approved'
    }
  }
}

resource Microsoft_Storage_storageAccounts_queueServices_storageAccounts_caleague_name_default 'Microsoft.Storage/storageAccounts/queueServices@2026-04-01' = {
  parent: storageAccounts_caleague_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource Microsoft_Storage_storageAccounts_tableServices_storageAccounts_caleague_name_default 'Microsoft.Storage/storageAccounts/tableServices@2026-04-01' = {
  parent: storageAccounts_caleague_name_resource
  name: 'default'
  properties: {
    cors: {
      corsRules: []
    }
  }
}

resource sites_champions_app_name_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: 'ftp'
  properties: {
    allow: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_func_name_ftp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: 'ftp'
  properties: {
    allow: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_app_name_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: 'scm'
  properties: {
    allow: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_func_name_scm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: 'scm'
  properties: {
    allow: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_app_name_web 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: 'web'
  properties: {
    acrUseManagedIdentityCreds: false
    alwaysOn: true
    autoHealEnabled: false
    azureStorageAccounts: {}
    defaultDocuments: [
      'Default.htm'
      'Default.html'
      'Default.asp'
      'index.htm'
      'index.html'
      'iisstart.htm'
      'default.aspx'
      'index.php'
      'hostingstart.html'
    ]
    detailedErrorLoggingEnabled: false
    elasticWebAppScaleLimit: 0
    experiments: {
      rampUpRules: []
    }
    ftpsState: 'FtpsOnly'
    functionsRuntimeScaleMonitoringEnabled: false
    http20Enabled: false
    http20ProxyFlag: 0
    httpLoggingEnabled: false
    ipSecurityRestrictions: [
      {
        action: 'Allow'
        description: 'Allow all access'
        ipAddress: 'Any'
        name: 'Allow all'
        priority: 2147483647
      }
    ]
    loadBalancing: 'LeastRequests'
    localMySqlEnabled: false
    logsDirectorySizeLimit: 35
    managedPipelineMode: 'Integrated'
    managedServiceIdentityId: 26341
    minTlsVersion: '1.2'
    minimumElasticInstanceCount: 0
    netFrameworkVersion: 'v10.0'
    numberOfWorkers: 1
    preWarmedInstanceCount: 0
    publicNetworkAccess: 'Enabled'
    publishingUsername: 'REDACTED'
    remoteDebuggingEnabled: false
    requestTracingEnabled: false
    scmIpSecurityRestrictions: [
      {
        action: 'Allow'
        description: 'Allow all access'
        ipAddress: 'Any'
        name: 'Allow all'
        priority: 2147483647
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    scmMinTlsVersion: '1.2'
    scmType: 'None'
    use32BitWorkerProcess: true
    virtualApplications: [
      {
        physicalPath: 'site\\wwwroot'
        preloadEnabled: true
        virtualPath: '/'
      }
    ]
    vnetName: '35a71dfd-ee9e-4107-b16b-76ae9675625d_champions-snet-app'
    vnetPrivatePortsCount: 0
    vnetRouteAllEnabled: true
    webSocketsEnabled: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_func_name_web 'Microsoft.Web/sites/config@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: 'web'
  properties: {
    acrUseManagedIdentityCreds: false
    alwaysOn: true
    autoHealEnabled: false
    azureStorageAccounts: {}
    cors: {
      allowedOrigins: [
        'https://portal.azure.com'
      ]
      supportCredentials: false
    }
    defaultDocuments: [
      'Default.htm'
      'Default.html'
      'Default.asp'
      'index.htm'
      'index.html'
      'iisstart.htm'
      'default.aspx'
      'index.php'
    ]
    detailedErrorLoggingEnabled: false
    experiments: {
      rampUpRules: []
    }
    ftpsState: 'FtpsOnly'
    functionAppScaleLimit: 0
    functionsRuntimeScaleMonitoringEnabled: false
    http20Enabled: false
    http20ProxyFlag: 0
    httpLoggingEnabled: false
    ipSecurityRestrictions: [
      {
        action: 'Allow'
        description: 'Allow all access'
        ipAddress: 'Any'
        name: 'Allow all'
        priority: 2147483647
      }
    ]
    loadBalancing: 'LeastRequests'
    localMySqlEnabled: false
    logsDirectorySizeLimit: 35
    managedPipelineMode: 'Integrated'
    managedServiceIdentityId: 26342
    minTlsVersion: '1.2'
    minimumElasticInstanceCount: 0
    netFrameworkVersion: 'v10.0'
    numberOfWorkers: 1
    preWarmedInstanceCount: 0
    publicNetworkAccess: 'Enabled'
    publishingUsername: 'REDACTED'
    remoteDebuggingEnabled: false
    requestTracingEnabled: false
    scmIpSecurityRestrictions: [
      {
        action: 'Allow'
        description: 'Allow all access'
        ipAddress: 'Any'
        name: 'Allow all'
        priority: 2147483647
      }
    ]
    scmIpSecurityRestrictionsUseMain: false
    scmMinTlsVersion: '1.2'
    scmType: 'None'
    use32BitWorkerProcess: false
    virtualApplications: [
      {
        physicalPath: 'site\\wwwroot'
        preloadEnabled: true
        virtualPath: '/'
      }
    ]
    vnetName: '35a71dfd-ee9e-4107-b16b-76ae9675625d_champions-snet-func'
    vnetPrivatePortsCount: 0
    vnetRouteAllEnabled: true
    webSocketsEnabled: false
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_app_name_caleague_net 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: 'caleague.net'
  properties: {
    hostNameType: 'Verified'
    siteName: 'champions-app'
    sslState: 'SniEnabled'
    thumbprint: 'FEA298F20C0674B326F4FB5710975C8072063F48'
  }
}

resource sites_champions_app_name_sites_champions_app_name_azurewebsites_net 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: '${sites_champions_app_name}.azurewebsites.net'
  properties: {
    hostNameType: 'Verified'
    siteName: 'champions-app'
  }
}

resource sites_champions_func_name_sites_champions_func_name_azurewebsites_net 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: '${sites_champions_func_name}.azurewebsites.net'
  properties: {
    hostNameType: 'Verified'
    siteName: 'champions-func'
  }
}

resource sites_champions_func_name_func_caleague_net 'Microsoft.Web/sites/hostNameBindings@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: 'func.caleague.net'
  properties: {
    hostNameType: 'Verified'
    siteName: 'champions-func'
    sslState: 'SniEnabled'
    thumbprint: 'CF84827510E6C3CE6D5B0FD6DB7CE8772D4059E5'
  }
}

resource emailServices_champions_acs_email_name_caleague_net_donotreply 'microsoft.communication/emailservices/domains/senderusernames@2026-03-18' = {
  parent: emailServices_champions_acs_email_name_caleague_net
  name: 'donotreply'
  properties: {
    displayName: 'DoNotReply'
    username: 'DoNotReply'
  }
  dependsOn: [
    emailServices_champions_acs_email_name_resource
  ]
}

resource privateDnsZones_privatelink_blob_core_windows_net_name_37eponirwkrcy 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: privateDnsZones_privatelink_blob_core_windows_net_name_resource
  location: 'global'
  name: '37eponirwkrcy'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworks_champions_vnet_name_resource.id
    }
  }
}

resource privateDnsZones_privatelink_database_windows_net_name_37eponirwkrcy 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: privateDnsZones_privatelink_database_windows_net_name_resource
  location: 'global'
  name: '37eponirwkrcy'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworks_champions_vnet_name_resource.id
    }
  }
}

resource privateDnsZones_privatelink_vaultcore_azure_net_name_37eponirwkrcy 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2024-06-01' = {
  parent: privateDnsZones_privatelink_vaultcore_azure_net_name_resource
  location: 'global'
  name: '37eponirwkrcy'
  properties: {
    registrationEnabled: false
    resolutionPolicy: 'Default'
    virtualNetwork: {
      id: virtualNetworks_champions_vnet_name_resource.id
    }
  }
}

resource privateEndpoints_champions_pep_kv_name_resource 'Microsoft.Network/privateEndpoints@2025-07-01' = {
  location: 'southcentralus'
  name: privateEndpoints_champions_pep_kv_name
  properties: {
    customDnsConfigs: []
    customNetworkInterfaceName: '${privateEndpoints_champions_pep_kv_name}-nic'
    ipConfigurations: []
    ipVersionType: 'IPv4'
    manualPrivateLinkServiceConnections: []
    privateLinkServiceConnections: [
      {
        name: privateEndpoints_champions_pep_kv_name
        properties: {
          groupIds: [
            'vault'
          ]
          privateLinkServiceConnectionState: {
            actionsRequired: 'None'
            status: 'Approved'
          }
          privateLinkServiceId: vaults_champions_kv_name_resource.id
        }
      }
    ]
    subnet: {
    }
  }
}

resource privateEndpoints_champions_pep_sql_name_resource 'Microsoft.Network/privateEndpoints@2025-07-01' = {
  location: 'southcentralus'
  name: privateEndpoints_champions_pep_sql_name
  properties: {
    customDnsConfigs: []
    customNetworkInterfaceName: '${privateEndpoints_champions_pep_sql_name}-nic'
    ipConfigurations: []
    ipVersionType: 'IPv4'
    manualPrivateLinkServiceConnections: []
    privateLinkServiceConnections: [
      {
        name: privateEndpoints_champions_pep_sql_name
        properties: {
          groupIds: [
            'sqlServer'
          ]
          privateLinkServiceConnectionState: {
            actionsRequired: 'None'
            description: 'Auto-approved'
            status: 'Approved'
          }
          privateLinkServiceId: servers_champions_sql_name_resource.id
        }
      }
    ]
    subnet: {
    }
  }
}

resource privateEndpoints_champions_pep_st_b_name_resource 'Microsoft.Network/privateEndpoints@2025-07-01' = {
  location: 'southcentralus'
  name: privateEndpoints_champions_pep_st_b_name
  properties: {
    customDnsConfigs: []
    customNetworkInterfaceName: '${privateEndpoints_champions_pep_st_b_name}-nic'
    ipConfigurations: []
    ipVersionType: 'IPv4'
    manualPrivateLinkServiceConnections: []
    privateLinkServiceConnections: [
      {
        name: privateEndpoints_champions_pep_st_b_name
        properties: {
          groupIds: [
            'blob'
          ]
          privateLinkServiceConnectionState: {
            actionsRequired: 'None'
            description: 'Auto-Approved'
            status: 'Approved'
          }
          privateLinkServiceId: storageAccounts_caleague_name_resource.id
        }
      }
    ]
    subnet: {
    }
  }
}

resource privateEndpoints_champions_pep_kv_name_default 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2025-07-01' = {
  name: '${privateEndpoints_champions_pep_kv_name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-vaultcore-azure-net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_vaultcore_azure_net_name_resource.id
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_champions_pep_kv_name_resource
  ]
}

resource privateEndpoints_champions_pep_sql_name_default 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2025-07-01' = {
  name: '${privateEndpoints_champions_pep_sql_name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-database-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_database_windows_net_name_resource.id
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_champions_pep_sql_name_resource
  ]
}

resource privateEndpoints_champions_pep_st_b_name_default 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2025-07-01' = {
  name: '${privateEndpoints_champions_pep_st_b_name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'privatelink-blob-core-windows-net'
        properties: {
          privateDnsZoneId: privateDnsZones_privatelink_blob_core_windows_net_name_resource.id
        }
      }
    ]
  }
  dependsOn: [
    privateEndpoints_champions_pep_st_b_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/advancedThreatProtectionSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_CreateIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'CreateIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_DbParameterization 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'DbParameterization'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_DefragmentIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'DefragmentIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_DropIndex 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'DropIndex'
  properties: {
    autoExecuteValue: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_ForceLastGoodPlan 'Microsoft.Sql/servers/databases/advisors@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'ForceLastGoodPlan'
  properties: {
    autoExecuteValue: 'Enabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingPolicies_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/auditingPolicies@2014-04-01' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  location: 'South Central US'
  name: 'Default'
  properties: {
    auditingState: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_auditingSettings_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/auditingSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'default'
  properties: {
    isAzureMonitorTargetEnabled: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupLongTermRetentionPolicies_servers_champions_sql_name_servers_champions_sql_name_db_default 'Microsoft.Sql/servers/databases/backupLongTermRetentionPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'default'
  properties: {
    monthlyRetention: 'PT0S'
    timeBasedImmutability: 'Disabled'
    weekOfYear: 0
    weeklyRetention: 'PT0S'
    yearlyRetention: 'PT0S'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_backupShortTermRetentionPolicies_servers_champions_sql_name_servers_champions_sql_name_db_default 'Microsoft.Sql/servers/databases/backupShortTermRetentionPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'default'
  properties: {
    diffBackupIntervalInHours: 12
    retentionDays: 7
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_extendedAuditingSettings_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/extendedAuditingSettings@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'default'
  properties: {
    isAzureMonitorTargetEnabled: false
    retentionDays: 0
    state: 'Disabled'
    storageAccountSubscriptionId: '00000000-0000-0000-0000-000000000000'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_geoBackupPolicies_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/geoBackupPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Default'
  properties: {
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_servers_champions_sql_name_db_Current 'Microsoft.Sql/servers/databases/ledgerDigestUploads@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Current'
  properties: {}
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_securityAlertPolicies_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/securityAlertPolicies@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Default'
  properties: {
    disabledAlerts: [
      ''
    ]
    emailAccountAdmins: false
    emailAddresses: [
      ''
    ]
    retentionDays: 0
    state: 'Disabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_transparentDataEncryption_servers_champions_sql_name_servers_champions_sql_name_db_Current 'Microsoft.Sql/servers/databases/transparentDataEncryption@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Current'
  properties: {
    state: 'Enabled'
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource Microsoft_Sql_servers_databases_vulnerabilityAssessments_servers_champions_sql_name_servers_champions_sql_name_db_Default 'Microsoft.Sql/servers/databases/vulnerabilityAssessments@2025-02-01-preview' = {
  parent: servers_champions_sql_name_servers_champions_sql_name_db
  name: 'Default'
  properties: {
    recurringScans: {
      emailSubscriptionAdmins: true
      isEnabled: false
    }
  }
  dependsOn: [
    servers_champions_sql_name_resource
  ]
}

resource servers_champions_sql_name_champions_pep_sql_89a64767_ae1d_495a_a7a7_1c5332adf896 'Microsoft.Sql/servers/privateEndpointConnections@2025-02-01-preview' = {
  parent: servers_champions_sql_name_resource
  name: 'champions-pep-sql-89a64767-ae1d-495a-a7a7-1c5332adf896'
  properties: {
    privateEndpoint: {
      id: privateEndpoints_champions_pep_sql_name_resource.id
    }
    privateLinkServiceConnectionState: {
      description: 'Auto-approved'
      status: 'Approved'
    }
  }
}

resource storageAccounts_caleague_name_default_azure_webjobs_hosts 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-04-01' = {
  parent: storageAccounts_caleague_name_default
  name: 'azure-webjobs-hosts'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    immutableStorageWithVersioning: {
      enabled: false
    }
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccounts_caleague_name_resource
  ]
}

resource storageAccounts_caleague_name_default_azure_webjobs_secrets 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-04-01' = {
  parent: storageAccounts_caleague_name_default
  name: 'azure-webjobs-secrets'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    immutableStorageWithVersioning: {
      enabled: false
    }
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccounts_caleague_name_resource
  ]
}

resource storageAccounts_caleague_name_default_bs_team_logo 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-04-01' = {
  parent: storageAccounts_caleague_name_default
  name: 'bs-team-logo'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    immutableStorageWithVersioning: {
      enabled: false
    }
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccounts_caleague_name_resource
  ]
}

resource storageAccounts_caleague_name_default_bs_user_logo 'Microsoft.Storage/storageAccounts/blobServices/containers@2026-04-01' = {
  parent: storageAccounts_caleague_name_default
  name: 'bs-user-logo'
  properties: {
    defaultEncryptionScope: '$account-encryption-key'
    denyEncryptionScopeOverride: false
    immutableStorageWithVersioning: {
      enabled: false
    }
    publicAccess: 'None'
  }
  dependsOn: [
    storageAccounts_caleague_name_resource
  ]
}

resource storageAccounts_caleague_name_default_azure_webjobs_blobtrigger_champions_func 'Microsoft.Storage/storageAccounts/queueServices/queues@2026-04-01' = {
  parent: Microsoft_Storage_storageAccounts_queueServices_storageAccounts_caleague_name_default
  name: 'azure-webjobs-blobtrigger-champions-func'
  properties: {
    metadata: {}
  }
  dependsOn: [
    storageAccounts_caleague_name_resource
  ]
}

resource sites_champions_app_name_resource 'Microsoft.Web/sites@2024-11-01' = {
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'app'
  location: 'South Central US'
  name: sites_champions_app_name
  properties: {
    clientAffinityEnabled: true
    clientAffinityProxyEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    containerSize: 0
    customDomainVerificationId: '88A2B5DD803EA97E5D0E71186E472C722217A6978ACCF3CB70886047C2C31E7E'
    dailyMemoryTimeQuota: 0
    dnsConfiguration: {}
    enabled: true
    endToEndEncryptionEnabled: false
    hostNameSslStates: [
      {
        hostType: 'Standard'
        name: 'caleague.net'
        sslState: 'SniEnabled'
        thumbprint: 'FEA298F20C0674B326F4FB5710975C8072063F48'
      }
      {
        hostType: 'Standard'
        name: '${sites_champions_app_name}.azurewebsites.net'
        sslState: 'Disabled'
      }
      {
        hostType: 'Repository'
        name: '${sites_champions_app_name}.scm.azurewebsites.net'
        sslState: 'Disabled'
      }
    ]
    hostNamesDisabled: false
    httpsOnly: true
    hyperV: false
    ipMode: 'IPv4'
    isXenon: false
    keyVaultReferenceIdentity: 'SystemAssigned'
    outboundVnetRouting: {
      allTraffic: false
      applicationTraffic: true
      backupRestoreTraffic: false
      contentShareTraffic: false
      imagePullTraffic: false
    }
    publicNetworkAccess: 'Enabled'
    redundancyMode: 'None'
    reserved: false
    scmSiteAlsoStopped: false
    serverFarmId: serverfarms_champions_asp_name_resource.id
    siteConfig: {
      acrUseManagedIdentityCreds: false
      alwaysOn: true
      functionAppScaleLimit: 0
      http20Enabled: false
      minimumElasticInstanceCount: 0
      numberOfWorkers: 1
    }
    storageAccountRequired: false
    virtualNetworkSubnetId: virtualNetworks_champions_vnet_name_champions_snet_app.id
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_func_name_resource 'Microsoft.Web/sites@2024-11-01' = {
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  location: 'South Central US'
  name: sites_champions_func_name
  properties: {
    clientAffinityEnabled: false
    clientAffinityProxyEnabled: false
    clientCertEnabled: false
    clientCertMode: 'Required'
    containerSize: 1536
    customDomainVerificationId: '88A2B5DD803EA97E5D0E71186E472C722217A6978ACCF3CB70886047C2C31E7E'
    dailyMemoryTimeQuota: 0
    dnsConfiguration: {}
    enabled: true
    endToEndEncryptionEnabled: false
    hostNameSslStates: [
      {
        hostType: 'Standard'
        name: '${sites_champions_func_name}.azurewebsites.net'
        sslState: 'Disabled'
      }
      {
        hostType: 'Repository'
        name: '${sites_champions_func_name}.scm.azurewebsites.net'
        sslState: 'Disabled'
      }
      {
        hostType: 'Standard'
        name: 'func.caleague.net'
        sslState: 'SniEnabled'
        thumbprint: 'CF84827510E6C3CE6D5B0FD6DB7CE8772D4059E5'
      }
    ]
    hostNamesDisabled: false
    httpsOnly: true
    hyperV: false
    ipMode: 'IPv4'
    isXenon: false
    keyVaultReferenceIdentity: 'SystemAssigned'
    outboundVnetRouting: {
      allTraffic: false
      applicationTraffic: true
      backupRestoreTraffic: false
      contentShareTraffic: false
      imagePullTraffic: false
    }
    publicNetworkAccess: 'Enabled'
    redundancyMode: 'None'
    reserved: false
    scmSiteAlsoStopped: false
    serverFarmId: serverfarms_champions_asp_name_resource.id
    siteConfig: {
      acrUseManagedIdentityCreds: false
      alwaysOn: true
      functionAppScaleLimit: 0
      http20Enabled: false
      minimumElasticInstanceCount: 0
      numberOfWorkers: 1
    }
    storageAccountRequired: false
    virtualNetworkSubnetId: virtualNetworks_champions_vnet_name_champions_snet_func.id
  }
  tags: {
    'hidden-link: /app-insights-resource-id': '/subscriptions/ea3dd5ca-0133-4319-8bf7-eed71ca621d8/resourceGroups/Champions/providers/microsoft.insights/components/champions-appi'
  }
}

resource sites_champions_app_name_35a71dfd_ee9e_4107_b16b_76ae9675625d_champions_snet_app 'Microsoft.Web/sites/virtualNetworkConnections@2024-11-01' = {
  parent: sites_champions_app_name_resource
  location: 'South Central US'
  name: '35a71dfd-ee9e-4107-b16b-76ae9675625d_champions-snet-app'
  properties: {
    isSwift: true
    vnetResourceId: virtualNetworks_champions_vnet_name_champions_snet_app.id
  }
}

resource sites_champions_func_name_35a71dfd_ee9e_4107_b16b_76ae9675625d_champions_snet_func 'Microsoft.Web/sites/virtualNetworkConnections@2024-11-01' = {
  parent: sites_champions_func_name_resource
  location: 'South Central US'
  name: '35a71dfd-ee9e-4107-b16b-76ae9675625d_champions-snet-func'
  properties: {
    isSwift: true
    vnetResourceId: virtualNetworks_champions_vnet_name_champions_snet_func.id
  }
}
