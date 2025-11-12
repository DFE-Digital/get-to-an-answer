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
    docker_image_name        = var.admin_image_name
    docker_registry_url      = "https://${azurerm_container_registry.gettoananswer-registry.login_server}"
    docker_registry_username = azurerm_container_registry.gettoananswer-registry.admin_username
    docker_registry_password = azurerm_container_registry.gettoananswer-registry.admin_password
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

    ip_restriction_default_action = "Deny"

    ip_restriction {
      name        = "Access from Front Door"
      service_tag = "AzureFrontDoor.Backend"
    }

    application_stack = app_req_config

    # Enforce HTTPS only
    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.api_app_settings

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp]
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

    application_stack = app_req_config

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  # Slot-specific settings to avoid leaking prod secrets
  app_settings = local.api_app_settings
}

# Linux Web App - Admin
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

    application_stack = app_req_config

    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.admin_app_settings

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp, azurerm_linux_web_app.gettoananswer-api]
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

    application_stack = app_req_config

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
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

    application_stack = app_req_config

    minimum_tls_version = "1.2"

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  lifecycle {
    ignore_changes = [tags]
  }

  app_settings = local.frontend_app_settings

  https_only = true
  depends_on = [azurerm_service_plan.gettoananswer-web-asp, azurerm_linux_web_app.gettoananswer-api]
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

    application_stack = app_req_config

    minimum_tls_version               = "1.2"
    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  app_settings = local.frontend_app_settings
}