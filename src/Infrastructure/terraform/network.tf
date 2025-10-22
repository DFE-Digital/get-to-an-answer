resource "azurerm_virtual_network" "gettoananswer_vnet" {
  name                = "${var.prefix}vnet-uks-gtaa"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  address_space       = ["10.0.0.0/16"]
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

  # Optional: allow internal communication within VNet on any port (adjust if needed)
  security_rule {
    name                       = "Allow-Internal-VNet"
    priority                   = 210
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "*"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "VirtualNetwork"
    source_port_range          = "*"
    destination_port_range     = "*"
  }

  # No rule exposing management ports (22/3389/etc.) to Internet
}

resource "azurerm_subnet" "gettoananswer_main_subnet" {
  name                 = "${var.prefix}subnet-uks-gta01"
  virtual_network_name = azurerm_virtual_network.gettoananswer_vnet.name
  resource_group_name  = azurerm_resource_group.gettoananswer-rg.name
  address_prefixes     = ["10.0.1.0/24"]

  service_endpoints = ["Microsoft.Sql", "Microsoft.Storage"]

  delegation {
    name = "asp-delegation"
    service_delegation {
      name = "Microsoft.Web/serverFarms"
      actions = [
        "Microsoft.Network/virtualNetworks/subnets/join/action"
      ]
    }
  }
}

resource "azurerm_subnet_network_security_group_association" "default" {
  subnet_id                 = azurerm_subnet.gettoananswer_main_subnet.id
  network_security_group_id = azurerm_network_security_group.gettoananswer-nsg.id
}

resource "azurerm_private_dns_zone" "default" {
  name                = "${var.prefix}pdz-uks-gtaa.database.windows.net"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name

  depends_on = [azurerm_subnet_network_security_group_association.default]
}

resource "azurerm_private_dns_zone_virtual_network_link" "default" {
  name                  = "${var.prefix}pdz-uks-vnet-link"
  private_dns_zone_name = azurerm_private_dns_zone.default.name
  virtual_network_id    = azurerm_virtual_network.gettoananswer_vnet.id
  resource_group_name   = azurerm_resource_group.gettoananswer-rg.name

  depends_on = [azurerm_subnet.gettoananswer_main_subnet]
}

#  VNet SQL Firewall Rule
resource "azurerm_mssql_virtual_network_rule" "mssql_vnet_rule" {
  name      = "${var.prefix}sql-uks-mssql-vnet-rule"
  server_id = azurerm_mssql_server.gettoananswer_mssql_server.id
  subnet_id = azurerm_subnet.gettoananswer_main_subnet.id
}

# API VNet Integration
resource "azurerm_app_service_virtual_network_swift_connection" "api_app_vn_conn" {
  app_service_id = azurerm_linux_web_app.gettoananswer-api.id
  subnet_id      = azurerm_subnet.gettoananswer_main_subnet.id

  depends_on = [azurerm_linux_web_app.gettoananswer-api, azurerm_subnet.gettoananswer_main_subnet]
}

# Admin VNet Integration
resource "azurerm_app_service_virtual_network_swift_connection" "admin_app_vn_conn" {
  app_service_id = azurerm_linux_web_app.gettoananswer-admin.id
  subnet_id      = azurerm_subnet.gettoananswer_main_subnet.id

  depends_on = [azurerm_linux_web_app.gettoananswer-admin, azurerm_subnet.gettoananswer_main_subnet]
}

# Website VNet Integration
resource "azurerm_app_service_virtual_network_swift_connection" "frontend_app_vn_conn" {
  app_service_id = azurerm_linux_web_app.gettoananswer-frontend.id
  subnet_id      = azurerm_subnet.gettoananswer_main_subnet.id

  depends_on = [azurerm_linux_web_app.gettoananswer-frontend, azurerm_subnet.gettoananswer_main_subnet]
}
