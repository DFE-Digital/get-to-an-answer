terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.114"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }
}

provider "azurerm" {
  features {}
}

# Add Microsoft Graph provider for Entra ID app registration
provider "msgraph" {
  tenant_id = null # Uses az cli / env by default; set explicitly if needed
}

########################
# Variables
########################

variable "project_name" {
  type        = string
  description = "Base name for all resources."
  default     = "azure-webapp-arch"
}

variable "location" {
  type        = string
  description = "Azure region."
  default     = "westeurope"
}

# Optional: custom AAD display name
variable "aad_app_display_name" {
  type        = string
  description = "Display name for Azure AD application registration."
  default     = "WebAppArchitecture"
}

# Optional: allowed redirect origins (comma-separated) for frontend/admin (can be updated post-deploy)
variable "allowed_redirect_hosts" {
  type        = list(string)
  description = "Allowed HTTPS hosts to be used for OIDC redirects (e.g., frontend/admin FQDNs)."
  default     = []
}

variable "api_image" {
  type        = string
  description = "Container image for the API service (e.g., myregistry.azurecr.io/api:latest)."
}

variable "frontend_image" {
  type        = string
  description = "Container image for the Front-End web app."
}

variable "admin_image" {
  type        = string
  description = "Container image for the Admin web app."
}

variable "sql_admin_login" {
  type        = string
  description = "SQL Server admin login."
}

variable "sql_admin_password" {
  type        = string
  description = "SQL Server admin password."
  sensitive   = true
}

# If you plan to use ACR with admin-enabled credentials
variable "enable_acr_admin" {
  type        = bool
  description = "Enable ACR admin user (simple auth for demos). Prefer managed identity in production."
  default     = true
}

# Optional: database name override
variable "db_name" {
  type        = string
  description = "Name of the SQL database."
  default     = "appdb"
}

resource "azurerm_container_app_environment" "cae" {
  name                = "${var.project_name}-cae"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
}

########################
# Azure AD (Entra ID) App Registration + Secret (via Microsoft Graph)
########################

# Application registration
resource "msgraph_application" "app" {
  display_name = var.aad_app_display_name

  web {
    # Redirect URIs can be updated after first deploy if hosts aren’t known yet
    redirect_uris = [
      for h in var.allowed_redirect_hosts : "https://${h}/signin-oidc"
    ]
    logout_url = length(var.allowed_redirect_hosts) > 0 ? "https://${var.allowed_redirect_hosts[0]}/signout-oidc" : null
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = true
    }
  }

  api {
    requested_access_token_version = 2
  }

  sign_in_audience = "AzureADMyOrg"
}

# Service principal (enterprise application)
resource "msgraph_service_principal" "sp" {
  application_id = msgraph_application.app.application_id
}

# Client secret (ensure lifecycle so it's not needlessly recreated)
resource "msgraph_application_password" "client_secret" {
  application_id = msgraph_application.app.id
  display_name   = "terraform-client-secret"
  end_date_relative = "87600h" # ~10 years; reduce for production policies
  # lifecycle ignore optional
  lifecycle {
    ignore_changes = [
      end_date_time
    ]
  }
}

########################
# Resource Group
########################

resource "azurerm_resource_group" "rg" {
  name     = "${var.project_name}-rg"
  location = var.location
}

########################
# Log Analytics + Container Apps Environment
########################

resource "azurerm_log_analytics_workspace" "law" {
  name                = "${var.project_name}-law"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_container_app_environment" "cae" {
  name                = "${var.project_name}-cae"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
}

########################
# Azure Container Registry
########################

resource "azurerm_container_registry" "acr" {
  name                = replace("${var.project_name}acr", "/[-_]/", "")
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Basic"
  admin_enabled       = var.enable_acr_admin
}

########################
# Azure SQL Server + Database
########################

resource "random_string" "sqlsuffix" {
  length  = 6
  upper   = false
  lower   = true
  numeric = true
  special = false
}

resource "azurerm_mssql_server" "sql" {
  name                         = "${var.project_name}-sql-${random_string.sqlsuffix.result}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"
}

resource "azurerm_mssql_database" "db" {
  name            = var.db_name
  server_id       = azurerm_mssql_server.sql.id
  sku_name        = "S0"
  zone_redundant  = false
  collation       = "SQL_Latin1_General_CP1_CI_AS"
  max_size_gb     = 10
}

