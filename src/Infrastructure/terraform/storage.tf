resource "azurerm_storage_account" "sa" {
  name                     = "${var.prefix}bsa-uks-images"
  resource_group_name      = azurerm_resource_group.gettoananswer-rg
  location                 = var.location
  account_tier             = "Standard"
  # LRS is Locally Redundant Storage, good for development/testing
  account_replication_type = "LRS"
}

# 3. Create a Blob Container
resource "azurerm_storage_container" "container" {
  name                  = "${var.prefix}bsc-uks-images"
  storage_account_id    = azurerm_storage_account.sa.id
  container_access_type = "private"
}