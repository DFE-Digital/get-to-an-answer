# Stopping The Site

## Overview
How to temporarily disable the service during incidents (e.g., DDoS, critical vulnerability) and how to bring it back safely.

## Prerequisites
- Access to the Azure subscription and resource group hosting the service.
- Permission to manage App Services and networking endpoints.
- Coordination channel open with stakeholders.

## Options to stop traffic

### 1) Stop Web Apps (fastest)
- For each environment Web App (API, Frontend, Admin as applicable), store service via the Azure console (manually).
- Effect: containers are halted; endpoints become unavailable.
- Rollback: Start the Web Apps and verify health.

### 2) Block ingress via front-door or network security group
- If an edge or routing component fronts the apps, disable the endpoint or block routes.
- Effect: external traffic is cut off while apps may remain running.
- Rollback: re-enable the endpoint and verify routes/health.

## Safe shutdown checklist
- Notify DfE, colleagues and support engineers.
- Confirm action in the correct subscription and environment.
- Stop the apps or block the traffic to the apps and then verify endpoints are unavailable.
- Monitor for any other suspicious activity.

## Bringing the site back
- Re-enable endpoints or start Web Apps.
- Validate:
    - App health endpoints return healthy status.
    - Admin can authenticate and basic CRUD works (if applicable).
    - Frontend loads and can reach the API.
- Monitor logs and metrics for stability before closing the incident.

## Notes
- Configuration and secrets are untouched by stop/start.
- If configuration changed during downtime, review and redeploy before restarting.
- Document actions taken and outcomes for post-incident review.