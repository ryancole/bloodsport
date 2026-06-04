param location string = resourceGroup().location
param namespaceName string = 'bloodsport-servicebus'
param skuName string = 'Standard'

resource namespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: skuName
  }
}

resource buildRegularSeasonQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'build-regular-season'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

output namespaceName string = namespace.name
output namespaceConnectionString string = listKeys('${namespace.id}/AuthorizationRules/RootManageSharedAccessKey', namespace.apiVersion).primaryConnectionString