# Allow Azure services to connect (frontend/admin/api via public network). Consider private endpoints for production.
resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Connection string (basic). Prefer Key Vault in production.
locals {
  sql_connection_string = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}

########################
# Container Apps: API, Front-End, Admin
########################

# Common CPU/memory
locals {
  cpu    = 0.5
  memory = "1.0Gi"
}

# Common AAD secrets/vars to inject into all apps
locals {
  aad_tenant_id     = data.azurerm_client_config.current.tenant_id
  aad_client_id     = msgraph_application.app.application_id
  aad_client_secret = msgraph_application_password.client_secret.value
  aad_instance      = "https://login.microsoftonline.com/"
  aad_audience      = msgraph_application.app.application_id
}

data "azurerm_client_config" "current" {}

# API Service
resource "azurerm_container_app" "api" {
  name                         = "${var.project_name}-api"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
    transport = "auto"
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = var.enable_acr_admin ? azurerm_container_registry.acr.admin_username : null
    password_secret_name = var.enable_acr_admin ? "acr-password" : null
  }

  secret {
    name  = "acr-password"
    value = var.enable_acr_admin ? azurerm_container_registry.acr.admin_password : null
  }

  # Existing DB secret
  secret {
    name  = "sql-conn"
    value = local.sql_connection_string
  }

  template {
    container {
      name   = "api"
      image  = var.api_image
      cpu    = local.cpu
      memory = local.memory

      env {
        name        = "ConnectionStrings__Default"
        secret_name = "sql-conn"
      }
      
      # AAD config exposed to API
      env { name = "AzureAd__Instance", value       = local.aad_instance }
      env { name = "AzureAd__Domain",    value       = "" } # optional, if you need it
      env { name = "AzureAd__TenantId",  value       = local.aad_tenant_id }
      env { name = "AzureAd__ClientId",  value       = local.aad_client_id }
      env { name = "AzureAd__Audience",  value       = local.aad_audience }
      env { name = "AzureAd__ClientSecret", secret_name = "aad-client-secret" }
    }

    scale {
      min_replicas = 1
      max_replicas = 3
      rules {
        name = "http-scaling"
        custom {
          type = "http"
          metadata = {
            concurrentRequests = "50"
          }
        }
      }
    }
  }
}

# Front-End Web App
resource "azurerm_container_app" "frontend" {
  name                         = "${var.project_name}-frontend"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
    transport = "auto"
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = var.enable_acr_admin ? azurerm_container_registry.acr.admin_username : null
    password_secret_name = var.enable_acr_admin ? "acr-password" : null
  }

  secret {
    name  = "acr-password"
    value = var.enable_acr_admin ? azurerm_container_registry.acr.admin_password : null
  }

  template {
    container {
      name   = "frontend"
      image  = var.frontend_image
      cpu    = local.cpu
      memory = local.memory

      env {
        name  = "API_BASE_URL"
        value = "https://${azurerm_container_app.api.latest_revision_fqdn}"
      }

      # AAD config exposed to Front-End
      env { name = "AzureAd__Instance",  value       = local.aad_instance }
      env { name = "AzureAd__Domain",    value       = "" }
      env { name = "AzureAd__TenantId",  value       = local.aad_tenant_id }
      env { name = "AzureAd__ClientId",  value       = local.aad_client_id }
      env { name = "AzureAd__Audience",  value       = local.aad_audience }
      env { name = "AzureAd__ClientSecret", secret_name = "aad-client-secret" }
      # For MVC callback
      env { name = "ASPNETCORE_URLS", value = "http://+:80" }
    }

    scale {
      min_replicas = 1
      max_replicas = 3
      rules {
        name = "http-scaling"
        custom {
          type = "http"
          metadata = {
            concurrentRequests = "50"
          }
        }
      }
    }
  }
}

