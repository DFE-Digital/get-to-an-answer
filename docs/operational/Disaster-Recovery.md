# Disaster Recovery

## Overview
This document outlines how the platform is restored or operated during incidents affecting application availability or hosting infrastructure.

## Hosting footprint (Azure)
Services run on Azure and are provisioned via Infrastructure as Code. Typical components include:
- Azure App Service (API, Admin, Frontend)
- Azure networking and supporting resources
- Azure SQL (or equivalent data persistence if configured)
- Container Registry and CI/CD pipelines

Most application components are stateless containers. Persistent state (e.g., database) must be recoverable via standard backup/restore procedures.

## Recovery principles
- Reprovision quickly using IaC.
- Restore minimal viable service first (API, then Frontend/Admin).
- Keep secrets out of repos; reconfigure via environment variables or secure stores.
- Validate health endpoints before reopening traffic.

## Scenarios and actions

- App Service or container failure
    - Redeploy the affected app via CI/CD.
    - If platform issue persists, recreate the Web App and rebind configuration.
    - Verify health checks and application logs.

- Region/resource outage
    - Recreate infrastructure in the same or alternate region using IaC variables (based DfE ELZ teams recommendations).
    - Re-deploy images from the container registry.
    - Update DNS or traffic manager/front-door targets if endpoints change.

- Database unavailability or corruption
    - Failover to secondary (if configured) or perform point-in-time restore.
    - Re-point connection strings and run health checks.
    - Validate application read/write paths.

- Configuration or secret loss
    - Re-seed environment variables/app settings from secure sources.
    - Rotate credentials if exposure is suspected.
    - Re-run deployment to ensure consistency.

## RTO/RPO targets
- RTO: restore core service within DfE standards timeframe by automating infrastructure and deployments.
- RPO: limited data loss based on database backup frequency and retention; use PITR/failover for reduction.

## Operational checklist
- Confirm scope and impact; notify stakeholders.
- Choose recovery path (redeploy vs. reprovision).
- Apply IaC to recreate resources if needed.
- Deploy latest known-good container images.
- Restore database (failover/PITR) if required.
- Validate health checks, logs, and synthetic tests.
- Restore traffic and monitor.

## Prevention and readiness
- Regularly validate backups and restore procedures.
- Keep IaC current with production state.
- Automate smoke tests post-deploy.
- Document environment variables and required app settings (stored securely).

## Glossary
- **IaC**: Infrastructure as Code
- **RTO**: Recovery Time Objective
- **RPO**: Recovery Point Objective
- **PITR**: Point-in-time restore
- **Failover**: Manual or automated failover
- **Synthetic tests**: Automated tests to validate application health
- **Synthetic monitoring**: Automated monitoring to validate application health
- **Synthetic monitoring dashboard**: Dashboard to monitor synthetic tests
- **Synthetic monitoring alerts**: Alerts to notify stakeholders of test failures