# Azure Infrastructure (Terraform)

## What it is
Infrastructure-as-code for provisioning the application’s Azure resources:
- Resource Group, App Service Plan, Linux Web Apps (API, Admin, Frontend)
- Azure Container Registry (ACR)
- Virtual Network, Subnet, Network Security Group
- Private DNS Zone and VNet link
- Azure SQL Server and Database
- VNet integration for Web Apps and SQL VNet rule

Two environment configurations are provided:
- Development (development.tfvars)
- Test (test.tfvars)

## Prerequisites
- Terraform CLI (or OpenTofu) v1.3+ recommended
- Azure CLI (az) authenticated to the target subscription
- Azure service principal with permissions to deploy (Contributor or scoped roles)
- Access to ACR images (or push images to the created ACR)
- Required secrets/variables available for CI and for local runs:
    - ARM_CLIENT_ID
    - ARM_CLIENT_SECRET
    - ARM_TENANT_ID
    - ARM_SUBSCRIPTION_ID
    - TF_VAR_sql_admin_username
    - TF_VAR_sql_admin_password

## How CI/CD installs and sets it up
The GitHub Actions workflow:
- Logs into Azure using the service principal from repository secrets.
- Exposes environment variables for Terraform:
    - ARM_CLIENT_ID, ARM_CLIENT_SECRET, ARM_TENANT_ID, ARM_SUBSCRIPTION_ID (Azure auth)
    - TF_VAR_sql_admin_username, TF_VAR_sql_admin_password (Terraform variables)
- Selects the target environment via workflow input (Development or Test) and runs:
    - terraform init
    - terraform plan -var-file=<environment>.tfvars
    - terraform apply with the generated plan

Variables are retrieved via environment variables set in CI (GitHub Secrets):
- Terraform automatically maps TF_VAR_<variable_name> to the corresponding input variable.
- For example, TF_VAR_sql_admin_password populates variable "sql_admin_password".

## Set up for local development
1) Sign in to Azure and select subscription:
```bash
az login az account set --subscription "<your-subscription-id>"
```

2) Export environment variables for Terraform (same names as CI):
```bash
export ARM_CLIENT_ID="" export ARM_CLIENT_SECRET=" " export ARM_TENANT_ID=" " export ARM_SUBSCRIPTION_ID=" "
export TF_VAR_sql_admin_username="azureadmin" export TF_VAR_sql_admin_password="<strong-password>"
```

3) Move to the Terraform directory:
```bash
cd src/Infrastructure/terraform
```

4) Initialize:
```bash
terraform init -upgrade 
```

5) Plan for your target environment:
- Development
```bash
terraform plan -var-file=development.tfvars -out=tfplan
```
- Test
```bash
terraform plan -var-file=test.tfvars -out=tfplan
```

6) Apply:
```bash
terraform apply -auto-approve tfplan
```

7) Retrieve outputs (URLs, names, FQDNs):
```bash
terraform output
```

## Notes
- Ensure the chosen environment file (development.tfvars or test.tfvars) matches your intended deployment.
- If your organization enforces policies (e.g., minimum TLS, private endpoints), adjust variables and resources accordingly.
- Store secrets only in secure locations (e.g., GitHub Secrets for CI, your shell/key vault locally) and never commit them to source control.