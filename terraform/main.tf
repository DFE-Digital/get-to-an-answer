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

########################
# Variables
########################

variable "environment" {
  type        = string
  description = "Environment name (dev|test|prod)"
  default     = "dev"
}

variable "location" {
  type        = string
  description = "Azure region"
  default     = "uksouth"
}

variable "project_name" {
  type        = string
  description = "Short system name used for resource naming"
  default     = "dfe-webapp"
}

variable "tags" {
  type        = map(string)
  description = "Standard DfE tags"
  default = {
    Owner            = "Platform"
    DataClass        = "Official"
    BusinessUnit     = "DfE"
    Criticality      = "Medium"
    CostCentre       = "TBC"
    SupportContact   = "platform@example.gov.uk"
    Sensitivity      = "Official"
    Environment      = "dev"
  }
}

variable "api_image" {
  type        = string
  description = "Container image for API (ACR path)"
}

variable "frontend_image" {
  type        = string
  description = "Container image for Front-End (ACR path)"
}

variable "admin_image" {
  type        = string
  description = "Container image for Admin (ACR path)"
}

variable "db_name" {
  type        = string
  default     = "appdb"
}

########################
# Resource Group
########################

resource "azurerm_resource_group" "rg" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.location
  tags     = var.tags
}

########################
# Hub-style networking for Private Endpoints
########################

resource "azurerm_virtual_network" "vnet" {
  name                = "${var.project_name}-${var.environment}-vnet"
  address_space       = ["10.60.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  tags                = var.tags
}

resource "azurerm_subnet" "snet_pe" {
  name                 = "snet-private-endpoints"
  resource_group_name  = azurerm_resource_group.rg.name
  virtual_network_name = azurerm_virtual_network.vnet.name
  address_prefixes     = ["10.60.10.0/24"]

  enforce_private_link_endpoint_network_policies = true
}

########################
# Log Analytics + Monitor DCR
########################

resource "azurerm_log_analytics_workspace" "law" {
  name                = "${var.project_name}-${var.environment}-law"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_monitor_data_collection_rule" "dcr" {
  name                = "${var.project_name}-${var.environment}-dcr"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  data_flow {
    streams      = ["Microsoft-ContainerLog", "Microsoft-InsightsMetrics"]
    destinations = ["law-dest"]
  }

  destinations {
    log_analytics {
      name                  = "law-dest"
      workspace_resource_id = azurerm_log_analytics_workspace.law.id
    }
  }

  tags = var.tags
}

########################
# Key Vault (RBAC) for secrets
########################

data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv" {
  name                        = replace("${var.project_name}-${var.environment}-kv", "/[-_]/", "")
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  sku_name                    = "standard"
  purge_protection_enabled    = true
  soft_delete_retention_days  = 90
  enable_rbac_authorization   = true
  public_network_access_enabled = true
  tags                        = var.tags
}

########################
# Azure Container Registry (private + MI pull)
########################

resource "azurerm_container_registry" "acr" {
  name                = replace("${var.project_name}${var.environment}acr", "/[-_]/", "")
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Standard"
  admin_enabled       = false
  tags                = var.tags
}

