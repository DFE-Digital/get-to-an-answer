# Get To An Answer

AI Assistant here. This document provides a practical, granular guide to develop, run, test, and deploy the Checker solution.

## Overview

Checker is a multi-project .NET 9 solution built with ASP.NET Core MVC and Web API, plus a web admin frontend that uses Node.js tooling.

Solution structure (high level):
- src/Common: Shared domain, DTOs, enums, and EF Core persistence
- src/Api: ASP.NET Core Web API (questionnaires, questions, answers)
- src/Admin: ASP.NET Core MVC admin site with frontend assets built via Gulp/Sass
- tests: Test projects (if present)
- terraform: Infrastructure as code (Terraform)
- docs: Additional documentation (if present)

Tech stack:
- .NET 9, C# 13, ASP.NET Core, MVC, Razor
- Entity Framework Core (DbContexts in Common)
- Node.js + npm, Gulp 5, gulp-sass 6, Sass 1.83, govuk-frontend 5.10, dfe-frontend 2.0.1

Package management:
- .NET: via dotnet SDK
- JavaScript: npm (do not use yarn)

## Prerequisites

- .NET SDK 9.x
- Node.js 18+ (LTS recommended)
- npm 9+ (bundled with Node)
- Terraform (if using IaC)
- A SQL database (connection strings configured via appsettings)

Verify:
- dotnet --info
- node -v
- npm -v
- terraform -version

## Getting Started

1) Clone and restore
- git clone <repo-url>
- cd Checker
- dotnet restore

2) Install admin frontend dependencies (npm)
- cd src/Admin
- rm -f yarn.lock  # ensure npm is used
- npm ci

3) Build solution
- cd ../../
- dotnet build

4) Apply EF Core migrations (if applicable)
- Ensure connection strings in appsettings.Development.json
- From solution root:
    - dotnet ef database update --project src/Common/Common.csproj
    - Repeat for audit DB if needed

5) Run projects
- API:
    - dotnet run --project src/Api/Api.csproj
- Admin:
    - dotnet run --project src/Admin/Admin.csproj

API and Admin each have their own launchSettings.json for ports and profiles.

## Configuration

Environment-specific settings are in:
- src/Api/appsettings.json and appsettings.Development.json
- src/Admin/appsettings.json and appsettings.Development.json

Common patterns:
- ConnectionStrings: main and audit databases
- Logging levels
- CORS origins (for any frontend consuming the API)
- Feature flags or environment-specific switches

Override via environment variables in production:
- ASPNETCORE_ENVIRONMENT=Production
- ConnectionStrings__Default=...
- ConnectionStrings__Audit=...

## Projects

### Common (src/Common)
- Domain DTOs: Question, Answer, Destination, Questionnaire
- Enums: EntityStatus, QuestionType, DestinationType
- Requests: Create/Update DTOs for API contracts
- EF Core:
    - Entities: AnswerEntity, QuestionEntity, QuestionnaireEntity, QuestionnaireSnapshotEntity
    - DbContexts: CheckerDbContext (primary), CheckerAuditDbContext (audit)
- Local utilities: e.g., MockActiveDirectoryApiClient (for local dev/testing)

Build/test dependency: other projects reference this library.

### API (src/Api)
- ASP.NET Core Web API
- Controllers:
    - AnswerController, QuestionController, QuestionnaireController: CRUD and workflow endpoints
    - WebController: health/info endpoints (and/or root routing)
- Program.cs: service registration, EF Core context wiring, CORS, JSON, etc.
- appsettings.*: API config, connection strings

Run:
- dotnet run --project src/Api/Api.csproj
  Test via:
- Checker.Api.http (HTTP scratch file) or curl/Postman

