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
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.4.0"
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

# Container Apps Environment
resource "azurerm_container_app_environment" "gettoananswer-cae" {
  name                       = "${var.prefix}cae-uks-gtaa"
  location                   = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name        = azurerm_resource_group.gettoananswer-rg.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.log-analytics-workspace.id
#  infrastructure_subnet_id   = azapi_resource.gettoananswer_main_subnet.id
  
  tags = {
    Environment = var.env
    Product     = var.product
  }
}

# resource "azapi_resource" "gettoananswer-cae" {
#   type = "Microsoft.App/managedEnvironments@2024-03-01"
#   name = "${var.prefix}cae-uks-gtaa"
#   parent_id = azurerm_resource_group.gettoananswer-rg.id
#   location = azurerm_resource_group.gettoananswer-rg.location
#   
#   body = {
#     properties = {
#       appLogsConfiguration = {
#         destination: "log-analytics",
#         logAnalyticsConfiguration = {
#           customerId = azurerm_log_analytics_workspace.log-analytics-workspace.workspace_id,
#           sharedKey = azurerm_log_analytics_workspace.log-analytics-workspace.primary_shared_key
#         }
#       },
#       vnetConfiguration = {
#         internal = true,
#         infrastructureSubnetId = azapi_resource.gettoananswer_main_subnet.id
#       }
#     }
#   }
#   
#   depends_on = [azurerm_log_analytics_workspace.log-analytics-workspace, azapi_resource.gettoananswer_main_subnet]
# }

# API Container App
resource "azurerm_container_app" "gettoananswer-api" {
  name                         = "${var.prefix}aca-uks-api"
  container_app_environment_id = azurerm_container_app_environment.gettoananswer-cae.id
  resource_group_name          = azurerm_resource_group.gettoananswer-rg.name
  revision_mode                = "Single"
  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
  registry {
    server               = azurerm_container_registry.gettoananswer-registry.login_server
    username             = azurerm_container_registry.gettoananswer-registry.admin_username
    password_secret_name = "acr-pwd"
  }
  secret {
    name  = "acr-pwd"
    value = azurerm_container_registry.gettoananswer-registry.admin_password
  }
  secret {
    name  = "aad-client-secret"
    value = var.ad_client_secret
  }
  template {
    container {
      name   = "api"
      image  = "${azurerm_container_registry.gettoananswer-registry.login_server}/${var.api_image_name}"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name = "ASPNETCORE_ENVIRONMENT"
        value = var.asp_env
      }
      env {
        name  = "ASPNETCORE_URLS"
        value = "http://0.0.0.0:8080"
      }
      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.application-insights.connection_string
      }
      env {
        name = "ConnectionStrings__DefaultConnection"
        value = "Server=tcp:${azurerm_mssql_server.gettoananswer_mssql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.gettoananswer_mssql_db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.gettoananswer_mssql_server.administrator_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
      }
      env {
        name  = "AzureAd__Domain"
        value = "Educationgovuk.onmicrosoft.com"
      }
      env {
        name  = "AzureAd__TenantId"
        value = var.ad_tenant_id
      }
      env {
        name  = "AzureAd__ClientId"
        value = var.ad_client_id
      }
      env {
        name  = "AzureAd__Audience"
        value = "api://${var.prefix}gettoananswer"
      }
      env {
        name  = "AzureAd__ClientSecret"
        value = "aad-client-secret"
      }
    }
  }
  depends_on = [azurerm_container_registry.gettoananswer-registry]
}

# Admin Container App
resource "azurerm_container_app" "gettoananswer-admin" {
  name                         = "${var.prefix}aca-uks-admin"
  container_app_environment_id = azurerm_container_app_environment.gettoananswer-cae.id
  resource_group_name          = azurerm_resource_group.gettoananswer-rg.name
  revision_mode                = "Single"
  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
  registry {
    server               = azurerm_container_registry.gettoananswer-registry.login_server
    username             = azurerm_container_registry.gettoananswer-registry.admin_username
    password_secret_name = "acr-pwd"
  }
  secret {
    name  = "acr-pwd"
    value = azurerm_container_registry.gettoananswer-registry.admin_password
  }
  secret {
    name  = "aad-client-secret"
    value = var.ad_client_secret
  }
  template {
    container {
      name   = "admin"
      image  = "${azurerm_container_registry.gettoananswer-registry.login_server}/${var.admin_image_name}"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name = "ASPNETCORE_ENVIRONMENT"
        value = var.asp_env
      }
      env {
        name  = "ASPNETCORE_URLS"
        value = "http://0.0.0.0:8080"
      }
      env {
        name  = "AppSettings__BaseUrl"
        value = azurerm_container_app.gettoananswer-api.latest_revision_fqdn
      }
      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.application-insights.connection_string
      }
      env {
        name  = "AzureAd__Domain"
        value = "Educationgovuk.onmicrosoft.com"
      }
      env {
        name  = "AzureAd__TenantId"
        value = var.ad_tenant_id
      }
      env {
        name  = "AzureAd__ClientId"
        value = var.ad_client_id
      }
      env {
        name  = "AzureAd__Audience"
        value = "api://${var.prefix}gettoananswer"
      }
      env {
        name  = "AzureAd__ClientSecret"
        value = "aad-client-secret"
      }
    }
  }
  depends_on = [azurerm_container_app.gettoananswer-api, ]
}

# Frontend Container App
resource "azurerm_container_app" "gettoananswer-frontend" {
  name                         = "${var.prefix}aca-uks-frontend"
  container_app_environment_id = azurerm_container_app_environment.gettoananswer-cae.id
  resource_group_name          = azurerm_resource_group.gettoananswer-rg.name
  revision_mode                = "Single"
  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "auto"
    traffic_weight {
      percentage = 100
      latest_revision = true
    }
  }
  registry {
    server               = azurerm_container_registry.gettoananswer-registry.login_server
    username             = azurerm_container_registry.gettoananswer-registry.admin_username
    password_secret_name = "acr-pwd"
  }
  secret {
    name  = "acr-pwd"
    value = azurerm_container_registry.gettoananswer-registry.admin_password
  }
  template {
    container {
      name   = "frontend"
      image  = "${azurerm_container_registry.gettoananswer-registry.login_server}/${var.frontend_image_name}"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name = "ASPNETCORE_ENVIRONMENT"
        value = var.asp_env
      }
      env {
        name  = "ASPNETCORE_URLS"
        value = "http://0.0.0.0:8080"
      }
      env {
        name  = "AppSettings__BaseUrl"
        value = azurerm_container_app.gettoananswer-api.latest_revision_fqdn
      }
      env {
        name  = "ApplicationInsights__ConnectionString"
        value = azurerm_application_insights.application-insights.connection_string
      }
    }
  }
  depends_on = [azurerm_container_app.gettoananswer-api]
}