# Azure Subscription in the new Enterprise Landing Zone (ELZ)

## Overview
This document details our process being one of the first to deploy to the new ELZ.

## DfE Azure

All services are provisioned via Terraform, possibly even the DevOps instrumentation, so others can repeat the process.

To do this, we have been configuring the deployment accordingly.

## Infrastructure as Code Changes

These changes exist in the `src/Infrastructure/terraform` folder of the root project.

Currently this is set to:

```terraform
locals {
  common_tags = {
    "Environment"      = var.environment
    "Product"          = "Get-To-An-Answer"
    "Service"          = "Teacher Training and Qualifications"
    "Service Offering" = "Get To An Answer"
  }
  service_prefix = "s263d01"
  location       = "uksouth"
}
```
We have .tfvars files for each environment, which are used to set the `environment` and `environment_prefix` variables.

## Resource naming conventions

{service-id}{env: d|t|p}{number: 01-99}-{resource-abbr}-uks-{free-text-description}

e.g. `s263d01-api-uks-api-service`

## GitHub Actions

### Environment Secrets

For each environment, the following secrets need updating in GitHub by someone with the correct permissions:

- `AZURE_CLIENT_ID` - The client ID the Azure SDK (and Terraform) should use
- `AZURE_CLIENT_SECRET` - The secret for the above client ID
- `AZURE_SUBSCRIPTION_ID` - The subscription in which the new infrastructure should live
- `AZURE_TENANT_ID` - The tenant in which the new infrastructure should live

## Post-Deployment Steps

TDB