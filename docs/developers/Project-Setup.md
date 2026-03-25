# Project Setup

This repository contains a multi-project .NET 10 solution with three web applications (API, Admin, Frontend), a shared Common library, automated tests, infrastructure-as-code, and developer documentation.

## Prerequisites

- .NET SDK 10.x
- Node.js 20+ with npm (for Admin/Frontend assets and E2E tests)
- Docker Desktop (for local compose and E2E)
- Azure CLI and Terraform (for infrastructure workflows, optional)
- Git

## Solution structure (high level)

- src/
    - Api — backend HTTP services
    - Admin — back-office management UI
    - Frontend — public-facing questionnaire site
    - Common — shared domain and infrastructure code
    - Infrastructure — Terraform and local Docker Compose
    - e2e — end-to-end tests
- tests/
    - Unit.Tests — unit tests
    - Integration.Tests — integration tests
- docs/ — documentation and developer guides

Refer to each project’s README for details.

## Running locally

Option A: Run via IDE
- Restore and build the solution.
- Start the API, then Admin and Frontend.
- Ensure Admin/Frontend are configured to call the API base URL from appsettings.Local.json.

Option B: Run via Docker Compose
- Copy Infrastructure/.env.example to .env and adjust values as needed.
- From src/Infrastructure run:
    - docker compose -f compose.yaml up --build
- Services will be available at the ports defined in compose.yaml.

### Creating a valid login token

In order to login to a locally running version of the admin, you will need to create a valid JWT to use with the Mock OAuth.

To create this, head to https://jwt.io, head to the JWTEncoder, select `HS256` as the signing algorithm, then click  `Generate Example`

Once you've done this, replace the payload with the following:

```json
{
  "iss": "http://dfe-issuer",
  "aud": "test-audience",
  "sub": "test-subject",
  "name": "",
  "emailaddress": "",
  "role": [
    "Admin"
  ],
  "oid": "",
  "exp": 0,
  "iat": 0
}
```

- Set `name` to your name
- Set `emailaddress` to an `@education.gov.uk` email address
- Set `oid` to a GUID
- Set `exp` to an epoch in the future
- Set `iat` to an epoch in the past

To get epochs, there is a useful tool at https://www.epochconverter.com/

Once you have updated the payload in the https://jwt.io site, copy the encoded JWT to your clipboard.

From there, you will need to go to your locally running admin and enter the url

`/dev/login?jt=XXXXXXX` where `XXXXXX` is the encoded JWT

## Configuration

- Each app has appsettings.*.json files. For local development, use appsettings.Local.json (not committed).
- Typical settings include API base URLs, database connection strings, and feature flags.
- Keep secrets out of source control; use environment variables or local user secrets when not using Docker.

## End-to-end tests

We provide Playwright-based E2E tests that run against a locally hosted site or containers.

Recommended workflow:
- Build local images and run the local compose stack (see “Running locally — Docker Compose”).
- From the E2E project folder, install and run tests:
    - yarn ci
    - yarn playwright install
    - yarn playwright test
- To view failures:
    - yarn playwright show-report

Note: Stop any conflicting local instances before running against containers to avoid port clashes.

## Testing (unit and integration)

- From the repo root:
    - dotnet test
- Integration tests may rely on local configuration (see tests/Integration.Tests/appsettings.LocalTest.json). Ensure the API and any required services are reachable or use the compose stack.

## Infrastructure

Infrastructure (Azure resources, networking, and app hosting) is defined under src/Infrastructure/terraform and deployed via CI/CD. Local development does not require provisioning cloud resources.

- To experiment locally with IaC workflows:
    - Ensure Azure CLI authentication.
    - Use terraform init/plan/apply with a non-shared state backend if testing outside CI.
- Do not commit local state or secrets.

## Developer workflows

- Branching and release processes are documented in docs/developers.
- CI pipelines validate code quality, run tests, and build container images.
- Application deploy workflows target environments using tagged images.

## Troubleshooting

- Port conflicts: stop existing instances or adjust compose/launch settings.
- SSL locally: verify HTTPS ports and certificates where applicable, or use HTTP for local if configured.
- Frontend/Admin cannot reach API: confirm API base URL and CORS settings in appsettings.Local.json or environment variables.
- Playwright issues: run npx playwright install and ensure services are reachable before tests.

## References

- API: src/Api/README.md
- Admin: src/Admin/README.md
- Frontend: src/Frontend/README.md
- Common: src/Common/README.md
- Infrastructure: src/Infrastructure/README.md
- E2E: src/e2e/README.md (or the E2E project README within src/e2e)