# Private DNS for ACR
resource "azurerm_private_dns_zone" "acr_zone" {
  name                = "privatelink.azurecr.io"
  resource_group_name = azurerm_resource_group.rg.name
  tags                = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "acr_link" {
  name                  = "acr-dns-link"
  resource_group_name   = azurerm_resource_group.rg.name
  private_dns_zone_name = azurerm_private_dns_zone.acr_zone.name
  virtual_network_id    = azurerm_virtual_network.vnet.id
  registration_enabled  = false
}

resource "azurerm_private_endpoint" "acr_pe" {
  name                = "${var.project_name}-${var.environment}-acr-pe"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  subnet_id           = azurerm_subnet.snet_pe.id
  tags                = var.tags

  private_service_connection {
    name                           = "acr-psc"
    private_connection_resource_id = azurerm_container_registry.acr.id
    is_manual_connection           = false
    subresource_names              = ["registry"]
  }

  private_dns_zone_group {
    name                 = "acr-dns"
    private_dns_zone_ids = [azurerm_private_dns_zone.acr_zone.id]
  }
}

########################
# Azure SQL (private)
########################

resource "random_string" "sqlsuffix" {
  length  = 6
  upper   = false
  lower   = true
  numeric = true
  special = false
}

resource "azurerm_mssql_server" "sql" {
  name                         = "${var.project_name}-${var.environment}-sql-${random_string.sqlsuffix.result}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  minimum_tls_version          = "1.2"
  administrator_login          = "sqladminuser"
  administrator_login_password = random_password.sql_admin.result
  public_network_access_enabled = false
  tags                         = var.tags
}

resource "random_password" "sql_admin" {
  length  = 24
  special = true
}

resource "azurerm_mssql_database" "db" {
  name           = var.db_name
  server_id      = azurerm_mssql_server.sql.id
  sku_name       = "S0"
  zone_redundant = false
  max_size_gb    = 10
  tags           = var.tags
}

# Private DNS + Endpoint for SQL
resource "azurerm_private_dns_zone" "sql_zone" {
  name                = "privatelink.database.windows.net"
  resource_group_name = azurerm_resource_group.rg.name
  tags                = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "sql_link" {
  name                  = "sql-dns-link"
  resource_group_name   = azurerm_resource_group.rg.name
  private_dns_zone_name = azurerm_private_dns_zone.sql_zone.name
  virtual_network_id    = azurerm_virtual_network.vnet.id
  registration_enabled  = false
}

resource "azurerm_private_endpoint" "sql_pe" {
  name                = "${var.project_name}-${var.environment}-sql-pe"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  subnet_id           = azurerm_subnet.snet_pe.id
  tags                = var.tags

  private_service_connection {
    name                           = "sql-psc"
    private_connection_resource_id = azurerm_mssql_server.sql.id
    is_manual_connection           = false
    subresource_names              = ["sqlServer"]
  }

  private_dns_zone_group {
    name                 = "sql-dns"
    private_dns_zone_ids = [azurerm_private_dns_zone.sql_zone.id]
  }
}

########################
# Container Apps Environment (workload identities)
########################

resource "azurerm_log_analytics_solution" "containerinsights" {
  solution_name         = "ContainerInsights"
  location              = azurerm_log_analytics_workspace.law.location
  resource_group_name   = azurerm_resource_group.rg.name
  workspace_resource_id = azurerm_log_analytics_workspace.law.id
  workspace_name        = azurerm_log_analytics_workspace.law.name

  plan {
    publisher = "Microsoft"
    product   = "OMSGallery/ContainerInsights"
  }
}

resource "azurerm_container_app_environment" "cae" {
  name                       = "${var.project_name}-${var.environment}-cae"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.law.id
  infrastructure_subnet_id   = null
  tags                       = var.tags
}

########################
# Identities + ACR RBAC
########################

resource "azurerm_user_assigned_identity" "workload" {
  name                = "${var.project_name}-${var.environment}-uami"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  tags                = var.tags
}

# Allow MI to pull images from ACR
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.workload.principal_id
}

########################
# Secrets in Key Vault
########################

resource "azurerm_key_vault_secret" "sql_conn" {
  name         = "sql-connection-string"
  value        = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.sql.administrator_login};Password=${random_password.sql_admin.result};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  key_vault_id = azurerm_key_vault.kv.id
}

########################
# Container Apps (no public ingress)
########################

locals {
  cpu    = 0.5
  memory = "1Gi"
}

resource "azurerm_container_app" "api" {
  name                         = "${var.project_name}-${var.environment}-api"
  resource_group_name          = azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.cae.id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.workload.id]
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    transport        = "auto"
  }

  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.workload.id
  }

  secret {
    name  = "sql-conn"
    value = azurerm_key_vault_secret.sql_conn.value
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
    }

    scale {
      min_replicas = 1
      max_replicas = 3
      rules {
        name = "http"
        custom {
          type = "http"
          metadata = {
            concurrentRequests = "50"
          }
        }
      }
    }
  }

  tags = var.tags
}

resource "azurerm_container_app" "frontend" {
  name                         = "${var.project_name}-${var.environment}-fe"
  resource_group_name          = azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.cae.id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.workload.id]
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    transport        = "auto"
  }

  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.workload.id
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
    }

    scale {
      min_replicas = 1
      max_replicas = 3
    }
  }

  tags = var.tags
}

resource "azurerm_container_app" "admin" {
  name                         = "${var.project_name}-${var.environment}-admin"
  resource_group_name          = azurerm_resource_group.rg.name
  container_app_environment_id = azurerm_container_app_environment.cae.id
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.workload.id]
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    transport        = "auto"
  }

  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.workload.id
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
    }

    scale {
      min_replicas = 1
      max_replicas = 2
    }
  }

  tags = var.tags
}

########################
# Front Door Standard/Premium + WAF
########################

resource "azurerm_cdn_frontdoor_profile" "fdp" {
  name                = "${var.project_name}-${var.environment}-fdp"
  resource_group_name = azurerm_resource_group.rg.name
  sku_name            = "Standard_AzureFrontDoor"
  tags                = var.tags
}

# Origin group for Container Apps (using their FQDNs)
resource "azurerm_cdn_frontdoor_origin_group" "og_fe" {
  name                     = "og-fe"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fdp.id
  session_affinity_enabled = false
  health_probe {
    probe_method = "GET"
    probe_protocol = "Https"
    probe_path   = "/"
    interval_in_seconds = 30
  }
}

