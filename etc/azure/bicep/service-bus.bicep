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

resource startRegularSeasonQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'start-regular-season'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

resource startPlayoffQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'start-playoff'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

resource buildPlayoffBracketQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'build-playoff-bracket'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

resource fetchRiotLobbyEventsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'fetch-riot-lobby-events'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

resource downloadReplayQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: namespace
  name: 'download-replay'
  properties: {
    lockDuration: 'PT5M'
    maxDeliveryCount: 3
  }
}

output namespaceName string = namespace.name
