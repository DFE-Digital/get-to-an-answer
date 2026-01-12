resource "azurerm_storage_account" "sa" {
  name                = "${var.prefix}imagestore"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  location            = var.location
  account_tier        = "Standard"
  # LRS is Locally Redundant Storage, good for development/testing
  account_replication_type = "LRS"
}

# Enable Microsoft Defender for Storage (includes malware scanning)
resource "azurerm_security_center_storage_defender" "sa_defender" {
  storage_account_id         = azurerm_storage_account.sa.id
  malware_scanning_on_upload_enabled = true
  malware_scanning_on_upload_cap_gb_per_month = 5000
  sensitive_data_discovery_enabled = true
}

# 3. Create a Blob Container
resource "azurerm_storage_container" "container" {
  name                  = "${var.prefix}images"
  storage_account_id    = azurerm_storage_account.sa.id
  container_access_type = "private"
}