resource "azurerm_cdn_frontdoor_origin" "origin_fe" {
  name                          = "fe-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.og_fe.id
  enabled                       = true
  host_name                     = azurerm_container_app.frontend.latest_revision_fqdn
  http_port                     = 80
  https_port                    = 443
  origin_host_header            = azurerm_container_app.frontend.latest_revision_fqdn
  priority                      = 1
  weight                        = 100
}

resource "azurerm_cdn_frontdoor_origin_group" "og_admin" {
  name                     = "og-admin"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fdp.id
  session_affinity_enabled = false
  health_probe {
    probe_method = "GET"
    probe_protocol = "Https"
    probe_path   = "/"
    interval_in_seconds = 30
  }
}

resource "azurerm_cdn_frontdoor_origin" "origin_admin" {
  name                          = "admin-origin"
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.og_admin.id
  enabled                       = true
  host_name                     = azurerm_container_app.admin.latest_revision_fqdn
  http_port                     = 80
  https_port                    = 443
  origin_host_header            = azurerm_container_app.admin.latest_revision_fqdn
  priority                      = 1
  weight                        = 100
}

resource "azurerm_cdn_frontdoor_endpoint" "fde" {
  name                     = "${var.project_name}-${var.environment}-fde"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fdp.id
  tags                     = var.tags
}

resource "azurerm_cdn_frontdoor_route" "route_fe" {
  name                          = "route-fe"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.fde.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.og_fe.id
  supported_protocols           = ["Https"]
  patterns_to_match             = ["/*"]
  https_redirect_enabled        = true
  link_to_default_domain        = true
  origin_path                   = "/"
}

resource "azurerm_cdn_frontdoor_route" "route_admin" {
  name                          = "route-admin"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.fde.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.og_admin.id
  supported_protocols           = ["Https"]
  patterns_to_match             = ["/admin/*"]
  https_redirect_enabled        = true
  link_to_default_domain        = false
  origin_path                   = "/"
}

# WAF Policy with managed rules (OWASP 3.2)
resource "azurerm_cdn_frontdoor_firewall_policy" "waf" {
  name                = "${var.project_name}-${var.environment}-waf"
  resource_group_name = azurerm_resource_group.rg.name
  sku_name            = "Standard_AzureFrontDoor"
  mode                = "Prevention"

  managed_rule {
    type    = "DefaultRuleSet"
    version = "2.1"
  }

  custom_rule {
    name     = "Block-Non-HTTPS"
    action   = "Block"
    priority = 1
    rule_type = "MatchRule"
    match_condition {
      match_variable     = "RequestScheme"
      operator           = "Equal"
      negate_condition   = false
      match_values       = ["HTTP"]
      transforms         = []
    }
  }

  tags = var.tags
}

resource "azurerm_cdn_frontdoor_security_policy" "fdsp" {
  name                               = "${var.project_name}-${var.environment}-sec"
  cdn_frontdoor_profile_id           = azurerm_cdn_frontdoor_profile.fdp.id
  security_policies {
    firewall {
      association {
        domains = [azurerm_cdn_frontdoor_endpoint.fde.host_name]
        patterns_to_match = ["/*"]
      }
      waf_policy_id = azurerm_cdn_frontdoor_firewall_policy.waf.id
    }
  }
}

########################
# Governance: Azure Policy samples
########################

data "azurerm_subscription" "current" {}

# Built-in: Require TLS 1.2 for SQL servers
resource "azurerm_policy_assignment" "sql_tls" {
  name                 = "${var.project_name}-${var.environment}-sqltls"
  scope                = azurerm_resource_group.rg.id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/1a5b4d8b-31b9-4d7b-9a2a-ec0e6d6d2a8d"
  display_name         = "Enforce TLS 1.2 for SQL"
  enforcement_mode     = true
}

# Built-in: Enforce diagnostic settings to LAW for SQL + ACR (example via initiative would be better)
resource "azurerm_policy_assignment" "send_diagnostics" {
  name                 = "${var.project_name}-${var.environment}-diag"
  scope                = azurerm_resource_group.rg.id
  policy_definition_id = "/providers/Microsoft.Authorization/policyDefinitions/0c5b57c9-5a8d-4b63-8b8b-7e5caa82b7e1"
  display_name         = "Deploy Diagnostic Settings to Log Analytics"
  parameters = jsonencode({
    logAnalytics = {
      value = azurerm_log_analytics_workspace.law.id
    }
  })
  enforcement_mode = true
}

########################
# Outputs
########################

output "resource_group" {
  value = azurerm_resource_group.rg.name
}

output "frontdoor_default_domain" {
  value = azurerm_cdn_frontdoor_endpoint.fde.host_name
}

output "sql_server_private_fqdn" {
  value = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}