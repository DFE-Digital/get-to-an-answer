

# Private DNS zone for Key Vault (vault and managed HSM share privatelink.vaultcore.azure.net)
resource "azurerm_private_dns_zone" "kv" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
}

# Link DNS zone to the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "kv_link" {
  name                  = "${var.prefix}-vnet-link"
  resource_group_name   = azurerm_resource_group.gettoananswer-rg.name
  private_dns_zone_name = azurerm_private_dns_zone.kv.name
  virtual_network_id    = azurerm_virtual_network.gettoananswer_vnet.id
  registration_enabled  = false
}

resource "azurerm_key_vault" "kv" {
  name                        = "${var.prefix}kv-uks-gtaa"
  location                    = var.location
  resource_group_name         = azurerm_resource_group.gettoananswer-rg.name
  tenant_id                   = data.azurerm_client_config.client.tenant_id

  # Guardrails
  soft_delete_retention_days  = 90
  purge_protection_enabled    = true

  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
  # For policy’s evaluatedExpressions: createMode must be "recover"
  sku_name                    = "standard"
  public_network_access_enabled = true
  
  # Network ACLs – only private endpoint, no public internet
  # Keep default deny; private endpoint bypasses this
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }
  
  rbac_authorization_enabled = true

  tags = local.common_tags
}

# Private Endpoint to the KV (moved to dedicated, non-delegated subnet)
resource "azurerm_private_endpoint" "kv_pe" {
  name                = "${var.prefix}kve-uks-pri-endpoint"
  location            = var.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  subnet_id           = azapi_resource.gettoananswer_kv_subnet.id

  private_service_connection {
    name                           = "${var.prefix}-pe-conn"
    private_connection_resource_id = azurerm_key_vault.kv.id
    is_manual_connection           = false
    subresource_names = ["vault"]
  }

  private_dns_zone_group {
    name = "${var.prefix}-pdns"
    private_dns_zone_ids = [azurerm_private_dns_zone.kv.id]
  }
}

# RBAC: allow app identity to read secrets (use object_id, not client_id)

data "azurerm_role_definition" "kv_secrets_user" {
  name  = "Key Vault Secrets User"
  scope = azurerm_key_vault.kv.id
}

data "azurerm_role_definition" "kv_secrets_officer" {
  name  = "Key Vault Secrets Officer"
  scope = azurerm_key_vault.kv.id
}

data "azurerm_role_definition" "kv_admin" {
  name  = "Key Vault Administrator"
  scope = azurerm_key_vault.kv.id
}

###################
# role assignments
###################

resource "azurerm_role_assignment" "kv_officer" {
  scope                = azurerm_key_vault.kv.id
  role_definition_id  = data.azurerm_role_definition.kv_secrets_officer.role_definition_id
  principal_id         = azurerm_user_assigned_identity.gtaa-identity.principal_id
  principal_type       = "ServicePrincipal"
}
resource "azurerm_role_assignment" "kv_user" {
  scope                = azurerm_key_vault.kv.id
  role_definition_id = data.azurerm_role_definition.kv_secrets_user.role_definition_id
  principal_id         = azurerm_user_assigned_identity.gtaa-identity.principal_id
  principal_type       = "ServicePrincipal"
}
resource "azurerm_role_assignment" "kv_administrator" {
  scope                = azurerm_key_vault.kv.id
  role_definition_id = data.azurerm_role_definition.kv_admin.role_definition_id
  principal_id         = azurerm_user_assigned_identity.gtaa-identity.principal_id
  principal_type       = "ServicePrincipal"
}
resource "azurerm_role_assignment" "kv_admin_sp" {
  scope                = azurerm_key_vault.kv.id
  role_definition_id = data.azurerm_role_definition.kv_admin.role_definition_id
  principal_id         = data.azurerm_client_config.client.object_id
  principal_type       = "ServicePrincipal"
}

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

