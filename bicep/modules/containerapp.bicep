param prefix string
param location string
param acrName string
param imageName string

resource containerEnv 'Microsoft.App/managedEnvironments@2023-10-01' existing = {
  name: '${prefix}-ca-env'
}

resource containerApp 'Microsoft.App/containerApps@2023-10-01' = {
  name: '${prefix}-auth-app'
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      ingress: { external: true, targetPort: 80 }
      registries: [
        { server: acrName, identity: { type: 'SystemAssigned' } }
      ]
    }
    template: {
      containers: [
        { name: 'auth', image: imageName, resources: { cpu: 0.5, memory: '1.0Gi' } }
      ]
    }
  }
}

output containerAppFqdn string = 'https://${containerApp.name}.${location}.azurecontainerapps.io'
