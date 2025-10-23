# Subscription-scoped inputs

# Resource Group
import {
  to = azurerm_resource_group.gettoananswer-rg
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa"
}

# App Service Plan
import {
  to = azurerm_service_plan.gettoananswer-web-asp
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/serverFarms/${var.prefix}asp-uks-web"
}

# Azure Container Registry
import {
  to = azurerm_container_registry.gettoananswer-registry
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.ContainerRegistry/registries/${var.prefix}acruksgtaa"
}

# Linux Web App - API
import {
  to = azurerm_linux_web_app.gettoananswer-api
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-api"
}

# Linux Web App - Admin
import {
  to = azurerm_linux_web_app.gettoananswer-admin
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-admin"
}

# Linux Web App - Frontend
import {
  to = azurerm_linux_web_app.gettoananswer-frontend
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-frontend"
}

# Virtual Network
import {
  to = azurerm_virtual_network.gettoananswer_vnet
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Network/virtualNetworks/${var.prefix}vnet-uks-gtaa"
}

# Network Security Group
import {
  to = azurerm_network_security_group.gettoananswer-nsg
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Network/networkSecurityGroups/${var.prefix}nsg-uks-gtaa"
}

# Subnet (created via azapi_resource)
# Note: azapi_resource uses the full ARM ID
import {
  to = azapi_resource.gettoananswer_main_subnet
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Network/virtualNetworks/${var.prefix}vnet-uks-gtaa/subnets/${var.prefix}subnet-uks-gtaa"
}

# Private DNS Zone
import {
  to = azurerm_private_dns_zone.default
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Network/privateDnsZones/${var.prefix}pdz-uks-gtaa.database.windows.net"
}

# Private DNS Zone VNet Link
import {
  to = azurerm_private_dns_zone_virtual_network_link.default
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Network/privateDnsZones/${var.prefix}pdz-uks-gtaa.database.windows.net/virtualNetworkLinks/${var.prefix}pdz-uks-vnet-link"
}

# MSSQL Server
import {
  to = azurerm_mssql_server.gettoananswer_mssql_server
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Sql/servers/${var.prefix}sql-uks-gtaa01"
}

# MSSQL Database
import {
  to = azurerm_mssql_database.gettoananswer_mssql_db
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Sql/servers/${var.prefix}sql-uks-gtaa01/databases/${var.prefix}db-uks-gtaa01"
}

# MSSQL VNet Rule
import {
  to = azurerm_mssql_virtual_network_rule.mssql_vnet_rule
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Sql/servers/${var.prefix}sql-uks-gtaa01/virtualNetworkRules/${var.prefix}sql-uks-mssql-vnet-rule"
}

# App Service VNet Integration (Swift) - API
import {
  to = azurerm_app_service_virtual_network_swift_connection.api_app_vn_conn
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-api/virtualNetworkConnections/${var.prefix}subnet-uks-gtaa"
}

# App Service VNet Integration (Swift) - Admin
import {
  to = azurerm_app_service_virtual_network_swift_connection.admin_app_vn_conn
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-admin/virtualNetworkConnections/${var.prefix}subnet-uks-gtaa"
}

# App Service VNet Integration (Swift) - Frontend
import {
  to = azurerm_app_service_virtual_network_swift_connection.frontend_app_vn_conn
  id = "/subscriptions/${var.subscription_id}/resourceGroups/${var.prefix}rg-uks-gtaa/providers/Microsoft.Web/sites/${var.prefix}app-uks-frontend/virtualNetworkConnections/${var.prefix}subnet-uks-gtaa"
}