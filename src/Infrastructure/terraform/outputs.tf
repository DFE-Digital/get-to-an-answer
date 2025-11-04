output "api_url" {
  value = "https://${azurerm_container_app.gettoananswer-api.latest_revision_fqdn}"
}

output "admin_url" {
  value = "https://${azurerm_container_app.gettoananswer-admin.latest_revision_fqdn}"
}

output "frontend_url" {
  value = "https://${azurerm_container_app.gettoananswer-frontend.latest_revision_fqdn}"
}

# Virtual network-related outputs
output "azurerm_virtual_network" {
  value       = azurerm_virtual_network.gettoananswer_vnet.name
  description = "Virtual network name"
}

output "azurerm_network_security_group" {
  value       = azurerm_network_security_group.gettoananswer-nsg.name
  description = "Network security group name"
}

output "azurerm_subnet" {
  value       = azapi_resource.gettoananswer_main_subnet.name
  description = "Subnet name"
}

output "azurerm_private_dns_zone" {
  value       = azurerm_private_dns_zone.default.name
  description = "Private DNS Zone name"
}

output "azurerm_private_dns_zone_virtual_network_link" {
  value       = azurerm_private_dns_zone_virtual_network_link.default.name
  description = "Private DNS Zone Virtual Network Link name"
}

# MsSql related outputs
output "mssql_server_name" {
  value       = azurerm_mssql_server.gettoananswer_mssql_server.name
  description = "The name of the SQL Server"
}
output "mssql_database_name" {
  value       = azurerm_mssql_database.gettoananswer_mssql_db.name
  description = "The name of the SQL Database"
}
output "mssql_server_fqdn" {
  value       = azurerm_mssql_server.gettoananswer_mssql_server.fully_qualified_domain_name
  description = "The fully qualified domain name of the SQL Server"
}