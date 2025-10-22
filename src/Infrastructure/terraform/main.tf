terraform {
  required_version = ">= 1.13.3"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.45"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "2.4.0"
    }
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "gettoananswer-rg" {
  name     = "${var.prefix}-rg"
  location = var.location

  tags = {
    Environment = var.env
    Product     = var.product
  }
}

# Shared DfE WAF (Azure Front Door) - Public entry point
# resource "azurerm_cdn_frontdoor_profile" "shared_waf_profile" {
#   name                = "${var.prefix}-afd-prof"
#   resource_group_name = azurerm_resource_group.gettoananswer-rg.name
#   sku_name            = "Standard_AzureFrontDoor"
#   tags = {
#     Environment = var.env
#     Product     = var.product
#   }
# }

# resource "azurerm_cdn_frontdoor_endpoint" "shared_waf_endpoint" {
#   name                     = "${var.prefix}-afd-endpoint"
#   cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.shared_waf_profile.id
# }

# Origin group and origins for API, Admin and Frontend apps
# resource "azurerm_cdn_frontdoor_origin_group" "origin_group" {
#   name                     = "${var.prefix}-og"
#   cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.shared_waf_profile.id
#   load_balancing {}
# }

# resource "azurerm_cdn_frontdoor_origin" "api_origin" {
#   name                           = "${var.prefix}-api-origin"
#   cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   enabled                        = true
#   host_name                      = azurerm_linux_web_app.gettoananswer-api.default_hostname
#   http_port                      = 80
#   https_port                     = 443
#   origin_host_header             = azurerm_linux_web_app.gettoananswer-api.default_hostname
#   certificate_name_check_enabled = true
# }
# 
# resource "azurerm_cdn_frontdoor_origin" "admin_origin" {
#   name                           = "${var.prefix}-admin-origin"
#   cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   enabled                        = true
#   host_name                      = azurerm_linux_web_app.gettoananswer-admin.default_hostname
#   http_port                      = 80
#   https_port                     = 443
#   origin_host_header             = azurerm_linux_web_app.gettoananswer-admin.default_hostname
#   certificate_name_check_enabled = true
# }
# 
# resource "azurerm_cdn_frontdoor_origin" "frontend_origin" {
#   name                           = "${var.prefix}-frontend-origin"
#   cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   enabled                        = true
#   host_name                      = azurerm_linux_web_app.gettoananswer-frontend.default_hostname
#   http_port                      = 80
#   https_port                     = 443
#   origin_host_header             = azurerm_linux_web_app.gettoananswer-frontend.default_hostname
#   certificate_name_check_enabled = true
# }
# 
# # Route everything through the shared WAF to the respective origins
# resource "azurerm_cdn_frontdoor_route" "route_api" {
#   name                          = "${var.prefix}-route-api"
#   cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.shared_waf_endpoint.id
#   cdn_frontdoor_origin_ids      = []
#   cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   supported_protocols           = ["Http", "Https"]
#   patterns_to_match             = ["/api/*"]
#   https_redirect_enabled        = true
#   forwarding_protocol           = "HttpsOnly"
#   link_to_default_domain        = true
# }
# 
# resource "azurerm_cdn_frontdoor_route" "route_admin" {
#   name                          = "${var.prefix}-route-admin"
#   cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.shared_waf_endpoint.id
#   cdn_frontdoor_origin_ids      = []
#   cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   supported_protocols           = ["Http", "Https"]
#   patterns_to_match             = ["/admin/*"]
#   https_redirect_enabled        = true
#   forwarding_protocol           = "HttpsOnly"
#   link_to_default_domain        = true
# }
# 
# resource "azurerm_cdn_frontdoor_route" "route_frontend" {
#   name                          = "${var.prefix}-route-frontend"
#   cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.shared_waf_endpoint.id
#   cdn_frontdoor_origin_ids      = []
#   cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.origin_group.id
#   supported_protocols           = ["Http", "Https"]
#   patterns_to_match             = ["/*"]
#   https_redirect_enabled        = true
#   forwarding_protocol           = "HttpsOnly"
#   link_to_default_domain        = true
# }

# App Service Plan (Web)
resource "azurerm_service_plan" "gettoananswer-web-asp" {
  name                = "${var.prefix}-web-asp"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  os_type             = "Linux"
  sku_name            = "B1"
}

# Azure Container Registry
resource "azurerm_container_registry" "gettoananswer-registry" {
  name                = "${var.prefix}acr"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  location            = azurerm_resource_group.gettoananswer-rg.location
  sku                 = "Basic"
  admin_enabled       = true
}

# Linux Web App - API
resource "azurerm_linux_web_app" "gettoananswer-api" {
  name                = "${var.prefix}-app-api"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id

  site_config {
    application_stack {
      docker_image_name        = var.api_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    # Enforce HTTPS only
    minimum_tls_version = "1.2"
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
}

# Linux Web App - Admin
resource "azurerm_linux_web_app" "gettoananswer-admin" {
  name                = "${var.prefix}-app-admin"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id

  site_config {
    application_stack {
      docker_image_name        = var.admin_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    minimum_tls_version = "1.2"
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
}

# Linux Web App - Frontend
resource "azurerm_linux_web_app" "gettoananswer-frontend" {
  name                = "${var.prefix}-app-frontend"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id     = azurerm_service_plan.gettoananswer-web-asp.id

  site_config {
    application_stack {
      docker_image_name        = var.frontend_image_name
      docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
      docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
      docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
    }
    minimum_tls_version = "1.2"
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
  }

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
}