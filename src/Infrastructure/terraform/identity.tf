resource "azurerm_user_assigned_identity" "gtaa-identity" {
  name                = "${var.prefix}mid-uks-gtaa"
  location            = var.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name

  tags = local.common_tags
}