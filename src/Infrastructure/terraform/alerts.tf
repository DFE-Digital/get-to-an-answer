
resource "azurerm_monitor_metric_alert" "availability-alert" {
  name                = "availability-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_application_insights.application-insights.id]
  description         = "Alert if availability is below ${var.alerting[var.prefix].thresholds.availability}%"
  severity            = 0
  frequency           = "PT1M"
  window_size         = "PT1H"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "availabilityResults/availabilityPercentage"
    aggregation      = "Average"
    operator         = "LessThan"
    threshold        = var.alerting[var.prefix].thresholds.availability
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}

# CPU for Container Apps Environment
resource "azurerm_monitor_metric_alert" "cpu_alert" {
  name                = "web-app-cpu-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_service_plan.gettoananswer-web-asp.id]
  description         = "Alert if CPU utilisation exceeds ${var.alerting[var.prefix].thresholds.cpu}% for more than 5 minutes"
  severity            = 2
  frequency           = "PT1M"
  window_size         = "PT5M"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "CpuPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = var.alerting[var.prefix].thresholds.cpu
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}

resource "azurerm_monitor_metric_alert" "memory_alert" {
  name                = "web-app-memory-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_service_plan.gettoananswer-web-asp.id]
  description         = "Alert if memory utilisation exceeds ${var.alerting[var.prefix].thresholds.memory}% for more than 5 minutes"
  severity            = 2
  frequency           = "PT1M"
  window_size         = "PT5M"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "Microsoft.Web/serverfarms"
    metric_name      = "MemoryPercentage"
    aggregation      = "Average"
    operator         = "GreaterThan"
    threshold        = var.alerting[var.prefix].thresholds.memory
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}

resource "azurerm_monitor_metric_alert" "api_app_error_alert" {
  name                = "api-app-error-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_linux_web_app.gettoananswer-api.id]
  description         = "Alert if HTTP 5xx error count exceeds ${var.alerting[var.prefix].thresholds.error}"
  severity            = 0
  frequency           = "PT1M"
  window_size         = "PT30M"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = var.alerting[var.prefix].thresholds.error
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}

resource "azurerm_monitor_metric_alert" "admin_app_error_alert" {
  name                = "admin-app-error-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_linux_web_app.gettoananswer-admin.id]
  description         = "Alert if HTTP 5xx error count exceeds ${var.alerting[var.prefix].thresholds.error}"
  severity            = 0
  frequency           = "PT1M"
  window_size         = "PT30M"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = var.alerting[var.prefix].thresholds.error
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}

resource "azurerm_monitor_metric_alert" "frontend_app_error_alert" {
  name                = "frontend-app-error-alert"
  resource_group_name = azurerm_resource_group.gettoananswer-rg.name
  scopes              = [azurerm_linux_web_app.gettoananswer-frontend.id]
  description         = "Alert if HTTP 5xx error count exceeds ${var.alerting[var.prefix].thresholds.error}"
  severity            = 0
  frequency           = "PT1M"
  window_size         = "PT30M"
  enabled             = var.alerting[var.prefix].alerts_enabled
  tags = {
    Environment = var.env
    Product     = var.product
  }

  criteria {
    metric_namespace = "Microsoft.Web/sites"
    metric_name      = "Http5xx"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = var.alerting[var.prefix].thresholds.error
  }

  action {
    action_group_id = azurerm_monitor_action_group.service-support-action.id
  }
}