# Infrastructure — Azure, Containers, Networking, and Automation

This folder defines how the platform is provisioned, deployed, and run across environments. It covers cloud resources (Azure), container images, CI/CD pipelines, and a local Docker Compose setup.

## Scope
- Provision Azure resources (App Services, VNet, ACR, SQL, DNS) via Terraform/OpenTofu.
- Build and deploy containerized apps (API, Admin, Frontend) via GitHub Actions to Azure Web Apps (Linux).
- Enforce secure networking with VNet integration and private SQL access.
- Provide a local development environment using Docker Compose.

## Service view

```mermaid
flowchart TB
  subgraph ELZ["Enterprise Landing Zone (ELZ)"]
    subgraph VNET[GTAA VNet]
      API["API Service (Linux Web App)"]:::app
      ADM["Admin Web Application (Linux Web App)"]:::app
      FE["Front-End Web Application (Linux Web App)"]:::app
      SQL[(Azure SQL Database)]:::db
      PDZ[Private DNS Zone *.database.windows.net]:::dns
      NSG[Network Security Group]:::sec
    end
    ACR[Azure Container Registry]:::acr
    ASP["App Service Plan (Linux)"]:::plan
  end

  PUB[Public Internet]:::ext -->|HTTPS| WAF[Shared DfE WAF / Azure Front Door]:::waf
  WAF -->|"HTTPS (future routing)"| FE
  WAF -->|"HTTPS (future routing)"| ADM
  WAF -->|"HTTPS (future routing)"| API

  FE -->|HTTPS| API
  ADM -->|HTTPS| API
  API -->|Private Link/VNet| SQL

  ACR --> API
  ACR --> ADM
  ACR --> FE

  ASP --> API
  ASP --> ADM
  ASP --> FE

  PDZ --- VNET
  NSG --- VNET

  classDef app fill:#e8f4ff,stroke:#5aa0ff,color:#003e74;
  classDef db fill:#fff5e6,stroke:#ffab40,color:#5d3b00;
  classDef dns fill:#f5f5f5,stroke:#bbb,color:#333;
  classDef sec fill:#f0fff4,stroke:#34c759,color:#0b5;
  classDef plan fill:#eef,stroke:#66c,color:#224;
  classDef acr fill:#eefafc,stroke:#27a9e1,color:#036;
  classDef waf fill:#fdeef2,stroke:#f66,color:#a00;
  classDef ext fill:#eee,stroke:#999,color:#333;
```


## Components

- Azure Resource Group: per-environment grouping of assets.
- App Service Plan (Linux): hosts the three Web Apps.
- Linux Web Apps:
  - API: serves data to Frontend/Admin; VNet-integrated; HTTPS-only.
  - Admin: back-office UI; calls API over HTTPS; VNet-integrated.
  - Frontend: public site; calls API over HTTPS; VNet-integrated.
- Azure Container Registry (ACR): stores Docker images for all apps.
- Networking:
  - Virtual Network with delegated subnet for App Services (Swift integration).
  - Network Security Group restricting inbound; emphasis on HTTPS-only flows.
  - Private DNS Zone for Azure SQL; VNet link for private resolution.
- Data:
  - Azure SQL Server/Database (referenced in networking and app settings).
- Pipelines (GitHub Actions):
  - Deploy API/Admin/Frontend (build, push to ACR, update Web Apps).
  - Deploy Infrastructure (Terraform init/plan/apply with remote state).

## Local development

- Docker Compose runs API, Admin, and Frontend together.
- Internal service DNS allows Admin/Frontend to call API at http://gettoananswer-api:8080.
- ASPNETCORE_ENVIRONMENT=Local is used for all containers.

```mermaid
flowchart LR
  FE[Frontend Container] -->|http://gettoananswer-api:8080| API[API Container]
  ADM[Admin Container] -->|http://gettoananswer-api:8080| API
```


## CI/CD flow (overview)

```mermaid
sequenceDiagram
  autonumber
  participant Dev as Developer
  participant GH as GitHub Actions
  participant TF as Terraform on Azure
  participant ACR as Azure Container Registry
  participant ASP as Azure Web Apps

  Dev->>GH: Manual dispatch (environment: Development/Test)
  GH->>TF: terraform init/plan/apply (remote state per env)
  TF-->>ASP: Provision/Update ACR, VNet, Web Apps, DNS, SQL
  GH->>ACR: Build & push images (api/admin/frontend) tagged with commit SHA
  GH->>ASP: Point Web Apps to ACR images + set app settings
  ASP-->>Dev: Deployment complete
```


## Implemented vs. planned (unimplemented) items

Implemented
- App Services for API, Admin, Frontend on a Linux plan.
- ACR with admin credentials for image pulls.
- VNet, delegated subnet, NSG, Private DNS zone, VNet links.
- VNet integration (Swift) for all three Web Apps.
- HTTPS-only configuration and TLS 1.2 minimum.
- GitHub Actions for Infrastructure and each application deployment.
- Local Docker Compose environment.

Unimplemented or stubbed (planned)
- Shared DfE WAF/Azure Front Door profile, endpoint, origins, and routes:
  - Front Door resources and per-path routing to API/Admin/Frontend are currently commented/stubbed.
  - When implemented, all public ingress should route via WAF with HTTPS enforcement and origin groups.
- Private endpoints/private link for SQL server (current design uses VNet integration and service endpoints; a private endpoint could further harden access).
- Centralized secret management (e.g., Azure Key Vault) for connection strings and app secrets with managed identity bindings.
- Autoscaling policies and higher SKUs for App Service Plan (currently B1 basic).
- Blue/green or staged deployments (slots) for zero-downtime releases.
- Observability stack (centralized logs/metrics/dashboards and alerting).
- Web Application Firewall policy tuning and custom rules once Front Door is enabled.
- IP allowlists or private ingress for Admin (if required by policy).

## Operations checklist

- Environments: choose Development or Test when running workflows.
- Image tagging: deployments use the current commit SHA; rollbacks can re-point to a prior tag.
- App settings: ensure API base URL and connection strings are set per environment.
- Remote Terraform state: stored in Azure Storage with a per-environment key; do not commit local state.

## Security notes

- Enforce HTTPS only on all Web Apps; minimum TLS 1.2.
- Keep ACR credentials secret; prefer managed identity when feasible (future).
- Restrict SQL access to VNet; consider private endpoint for strongest posture.
- Route external traffic via WAF/Front Door when implemented to centralize security controls.