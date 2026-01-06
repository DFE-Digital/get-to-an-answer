# Setup the terraform backend

This document describes how to setup the terraform backend for the GTAA service.

## Prerequisites

- Terraform
- Azure CLI

## Steps

1. Create an Azure resource group.
```bash
# **ENVIRONMENT** is the environment you are deploying to.
# **SERVICE_NAME** is the name of the service you are deploying.
# **SERVICE_OFFERING** is the service offering you are deploying.
az group create \
            -n ${{ vars.GTAA_PREFIX }}rg-uks-terraform-state \
            -l uksouth \
            --tags Environment="{ **ENVIRONMENT** }" Product="{ **SERVICE_NAME** }" "Service Offering"="**SERVICE_OFFERING**"
```

2. Create a storage account in the Azure Portal.
```bash
# **SERVICE_PREFIX** is the service id provided when you get the Azure subscription.
# **ENVIRONMENT** is the environment you are deploying to.
# **SERVICE_NAME** is the name of the service you are deploying.
# **SERVICE_OFFERING** is the service offering you are deploying.
az storage account create \
            -g { **SERVICE_PREFIX** }rg-uks-terraform-state \
            -n ${{ steps.tfenv.outputs.sa }} \
            -l uksouth \
            --sku Standard_LRS \
            --kind StorageV2 \
            --https-only true \
            --min-tls-version TLS1_2 \
            --allow-blob-public-access false \
            --tags Environment="{ **ENVIRONMENT** }" Product="{ **SERVICE_NAME** }" "Service Offering"="**SERVICE_OFFERING**"
```

3. Assign blob storage role/permissions to the service principal
```bash
# **SERVICE_PREFIX** is the service id provided when you get the Azure subscription.
# **STORAGE_ACCOUNT_NAME** is the name of the storage account created.
# **SERVICE_PRINCIPAL_CLIENT_ID** is the client ID of the service principal.
# **SERVICE_PRINCIPAL_SUBSCRIPTION_ID** is the subscription ID of the service principal.
az role assignment create \
            --assignee { **SERVICE_PRINCIPAL_CLIENT_ID** } \
            --role "Storage Blob Data Owner" \
            --scope "/subscriptions/{ **SERVICE_PRINCIPAL_SUBSCRIPTION_ID }/resourceGroups/{ **SERVICE_PREFIX** }rg-uks-terraform-state/providers/Microsoft.Storage/storageAccounts/{ **STORAGE_ACCOUNT_NAME** }"
```

4. Create a container in the storage account.
```bash
# **STORAGE_ACCOUNT_NAME** is the name of the storage account created.
az storage container create --name tfstate \
            --account-name { **STORAGE_ACCOUNT_NAME** } --auth-mode login
```

5. Add terraform backend configuration in the root of the project.
```terraform
terraform {
  ...
  backend "azurerm" {}
}
```

6. Add the backend configuration to the terraform init command.

   a. Add variable with configuration details.
    ```bash
    # **SERVICE_PREFIX** is the service id provided when you get the Azure subscription.
    # **SERVICE_ACCOUNT_NAME** is the name of the storage account created.
    # **ENVIRONMENT** is the environment you are deploying to.
    COMMON='-backend-config="resource_group_name={ **SERVICE_PREFIX** }rg-uks-terraform-state" -backend-config="storage_account_name={ **SERVICE_ACCOUNT_NAME** }" -backend-config="container_name=tfstate" -backend-config="key={ **ENVIRONMENT** }.tfstate"'
    ```

   b. Add the backend configuration to the terraform init command.
    ```bash
    eval terraform init -reconfigure $COMMON       
    ```
   Or:
    ```bash
    eval terraform init -migrate-state $COMMON
    ```
