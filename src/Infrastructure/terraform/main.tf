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

# Resource Group
resource "azurerm_resource_group" "gettoananswer-rg" {
  name     = "${var.prefix}rg-uks-gtaa"
  location = var.location

  tags = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_log_analytics_workspace" "log-analytics-workspace" {
  name                = "${var.prefix}lga-uks-log-analytics-workspace"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  retention_in_days   = 180
  tags                = {
    Environment = var.env
    Product     = var.product
  }
}

resource "azurerm_application_insights" "application-insights" {
  name                = "${var.prefix}ais-uks-app-insights"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.log-analytics-workspace.id
  tags                = {
    Environment = var.env
    Product     = var.product
  }
}

# App Service Plan (Web)
resource "azurerm_service_plan" "gettoananswer-web-asp" {
  name                = "${var.prefix}asp-uks-web"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  os_type             = "Linux"
  sku_name            = "B1"

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

# Linux Web App - API
resource "azurerm_linux_web_app" "gettoananswer-api" {
  name                = "${var.prefix}app-uks-api"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    application_stack {
      docker_image_name        = var.api_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    # Enforce HTTPS only
    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    ApplicationInsights__ConnectionString = azurerm_application_insights.application-insights.connection_string
    ConnectionStrings__DefaultConnection = "Server=tcp:${azurerm_mssql_server.gettoananswer_mssql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.gettoananswer_mssql_db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.gettoananswer_mssql_server.administrator_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    SQLSERVER_SA_PASSWORD = var.sql_admin_password
    AzureAd__Domain = "Educationgovuk.onmicrosoft.com"
    AzureAd__TenantId = var.ad_tenant_id
    AzureAd__ClientId = var.ad_client_id
    AzureAd__Audience = "api://${var.prefix}gettoananswer"
    AzureAd__ClientSecret = var.ad_client_secret
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
}

# Linux Web App - Admin
resource "azurerm_linux_web_app" "gettoananswer-admin" {
  name                = "${var.prefix}app-uks-admin"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    application_stack {
      docker_image_name        = var.admin_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags, app_settings, sticky_settings]
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    AppSettings__BaseUrl                = "https://${azurerm_linux_web_app.gettoananswer-api.default_hostname}"
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp, azurerm_linux_web_app.gettoananswer-api]
}

# Linux Web App - Frontend
resource "azurerm_linux_web_app" "gettoananswer-frontend" {
  name                = "${var.prefix}app-uks-frontend"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    application_stack {
      docker_image_name        = var.frontend_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags, app_settings, sticky_settings]
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
    AppSettings__BaseUrl                = "https://${azurerm_linux_web_app.gettoananswer-api.default_hostname}"
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp, azurerm_linux_web_app.gettoananswer-api]
}