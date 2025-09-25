param prefix string
param location string
param administratorLogin string
@secure()
param administratorLoginPassword string

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-04-30-preview' = {
  name: '${prefix}-pg'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
    capacity: 1
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: '15'
    storage: { storageSizeGb: 32 }
    backup: { backupRetentionDays: 7 }
  }
}

output fqdn string = postgres.properties.fullyQualifiedDomainName
