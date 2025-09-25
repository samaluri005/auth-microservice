param prefix string
param location string

resource kv 'Microsoft.KeyVault/vaults@2024-01-01-preview' = {
  name: '${prefix}-kv'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: { name: 'standard', family: 'A' }
    enableSoftDelete: true
    enablePurgeProtection: false
  }
}

output keyVaultId string = kv.id
