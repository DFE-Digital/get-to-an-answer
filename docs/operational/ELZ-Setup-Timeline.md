# Azure Enterprise Landing Zone (ELZ) Setup Timeline

## Overview
This document details our process/experience being one of the first to deploy to the new ELZ.

## Azure Subscription Prerequisites

- A service offering needs to be filed
- A Dev and Test subscription ServiceNow form need to be submitted

## DfE Azure Setup

When you first get your Dev and Test Azure Subscription details, it will have the service principal access credentials sent in a secure email (you'll have to manually copy the details).

### Setup the terraform state management
[Terraform state setup](../architecture/Terraform-State-Setup.md)

### Steps to setup the new ELZ Dev/Test environment(s)
1. Registering resource providers.
```bash
az provider register --namespace Microsoft.AlertsManagement
```

- Challenges around setting up the subnet due to a ELZ policy, denying the creation.
- Challenges around storing user email in plain text in the database.
- Challenges around Dev Shared WAF and Shared App Registration ServiceNow tickets.
- Setting up frontdoor and custom domains.
- IP restricting apps and setting a private endpoint, for internal communication between services.
- Challenges around getting test users in Entra ID, for our Api and Admin apps.
- Challenges around assigning role to managed identities.
- Setting up KeyVault.

All services are provisioned via Terraform, possibly even the DevOps instrumentation, so others can repeat the process.

To do this, we have been configuring the deployment accordingly.

## Infrastructure as Code Changes

These changes exist in the `src/Infrastructure/terraform` folder of the root project.

Currently this is set to:

```terraform
env      = "{environment}"
asp_env  = "{AspNetEnvironment}"
product  = "Get-To-An-Answer"
prefix   = "s263d01"
location = "uksouth"
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