# Admin Web App
resource "azurerm_container_app" "admin" {
  name                         = "${var.project_name}-admin"
  container_app_environment_id = azurerm_container_app_environment.cae.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  ingress {
    external_enabled = true
    target_port      = 80
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
    transport = "auto"
  }

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = var.enable_acr_admin ? azurerm_container_registry.acr.admin_username : null
    password_secret_name = var.enable_acr_admin ? "acr-password" : null
  }

  secret {
    name  = "acr-password"
    value = var.enable_acr_admin ? azurerm_container_registry.acr.admin_password : null
  }

  template {
    container {
      name   = "admin"
      image  = var.admin_image
      cpu    = local.cpu
      memory = local.memory

      env {
        name  = "API_BASE_URL"
        value = "https://${azurerm_container_app.api.latest_revision_fqdn}"
      }

      # AAD config exposed to Admin
      env { name = "AzureAd__Instance",  value       = local.aad_instance }
      env { name = "AzureAd__Domain",    value       = "" }
      env { name = "AzureAd__TenantId",  value       = local.aad_tenant_id }
      env { name = "AzureAd__ClientId",  value       = local.aad_client_id }
      env { name = "AzureAd__Audience",  value       = local.aad_audience }
      env { name = "AzureAd__ClientSecret", secret_name = "aad-client-secret" }
      env { name = "ASPNETCORE_URLS", value = "http://+:80" }
    }

    scale {
      min_replicas = 1
      max_replicas = 2
      rules {
        name = "http-scaling"
        custom {
          type = "http"
          metadata = {
            concurrentRequests = "30"
          }
        }
      }
    }
  }
}

########################
# Azure Front Door (Classic)
########################

resource "azurerm_frontdoor" "afd" {
  name                = "${var.project_name}-afd"
  resource_group_name = azurerm_resource_group.rg.name

  routing_rule {
    name               = "route-frontend"
    accepted_protocols = ["Http", "Https"]
    patterns_to_match  = ["/*"]
    frontend_endpoints = ["defaultEndpoint"]
    forwarding_configuration {
      forwarding_protocol = "HttpsOnly"
      backend_pool_name   = "frontendPool"
    }
  }

  routing_rule {
    name               = "route-admin"
    accepted_protocols = ["Http", "Https"]
    patterns_to_match  = ["/admin/*"]
    frontend_endpoints = ["defaultEndpoint"]
    forwarding_configuration {
      forwarding_protocol = "HttpsOnly"
      backend_pool_name   = "adminPool"
    }
  }

  backend_pool_load_balancing {
    name = "lb"
  }

  backend_pool_health_probe {
    name                = "probe"
    path                = "/"
    protocol            = "Https"
    interval_in_seconds = 30
  }

  backend_pool {
    name = "frontendPool"
    backend {
      host_header = azurerm_container_app.frontend.latest_revision_fqdn
      address     = azurerm_container_app.frontend.latest_revision_fqdn
      http_port   = 80
      https_port  = 443
      priority    = 1
      weight      = 50
    }
    load_balancing_name = "lb"
    health_probe_name   = "probe"
  }

  backend_pool {
    name = "adminPool"
    backend {
      host_header = azurerm_container_app.admin.latest_revision_fqdn
      address     = azurerm_container_app.admin.latest_revision_fqdn
      http_port   = 80
      https_port  = 443
      priority    = 1
      weight      = 50
    }
    load_balancing_name = "lb"
    health_probe_name   = "probe"
  }

  frontend_endpoint {
    name                              = "defaultEndpoint"
    host_name                         = "${var.project_name}-afd.azurefd.net"
    session_affinity_enabled          = false
    web_application_firewall_policy_link_id = null
  }
}

########################
# Outputs
########################

output "resource_group" {
  value = azurerm_resource_group.rg.name
}

output "api_url" {
  value = "https://${azurerm_container_app.api.latest_revision_fqdn}"
}

output "frontend_url" {
  value = "https://${azurerm_container_app.frontend.latest_revision_fqdn}"
}

output "admin_url" {
  value = "https://${azurerm_container_app.admin.latest_revision_fqdn}"
}

output "frontdoor_url" {
  value = "https://${azurerm_frontdoor.afd.frontend_endpoint[0].host_name}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "aad_tenant_id" {
  value = local.aad_tenant_id
}

output "aad_client_id" {
  value = local.aad_client_id
}

output "aad_client_secret_note" {
  value     = "Client secret is stored as Container App secret 'aad-client-secret' for api/frontend/admin."
  sensitive = false
}
