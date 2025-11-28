################
# Secret values
################

# Create secrets from CI-provided values
resource "azurerm_key_vault_secret" "sql_admin_password" {
  name         = "${var.prefix}kv-uks-sql-pwd"
  value        = var.sql_admin_password
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "application/octet-stream"

  lifecycle {
    ignore_changes = [value]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

resource "azurerm_key_vault_secret" "ad_client_secret" {
  name         = "${var.prefix}kv-uks-ad-clst"
  value        = var.ad_client_secret
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "application/octet-stream"

  lifecycle {
    ignore_changes = [value]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

# Additional secrets for non-sensitive configuration values
resource "azurerm_key_vault_secret" "ad_tenant_id" {
  name         = "${var.prefix}kv-uks-ad-tnid"
  value        = var.ad_tenant_id
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "text/plain"

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

resource "azurerm_key_vault_secret" "ad_client_id" {
  name         = "${var.prefix}kv-uks-ad-clid"
  value        = var.ad_client_id
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "text/plain"

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

resource "azurerm_key_vault_secret" "connection_string" {
  name         = "${var.prefix}kv-uks-sqlcs"
  value        = "Server=tcp:${azurerm_mssql_server.gettoananswer_mssql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.gettoananswer_mssql_db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.gettoananswer_mssql_server.administrator_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "text/plain"

  lifecycle {
    ignore_changes = [value]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

resource "azurerm_key_vault_secret" "bs_connection_string" {
  name         = "${var.prefix}kv-uks-bs-conn-str"
  value        = azurerm_storage_account.sa.primary_connection_string
  key_vault_id = azurerm_key_vault.kv.id

  content_type = "text/plain"

  lifecycle {
    ignore_changes = [value]
  }

  tags = local.common_tags

  depends_on = [
    azurerm_role_assignment.kv_officer,
    azurerm_role_assignment.kv_administrator,
    azurerm_role_assignment.kv_admin_sp
  ]
}

