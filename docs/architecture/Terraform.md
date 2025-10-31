<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 1.13.3 |
| <a name="requirement_azapi"></a> [azapi](#requirement\_azapi) | 2.7.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | ~> 4.45 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azapi"></a> [azapi](#provider\_azapi) | 2.7.0 |
| <a name="provider_azurerm"></a> [azurerm](#provider\_azurerm) | ~> 4.45 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azapi_resource.gettoananswer_main_subnet](https://registry.terraform.io/providers/Azure/azapi/2.7.0/docs/resources/resource) | resource |
| [azurerm_app_service_virtual_network_swift_connection.admin_app_vn_conn](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service_virtual_network_swift_connection) | resource |
| [azurerm_app_service_virtual_network_swift_connection.api_app_vn_conn](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service_virtual_network_swift_connection) | resource |
| [azurerm_app_service_virtual_network_swift_connection.frontend_app_vn_conn](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/app_service_virtual_network_swift_connection) | resource |
| [azurerm_container_registry.gettoananswer-registry](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/container_registry) | resource |
| [azurerm_linux_web_app.gettoananswer-admin](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/linux_web_app) | resource |
| [azurerm_linux_web_app.gettoananswer-api](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/linux_web_app) | resource |
| [azurerm_linux_web_app.gettoananswer-frontend](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/linux_web_app) | resource |
| [azurerm_mssql_database.gettoananswer_mssql_db](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/mssql_database) | resource |
| [azurerm_mssql_server.gettoananswer_mssql_server](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/mssql_server) | resource |
| [azurerm_mssql_virtual_network_rule.mssql_vnet_rule](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/mssql_virtual_network_rule) | resource |
| [azurerm_network_security_group.gettoananswer-nsg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/network_security_group) | resource |
| [azurerm_private_dns_zone.default](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_dns_zone) | resource |
| [azurerm_private_dns_zone_virtual_network_link.default](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/private_dns_zone_virtual_network_link) | resource |
| [azurerm_resource_group.gettoananswer-rg](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/resource_group) | resource |
| [azurerm_service_plan.gettoananswer-web-asp](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/service_plan) | resource |
| [azurerm_virtual_network.gettoananswer_vnet](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/virtual_network) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_admin_image_name"></a> [admin\_image\_name](#input\_admin\_image\_name) | The repository name and tag for the admin container. | `string` | n/a | yes |
| <a name="input_api_image_name"></a> [api\_image\_name](#input\_api\_image\_name) | The repository name and tag for the API container. | `string` | n/a | yes |
| <a name="input_env"></a> [env](#input\_env) | Environment (dev, test, prod) | `string` | n/a | yes |
| <a name="input_frontend_image_name"></a> [frontend\_image\_name](#input\_frontend\_image\_name) | The repository name and tag for the frontend container. | `string` | n/a | yes |
| <a name="input_location"></a> [location](#input\_location) | Azure region | `string` | `"uksouth"` | no |
| <a name="input_prefix"></a> [prefix](#input\_prefix) | Prefix for resource names | `string` | `"s213"` | no |
| <a name="input_product"></a> [product](#input\_product) | Name of the project | `string` | `"Find Education and Training"` | no |
| <a name="input_sql_admin_password"></a> [sql\_admin\_password](#input\_sql\_admin\_password) | The administrator password of the SQL logical server. | `string` | `null` | no |
| <a name="input_sql_admin_username"></a> [sql\_admin\_username](#input\_sql\_admin\_username) | The administrator username of the SQL logical server. | `string` | `"azureadmin"` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_admin_url"></a> [admin\_url](#output\_admin\_url) | n/a |
| <a name="output_api_url"></a> [api\_url](#output\_api\_url) | n/a |
| <a name="output_azurerm_network_security_group"></a> [azurerm\_network\_security\_group](#output\_azurerm\_network\_security\_group) | Network security group name |
| <a name="output_azurerm_private_dns_zone"></a> [azurerm\_private\_dns\_zone](#output\_azurerm\_private\_dns\_zone) | Private DNS Zone name |
| <a name="output_azurerm_private_dns_zone_virtual_network_link"></a> [azurerm\_private\_dns\_zone\_virtual\_network\_link](#output\_azurerm\_private\_dns\_zone\_virtual\_network\_link) | Private DNS Zone Virtual Network Link name |
| <a name="output_azurerm_subnet"></a> [azurerm\_subnet](#output\_azurerm\_subnet) | Subnet name |
| <a name="output_azurerm_virtual_network"></a> [azurerm\_virtual\_network](#output\_azurerm\_virtual\_network) | Virtual network name |
| <a name="output_frontend_url"></a> [frontend\_url](#output\_frontend\_url) | n/a |
| <a name="output_mssql_database_name"></a> [mssql\_database\_name](#output\_mssql\_database\_name) | The name of the SQL Database |
| <a name="output_mssql_server_fqdn"></a> [mssql\_server\_fqdn](#output\_mssql\_server\_fqdn) | The fully qualified domain name of the SQL Server |
| <a name="output_mssql_server_name"></a> [mssql\_server\_name](#output\_mssql\_server\_name) | The name of the SQL Server |
<!-- END_TF_DOCS -->