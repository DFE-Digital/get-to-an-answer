workspace "CxD - GTAA" "CxD - Get To An Answer (GTAA)" {

    model {
    
        gtaa = softwareSystem "Guided Content Flow" {
            api = container "API Service" {
                description "API for admin system and front end communication"
                technology "dotnet 9.0 web api"
                tags "Microsoft Azure - API Management Services"
            }
            
            webapp = container "Front-End Web Application" {
                description "Front end for hosting various versions of the site"
                technology "dotnet 9.0"
                tags "Microsoft Azure - Website Staging"
                api -> this "Fetches information from"
            }
            
            
            
            admin = container "Admin Web Application" {
                description "Admin interface for the site"
                technology "dotnet 9.0"
                tags "Microsoft Azure - Website Staging"
                this -> api "Writes to"
                api -> this "Reads from"
            }

        }
        
        service = deploymentEnvironment "Service" {

            deploymentNode "Enterprise Landing Zone (ELZ)" {
                tags "Microsoft Azure - Azure A"

                deploymentNode "UK South" {
                    tags "Microsoft Azure - Region Management"
                    
                    internet = deploymentNode "Public Internet" {
                        tags "Microsoft Azure - Entra Internet Access"
                        
                        fd = infrastructureNode "Azure Front Door (Dev/Test/Prod)" {
                            technology "Azure Front Door and Firewalls"
                            description "Automatically distributes and secures incoming application traffic"
                            tags "Microsoft Azure - Firewalls"
                        }
                    }
                    
                    entra = deploymentNode "DfE Entra" {
                        tags "Microsoft Azure - Entra Privleged Identity Management"
                        login = infrastructureNode "DfE Users" {
                            tags "Microsoft Azure - Entra Identity Roles and Administrators"
                            technology "Microsoft Entra"
                        }
                    }
                    
                    vnet = deploymentNode "GTAA Vnet" {
                        tags "Microsoft Azure - Virtual Networks"
                        
                        db = infrastructureNode "Relational DB" {
                            description "SQL Server for storing Flows and Authentication"
                            tags "Microsoft Azure - SQL Server"
                        }
                        
                        storage = infrastructureNode "Azure Storage" {
                            description "Storage for uploaded images"
                            tags "Microsoft Azure - Storage Accounts"
                        }
                        
                        keyvault = infrastructureNode "Key Vault" {
                            description "Secrets (Connection Strings, API Keys, etc)"
                            tags "Microsoft Azure - Key Vaults"
                        }
                        
                        webContainer = deploymentNode "Auto-Scaling App Service Plan" {
                            tags "Microsoft Azure - App Service Plans"
                            
                            webAppService = deploymentNode "Web - App Service" {
                                tags "Microsoft Azure - App Services"
                                webInstance = containerInstance webapp {
                                    fd -> this "Fowards requests to" "HTTPS"
                                    storage -> this "Reads images from"
                                }
                            }
                            
                            apiAppService = deploymentNode "API - App Service" {
                                tags "Microsoft Azure - App Services"
                                apiInstance = containerInstance api {
                                    db -> this "Reads data"
                                    this -> db "Writes data"
                                    this -> login "Authenticates against" "@education.gov.uk"
                                    fd -> this "Fowards requests to" "HTTPS"

                                }
                            }
                            
                            adminAppService = deploymentNode "Admin - App Service" {
                                tags "Microsoft Azure - App Services"
                                adminInstance = containerInstance admin {
                                    fd -> this "Fowards requests to" "HTTPS"
                                    this -> login "Authenticates against" "@education.gov.uk"
                                    storage -> this "Reads images from"
                                    this -> storage "Writes images to"
                                }
                            }
                            
                           keyvault -> this "Provides secrets to"
                            
                        }
                           
                    }
                }
            }
        }
    }

    configuration {
        scope softwaresystem
    }
    
    views {
        themes https://raw.githubusercontent.com/structurizr/themes/refs/heads/master/microsoft-azure-2024.07.15/icons.json
    
        
        deployment * Service {
            title "Service View"
            include *
        }
        
        
        
    }

}