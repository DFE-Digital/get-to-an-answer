locals {
  api_app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE   = "false"
    ApplicationInsights__ConnectionString = azurerm_application_insights.application-insights.connection_string
    ConnectionStrings__DefaultConnection  = "Server=tcp:${azurerm_mssql_server.gettoananswer_mssql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.gettoananswer_mssql_db.name};Persist Security Info=False;User ID=${azurerm_mssql_server.gettoananswer_mssql_server.administrator_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    SQLSERVER_SA_PASSWORD                 = var.sql_admin_password
    AzureAd__Domain                       = "Educationgovuk.onmicrosoft.com"
    AzureAd__TenantId                     = var.ad_tenant_id
    AzureAd__ClientId                     = var.ad_client_id
    AzureAd__Audience                     = "api://${var.ad_client_id}"
    AzureAd__ClientSecret                 = var.ad_client_secret
  }
  admin_app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE   = "false"
    ASPNETCORE_FORWARDEDHEADERS_ENABLED   = "true"
    ApplicationInsights__ConnectionString = azurerm_application_insights.application-insights.connection_string
    ApiSettings__BaseUrl                  = "https://${azurerm_linux_web_app.gettoananswer-api.default_hostname}"
    AzureAd__Domain                       = "Educationgovuk.onmicrosoft.com"
    AzureAd__TenantId                     = var.ad_tenant_id
    AzureAd__ClientId                     = var.ad_client_id
    AzureAd__ClientSecret                 = var.ad_client_secret
    AzureAd__CallbackPath                 = "/signin-oidc"
  }
  frontend_app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE   = "false"
    ASPNETCORE_FORWARDEDHEADERS_ENABLED   = "true"
    ApplicationInsights__ConnectionString = azurerm_application_insights.application-insights.connection_string
    ApiSettings__BaseUrl                  = "https://${azurerm_linux_web_app.gettoananswer-api.default_hostname}"
  }

  app_req_config = {
    docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
    docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
    docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
  }

  managed_identity = {
    type = "UserAssigned"
    identity_ids = [
      azurerm_user_assigned_identity.gtaa-identity.id
    ]
  }
}

# Linux Web App - API
resource "azurerm_linux_web_app" "gettoananswer-api" {
  name                      = "${var.prefix}app-uks-api"
  location                  = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name       = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id           = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    always_on = true

    # With Private Endpoint, keep public locked down
    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack {
      docker_image_name        = var.api_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    # Enforce HTTPS only
    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }
  
  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.api_app_settings

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
}
# Private DNS zone for Web Apps Private Link
resource "azurerm_private_dns_zone" "webapps_privatelink" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
}
# Link VNet to the Private DNS zone
resource "azurerm_private_dns_zone_virtual_network_link" "webapps_privatelink_vnet_link" {
  name                  = "${var.prefix}-webapps-privatelink-link"
  resource_group_name   = azurerm_resource_group.gettoananswer-rg.name
  private_dns_zone_name = azurerm_private_dns_zone.webapps_privatelink.name
  virtual_network_id    = azurerm_virtual_network.gettoananswer_vnet.id
  registration_enabled  = false
}
# Private Endpoint for API app
resource "azurerm_private_endpoint" "api_pe" {
  name                = "${var.prefix}-api-pe"
  location            = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  subnet_id           = azapi_resource.gettoananswer_api_pe_subnet.id

  private_service_connection {
    name                           = "${var.prefix}-api-psc"
    private_connection_resource_id = azurerm_linux_web_app.gettoananswer-api.id
    subresource_names              = ["sites"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "${var.prefix}-api-dnsgrp"
    private_dns_zone_ids = [azurerm_private_dns_zone.webapps_privatelink.id]
  }
}
# Staging slot for API (deploy here first; swap into production)
resource "azurerm_linux_web_app_slot" "gettoananswer-api-staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.gettoananswer-api.id

  https_only = true

  site_config {
    always_on = true

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack {
      docker_image_name        = var.api_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  # Slot-specific settings to avoid leaking prod secrets
  app_settings = local.api_app_settings
}

resource "azurerm_linux_web_app" "gettoananswer-admin" {
  name                      = "${var.prefix}app-uks-admin"
  location                  = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name       = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id           = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    always_on = true

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack {
      docker_image_name        = var.admin_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.admin_app_settings

  https_only = true
  depends_on = [
    azurerm_service_plan.gettoananswer-web-asp,
    azurerm_linux_web_app.gettoananswer-api,
    azurerm_private_endpoint.api_pe,
    azurerm_private_dns_zone_virtual_network_link.webapps_privatelink_vnet_link
  ]
}

# Staging slot for Admin
resource "azurerm_linux_web_app_slot" "gettoananswer-admin-staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.gettoananswer-admin.id

  https_only = true

  site_config {
    always_on = true

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    ip_restriction {
      name                      = "Allow from VNet Subnet"
      priority                  = 200
      action                    = "Allow"
      virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id
    }

    application_stack {
      docker_image_name        = var.admin_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  app_settings = local.admin_app_settings
}

# Linux Web App - Frontend
resource "azurerm_linux_web_app" "gettoananswer-frontend" {
  name                      = "${var.prefix}app-uks-frontend"
  location                  = azurerm_resource_group.gettoananswer-rg.location
  resource_group_name       = azurerm_resource_group.gettoananswer-rg.name
  service_plan_id           = azurerm_service_plan.gettoananswer-web-asp.id
  virtual_network_subnet_id = azapi_resource.gettoananswer_main_subnet.id

  site_config {
    always_on = true

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack {
      docker_image_name        = var.frontend_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.frontend_app_settings

  https_only = true
  depends_on = [
    azurerm_service_plan.gettoananswer-web-asp,
    azurerm_linux_web_app.gettoananswer-api,
    azurerm_private_endpoint.api_pe,
    azurerm_private_dns_zone_virtual_network_link.webapps_privatelink_vnet_link
  ]
}

# Staging slot for Frontend
resource "azurerm_linux_web_app_slot" "gettoananswer-frontend-staging" {
  name           = "staging"
  app_service_id = azurerm_linux_web_app.gettoananswer-frontend.id

  https_only = true

  site_config {
    always_on = true

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack {
      docker_image_name        = var.frontend_image_name
      docker_registry_url      = local.app_req_config.docker_registry_url
      docker_registry_username = local.app_req_config.docker_registry_username
      docker_registry_password = local.app_req_config.docker_registry_password
    }

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  identity {
    type = local.managed_identity.type
    identity_ids = local.managed_identity.identity_ids
  }

  app_settings = local.frontend_app_settings
}