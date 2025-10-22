resource "azurerm_mssql_server" "gettoananswer_mssql_server" {
  name                         = "${var.prefix}sql-uks-gtaa01"
  resource_group_name          = azurerm_resource_group.gettoananswer-rg.name
  location                     = azurerm_resource_group.gettoananswer-rg.location
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  version                      = "12.0"

  tags = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_mssql_database" "gettoananswer_mssql_db" {
  name       = "${var.prefix}db-uks-gtaa01"
  server_id  = azurerm_mssql_server.gettoananswer_mssql_server.id
  sku_name   = "S0"

  max_size_gb                 = 2

  tags = {
    Environment = var.env
    Product     = var.product
  }

  depends_on = [azurerm_mssql_server.gettoananswer_mssql_server]
}

