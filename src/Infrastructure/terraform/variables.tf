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