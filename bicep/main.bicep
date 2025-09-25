// main.bicep - orchestrates modules
param environment string // dev|sit|uat|preprod|prod
param prefix string // e.g. 'ecauth'
param location string = resourceGroup().location

// Optional parameters with safe defaults
@allowed([ 'Basic' 'Standard' 'Premium' ])
param acrSku string = 'Standard'

@secure()
param postgresAdminPassword string

var namePrefix = '${prefix}-${environment}'

module core 'modules/core.bicep' = {
  name: 'core-${environment}'
  params: {
    prefix: namePrefix
    location: location
    acrSku: acrSku
  }
}

module postgres 'modules/postgres.bicep' = {
  name: 'postgres-${environment}'
  params: {
    prefix: namePrefix
    location: location
    administratorLogin: 'pgadmin'
    administratorLoginPassword: postgresAdminPassword
  }
}

module kv 'modules/keyvault.bicep' = {
  name: 'kv-${environment}'
  params: {
    prefix: namePrefix
    location: location
  }
}

module app 'modules/containerapp.bicep' = {
  name: 'containerapp-${environment}'
  params: {
    prefix: namePrefix
    location: location
    acrName: core.outputs.acrName
    imageName: '${core.outputs.acrLoginServer}/auth:latest' // during deploy, pipeline updates image to specific tag
  }
}

output acrLoginServer string = core.outputs.acrLoginServer
output containerAppFqdn string = app.outputs.containerAppFqdn
output postgresFqdn string = postgres.outputs.fqdn
output keyVaultId string = kv.outputs.keyVaultId
