terraform {
  required_version = ">= 1.13.3"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.45"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "2.7.0"
    }
  }
  backend "azurerm" {}
}

provider "azapi" {}

provider "azurerm" {
  features {}
}

# resource "azurerm_resource_provider_registration" "reg_cs" {
#   name = "Microsoft.ContainerService"
# }

# Resource Group
resource "azurerm_resource_group" "gettoananswer-rg" {
  name     = "${var.prefix}rg-uks-gtaa"
  location = var.location

  tags = local.common_tags
}

resource "azurerm_log_analytics_workspace" "log-analytics-workspace" {
  name                = "${var.prefix}lga-uks-log-analytics-workspace"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  retention_in_days   = 180
  tags                = local.common_tags
}

resource "azurerm_application_insights" "application-insights" {
  name                = "${var.prefix}ais-uks-app-insights"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.log-analytics-workspace.id
  tags                = local.common_tags
}

# App Service Plan (Web)
resource "azurerm_service_plan" "gettoananswer-web-asp" {
  name                = "${var.prefix}asp-uks-web"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  os_type             = "Linux"
  sku_name            = "P0v3"

  lifecycle {
    ignore_changes = [tags]
  }
}

# Azure Container Registry
resource "azurerm_container_registry" "gettoananswer-registry" {
  name                = "${var.prefix}acruksgtaa"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  location            = azurerm_resource_group.gettoananswer-rg.location
  sku                 = "Basic"
  admin_enabled       = true

  lifecycle {
    ignore_changes = [tags]
  }
}