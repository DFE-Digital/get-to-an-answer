resource "azurerm_key_vault" "key-vault" {
  name                = "${var.prefix}kyv-uks-keyvault"
  location            = var.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  tenant_id           = data.azurerm_client_config.client.tenant_id
  sku_name            = "standard"

  // ELZ guardrails
  soft_delete_retention_days     = 90
  purge_protection_enabled       = true
  public_network_access_enabled  = false

  // Allow Azure services bypass, default deny (firewall enabled by policy)
  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
  }

  // Access control: use default (Vault access policy model). Remove deprecated enable_rbac_authorization.
  // If RBAC is required, manage it outside with role assignments.

  tags = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_key_vault_access_policy" "github-kv-access" {
  key_vault_id       = azurerm_key_vault.key-vault.id
  tenant_id          = data.azurerm_client_config.client.tenant_id
  object_id          = data.azurerm_client_config.client.object_id
  secret_permissions = ["Get", "List", "Set", "Delete", "Purge", "Restore"]
}

/*resource "azurerm_key_vault_access_policy" "web-app-kv-access" {
  key_vault_id       = azurerm_key_vault.key-vault.id
  tenant_id          = azurerm_linux_web_app.gettoananswer-api.identity[0].tenant_id
  object_id          = azurerm_linux_web_app.gettoananswer-admin.identity[0].principal_id
  secret_permissions = ["Get"]
}

resource "azurerm_key_vault_access_policy" "web-app-staging-kv-access" {
  key_vault_id       = azurerm_key_vault.key-vault.id
  tenant_id          = azurerm_linux_web_app_slot.web-app-service-staging.identity[0].tenant_id
  object_id          = azurerm_linux_web_app_slot.web-app-service-staging.identity[0].principal_id
  secret_permissions = ["Get"]
}*/

resource "azurerm_key_vault_secret" "application-insights-connection-string" {
  key_vault_id = azurerm_key_vault.key-vault.id
  name         = "application-insights-connection-string"
  value        = azurerm_application_insights.application-insights.connection_string
  content_type = "text/plain"

  depends_on = [azurerm_key_vault_access_policy.github-kv-access]
}