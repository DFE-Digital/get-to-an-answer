resource "azurerm_virtual_network" "gettoananswer_vnet" {
  name                = "${var.prefix}vnet-uks-gtaa"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  address_space       = ["10.0.0.0/16"]

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azurerm_network_security_group" "gettoananswer-nsg" {
  name                = "${var.prefix}nsg-uks-gtaa"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name

  # Compliant: no inbound from Internet; add only specific allows if required (e.g., from corporate IPs/VNet)
  security_rule {
    name                       = "Allow-HTTPS-From-VNet"
    priority                   = 200
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
    destination_port_ranges    = ["443"]
    source_port_range          = "*"
  }

  lifecycle {
    ignore_changes = [tags]
  }
}

resource "azapi_resource" "gettoananswer_main_subnet" {
  type      = "Microsoft.Network/virtualNetworks/subnets@2024-05-01"
  name      = "${var.prefix}subnet-uks-gtaa"
  parent_id = azurerm_virtual_network.gettoananswer_vnet.id

  body = {
    properties = {
      addressPrefixes = ["10.0.1.0/24"]
      delegations = [{
        name = "asp-delegation"
        properties = {
          serviceName = "Microsoft.Web/serverFarms"
        }
      }]
      serviceEndpoints = [
        {
          service   = "Microsoft.Sql"
          locations = [azurerm_resource_group.gettoananswer-rg.location]
        }
      ]
      networkSecurityGroup = {
        id = azurerm_network_security_group.gettoananswer-nsg.id
      }
    }
  }

  lifecycle {
    ignore_changes = [tags]
  }

  depends_on = [azurerm_network_security_group.gettoananswer-nsg]
}

resource "azurerm_private_dns_zone" "default" {
  name                = "${var.prefix}pdz-uks-gtaa.database.windows.net"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name

  lifecycle {
    ignore_changes = [tags]
  }

  depends_on = [azapi_resource.gettoananswer_main_subnet]
}

resource "azurerm_private_dns_zone_virtual_network_link" "default" {
  name                  = "${var.prefix}pdz-uks-vnet-link"
  private_dns_zone_name = azurerm_private_dns_zone.default.name
  virtual_network_id    = azurerm_virtual_network.gettoananswer_vnet.id
  resource_group_name   = azurerm_resource_group.gettoananswer-rg.name

  lifecycle {
    ignore_changes = [tags]
  }

  depends_on = [azapi_resource.gettoananswer_main_subnet]
}

#  VNet SQL Firewall Rule
resource "azurerm_mssql_virtual_network_rule" "mssql_vnet_rule" {
  name      = "${var.prefix}sql-uks-mssql-vnet-rule"
  server_id = azurerm_mssql_server.gettoananswer_mssql_server.id
  subnet_id = azapi_resource.gettoananswer_main_subnet.id
}
