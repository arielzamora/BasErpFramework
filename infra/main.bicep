@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name of the existing Azure Container Registry')
param acrLoginServer string

@description('Username for ACR')
param acrUsername string

@secure()
@description('Password for ACR')
param acrPassword string

@description('Image tag for backend')
param backendImage string

@description('Image tag for frontend')
param frontendImage string

@secure()
@description('Administrator password for the SQL Server')
param sqlAdminPassword string

@description('Name for the backend container app')
param backendAppName string = 'ca-bas-erp-backend'

@description('Name for the frontend container app')
param frontendAppName string = 'ca-bas-erp-frontend'

// Names generated based on the resource group to ensure uniqueness
var uniqueStringSuffix = uniqueString(resourceGroup().id)
var sqlServerName = 'sql-bas-erp-${uniqueStringSuffix}'
var sqlDatabaseName = 'BasErpBd'
var logAnalyticsName = 'law-bas-erp-${uniqueStringSuffix}'
var containerAppEnvName = 'cae-bas-erp-${uniqueStringSuffix}'

// 1. Azure SQL Server
resource sqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

// Allow Azure services to access the SQL Server
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2022-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// 2. Azure SQL Database (Multitenant: Basic Tier)
var tenants = [
  'Default'
  'TenantA'
  'TenantB'
]

resource sqlDatabases 'Microsoft.Sql/servers/databases@2022-05-01-preview' = [for tenant in tenants: {
  parent: sqlServer
  name: '${sqlDatabaseName}_${tenant}'
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
}]

// 3. Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// 4. Container Apps Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// Build the SQL Connection string dynamically (Base connection points to Default)
var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName}_Default;Persist Security Info=False;User ID=sqladmin;Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

// 5. Backend Container App
resource backendApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: backendAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080 // .NET 8+ default port
      }
      registries: [
        {
          server: acrLoginServer
          username: acrUsername
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: acrPassword
        }
        {
          name: 'sql-connection'
          value: sqlConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'backend'
          image: '${acrLoginServer}/${backendImage}'
          env: [
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0 // Scale to zero when not used to save money
        maxReplicas: 2
      }
    }
  }
}

// 6. Frontend Container App
resource frontendApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: frontendAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 80 // NGINX default port
      }
      registries: [
        {
          server: acrLoginServer
          username: acrUsername
          passwordSecretRef: 'acr-password'
        }
      ]
      secrets: [
        {
          name: 'acr-password'
          value: acrPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'frontend'
          image: '${acrLoginServer}/${frontendImage}'
          env: [
            {
              name: 'BACKEND_URL'
              value: 'https://${backendApp.properties.configuration.ingress.fqdn}'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 2
      }
    }
  }
}

output backendUrl string = backendApp.properties.configuration.ingress.fqdn
output frontendUrl string = frontendApp.properties.configuration.ingress.fqdn