### Admin (src/Admin)
- ASP.NET Core MVC Razor app for administering questions/questionnaires
- Controllers: HomeController, PreviewController
- Views: Create/Edit/Manage Questions and Questionnaires, Publish/Delete confirmations, Preview
- wwwroot: compiled static assets
- AssetSrc: source styles/scripts for Gulp build
- Frontend toolchain:
    - Gulpfile.js
    - package.json (npm scripts)
    - govuk-frontend + dfe-frontend styles
    - gulp-sass (Dart Sass)

Run:
- npm run build (build assets)
- dotnet run --project src/Admin/Admin.csproj

Recommended npm scripts (package.json):
- npm ci
- npm run build
- npm run watch (if defined) for live asset builds

Note: Use npm (not yarn). Remove yarn.lock to avoid conflicts.

## Development Workflow

- Restore and build once
- Run API and Admin concurrently
- If editing Sass/JS in Admin:
    - npm run watch (if available) to rebuild assets on change
    - Otherwise rerun npm run build

EF Core:
- Add migration:
    - dotnet ef migrations add <Name> --project src/Common/Common.csproj
- Update DB:
    - dotnet ef database update --project src/Common/Common.csproj

Testing:
- dotnet test
- Place tests under tests/ with appropriate references to Common, Api, Admin

Logging:
- Configure log levels in appsettings.Development.json
- Use ASP.NET console logging during development

## Running in IDE

- JetBrains Rider, Visual Studio, or VS Code supported
- Open Checker.sln
- Set multiple startup projects (Api, Admin) or run individually
- Use launchSettings.json profiles

## API Usage

- Base URL from Api launch settings
- Endpoints typically under:
    - /api/questionnaires
    - /api/questions
    - /api/answers
- JSON contracts use DTOs defined in Common/Domain/Request and Domain/*. Map from Admin UI forms to these payloads.

Use Checker.Api.http for quick manual requests.

## Frontend Assets (Admin)

Dependencies:
- govuk-frontend and dfe-frontend for styles/components
- gulp tasks for:
    - Compiling Sass to CSS
    - Bundling/minifying JS (if configured)
    - Copying assets to wwwroot

Typical commands:
- npm ci
- npm run build
- npm run watch

Ensure Node and npm are up to date.

## Environment Setup

Local development:
- ASPNETCORE_ENVIRONMENT=Development
- Use local SQL (e.g., SQL Server, PostgreSQL) per your EF provider
- Configure connection strings in Admin and Api appsettings.Development.json if each app needs DB access

Staging/Production:
- Use environment variables or appsettings.Production.json
- Harden CORS, logging, and error handling
- Apply DB migrations before deployment

## Terraform (terraform/)
- main.tf and related files describe infrastructure (e.g., app service, DB, networking)
- Initialize: terraform init
- Plan: terraform plan -var-file=env/<environment>.tfvars
- Apply: terraform apply -var-file=env/<environment>.tfvars
- Keep secrets out of VCS; use remote state/backends as needed

## CI/CD (suggested)

- dotnet restore
- npm ci (src/Admin)
- npm run build (src/Admin)
- dotnet build --configuration Release
- dotnet test --configuration Release
- dotnet publish src/Api -c Release -o out/api
- dotnet publish src/Admin -c Release -o out/admin
- Publish artifacts
- Run terraform plan/apply in deployment stages

## Troubleshooting

- Port conflicts: change applicationUrl in launchSettings.json or set ASPNETCORE_URLS
- Static assets not updating in Admin: re-run npm run build or watch; clear browser cache
- DB errors:
    - Verify connection strings
    - Run migrations
    - Check provider compatibility with .NET 9
- CORS: ensure Admin origin is allowed by API CORS policy during local dev
- Node tool mismatch: ensure npm, not yarn. Delete yarn.lock; prefer npm ci over npm install for reproducible builds.

## Contributing

- Branch from main, follow conventional commits if used
- Add/adjust unit/integration tests
- Keep Common contracts stable; version API if breaking changes are needed
- Run formatters/analyzers and ensure build passes

## License

Include your project’s license here.

## Contact

For issues or questions, open an issue or contact the maintainers.