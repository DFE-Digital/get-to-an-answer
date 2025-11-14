workspace "GetToAnAnswer" "Find Education and Training - Hosting (Terraform-derived App Service)" {

  model {
    properties { "structurizr.groupSeparator" "/" }

    group "DfE" {
      gtaa = softwareSystem "Find Education and Training" {
        description "Frontend, API, and Admin running on Azure App Service (Linux) behind Front Door"
        tags "System"

        frontend = container "Frontend" {
          technology "ASP.NET Core (container on App Service)"
          tags "Azure/AppService"
          description "Public-facing site"
        }
        api = container "API" {
          technology "ASP.NET Core (container on App Service)"
          tags "Azure/AppService"
          description "Backend API"
          frontend -> this "Calls" "HTTPS"
          admin -> this "Calls" "HTTPS"
        }
        admin = container "Admin" {
          technology "ASP.NET Core (container on App Service)"
          tags "Azure/AppService"
          description "Administrative UI"
        }
      }
    }

    service = deploymentEnvironment "Service" {
      deploymentNode "Microsoft Azure" {
        tags "Azure/Subscription"

        region = deploymentNode "Region" {
          tags "Azure/Region"

          internet = deploymentNode "Public Internet" { tags "Azure/Public" }
          afd = infrastructureNode "Azure Front Door (WAF)" {
            technology "Azure Front Door"
            tags "Azure/FrontDoor"
            description "Global entry point with WAF"
          }

          rg = deploymentNode "Resource Group" { tags "Azure/ResourceGroup" }

          // Networking
          vnet = deploymentNode "Virtual Network" { tags "Azure/VNet" }
          subnet = deploymentNode "App Service Subnet" { tags "Azure/Subnet" }
          vnet -> subnet "Contains"

          // Compute (App Service)
          asp = deploymentNode "App Service Plan (Linux)" {
            tags "Azure/AppServicePlan"

            api_app = deploymentNode "API - Linux Web App" {
              tags "Azure/WebApp"
              api_inst = containerInstance api {
                afd -> this "Routes traffic (backend origin)" "HTTPS"
              }
              api_slot = deploymentNode "API - Staging Slot" { tags "Azure/WebApp/Slot" }
            }

            admin_app = deploymentNode "Admin - Linux Web App" {
              tags "Azure/WebApp"
              admin_inst = containerInstance admin {
                afd -> this "Routes traffic (admin path)" "HTTPS"
              }
              admin_slot = deploymentNode "Admin - Staging Slot" { tags "Azure/WebApp/Slot" }
            }

            fe_app = deploymentNode "Frontend - Linux Web App" {
              tags "Azure/WebApp"
              fe_inst = containerInstance frontend {
                afd -> this "Routes traffic (frontend path)" "HTTPS"
              }
              fe_slot = deploymentNode "Frontend - Staging Slot" { tags "Azure/WebApp/Slot" }
            }
          }

          // VNet Integration (regional VNet integration from Web Apps to subnet)
          api_app -> subnet "VNet integration"
          admin_app -> subnet "VNet integration"
          fe_app -> subnet "VNet integration"

          // Platform services used by apps
          acr = infrastructureNode "Azure Container Registry" {
            technology "ACR"
            tags "Azure/ACR"
            description "Hosts application images"
          }

          ai = infrastructureNode "Application Insights" {
            technology "Azure Monitor/App Insights"
            tags "Azure/AppInsights"
            description "Telemetry and traces"
          }

          sql = infrastructureNode "Azure SQL Database" {
            technology "Azure SQL"
            tags "Azure/SQL"
            description "Relational DB (ADO.NET connection string from Terraform)"
          }

          storage = infrastructureNode "Storage Account" {
            technology "Azure Storage"
            tags "Azure/Storage"
            description "Blob/file storage (if configured)"
          }

          // App dependencies
          api_inst -> sql "Reads/Writes" "TDS (1433)"
          admin_inst -> api_inst "Calls" "HTTPS"
          fe_inst -> api_inst "Calls" "HTTPS"

          api_inst -> storage "Reads/Writes blobs" "HTTPS"
          admin_inst -> storage "Reads/Writes blobs" "HTTPS"
          fe_inst -> storage "Reads/Writes blobs" "HTTPS"

          // Telemetry
          api_inst -> ai "Sends telemetry"
          admin_inst -> ai "Sends telemetry"
          fe_inst -> ai "Sends telemetry"

          // Image pulls
          api_inst -> acr "Pulls container image"
          admin_inst -> acr "Pulls container image"
          fe_inst -> acr "Pulls container image"

          // Ingress
          internet -> afd "Requests" "HTTPS"
          afd -> fe_inst "Frontend origin" "HTTPS"
          afd -> admin_inst "Admin origin" "HTTPS"
          afd -> api_inst "API origin" "HTTPS"
        }
      }
    }
  }

  views {
    deployment * Service {
      title "Terraform Deployment View (Azure App Service)"
      include *
      autoLayout lr
    }

    themes https://raw.githubusercontent.com/structurizr/themes/refs/heads/master/microsoft-azure-2024.07.15/icons.json
  }
}