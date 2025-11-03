variable "env" {
  description = "Environment (dev, test, prod)"
  type        = string
}

variable "product" {
  description = "Name of the project"
  type        = string
  default     = "Find Education and Training"
}

variable "prefix" {
  description = "Prefix for resource names"
  type        = string
  default     = "s213"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "uksouth"
}

variable "api_image_name" {
  type        = string
  description = "The repository name and tag for the API container."
}

variable "admin_image_name" {
  type        = string
  description = "The repository name and tag for the admin container."
}

variable "frontend_image_name" {
  type        = string
  description = "The repository name and tag for the frontend container."
}

variable "sql_admin_username" {
  type        = string
  description = "The administrator username of the SQL logical server."
  default     = "azureadmin"
}

variable "sql_admin_password" {
  type        = string
  description = "The administrator password of the SQL logical server."
  sensitive   = true
  default     = null
}

variable "api_custom_domain" {
  type        = string
  description = "The public subdomain for the api service"
}

variable "admin_custom_domain" {
  type        = string
  description = "The public subdomain for the admin service"
}

variable "frontend_custom_domain" {
  type        = string
  description = "The public subdomain for the frontend service"
}

variable "azure_frontdoor_scale" {
  description = "Azure Front Door Scale"
  type        = string
  default     = "Standard"
}

variable "support_alert_email" {
  description = "Alert support email or group"
  type        = string
}

variable "alerting" {
  description = "Alerting configuration per environment"
  type = map(object({
    name                 = string
    alerts_enabled       = bool
    email_alerts_enabled = bool
    smart_alerts_enabled = bool
    thresholds = object({
      availability = number
      cpu          = number
      memory       = number
      error        = number
    })
  }))
  default = {
    s263d01 = {
      name                 = "Test"
      alerts_enabled       = false
      email_alerts_enabled = false
      smart_alerts_enabled = false
      thresholds = {
        availability = 90
        cpu          = 95
        memory       = 95
        error        = 5
      }
    }
    s263t01 = {
      name                 = "Staging"
      alerts_enabled       = true
      email_alerts_enabled = false
      smart_alerts_enabled = true
      thresholds = {
        availability = 99.9
        cpu          = 85
        memory       = 85
        error        = 1
      }
    }
    s263p01 = {
      name                 = "Production"
      alerts_enabled       = true
      email_alerts_enabled = true
      smart_alerts_enabled = true
      thresholds = {
        availability = 99.9
        cpu          = 85
        memory       = 85
        error        = 1
      }
    }
  }
}