param prefix string
param location string
param acrSku string

resource law 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: '${prefix}-law'
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: '${prefix}acr'
  location: location
  sku: { name: acrSku }
  properties: { adminUserEnabled: false }
}

resource containerEnv 'Microsoft.App/managedEnvironments@2023-10-01' = {
  name: '${prefix}-ca-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: law.properties.customerId
        sharedKey: listKeys(law.id, law.apiVersion).primarySharedKey
      }
    }
  }
}

output acrName string = acr.name
output acrLoginServer string = acr.properties.loginServer
