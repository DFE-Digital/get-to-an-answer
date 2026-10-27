# Developer Setup Guide: Get to an Answer (GTAA) Project

A comprehensive guide to set up the Get to an Answer project locally for development. This guide covers database setup, API configuration, environment setup, integration tests, and e2e tests.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Part 1: SQL Server Local Installation](#part-1-sql-server-local-installation)
3. [Part 2: API Project Setup & Database Migrations](#part-2-api-project-setup--database-migrations)
4. [Part 3: Frontend & Admin Environments](#part-3-frontend--admin-environments)
5. [Part 4: Integration Tests with Docker](#part-4-integration-tests-with-docker)
6. [Part 5: End-to-End (E2E) Tests](#part-5-end-to-end-e2e-tests)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

Before you start, ensure you have the following installed:

- **Visual Studio 2022** or later (Professional/Enterprise recommended)
- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Git** - [Download](https://git-scm.com/)
- **Docker Desktop** (for Integration Tests) - [Download](https://www.docker.com/products/docker-desktop)
- **PowerShell 5.1+** (comes with Windows 10+)

**Verify installations:**
```powershell
dotnet --version        # Should show .NET 10.x.x
docker --version        # Should show Docker version
git --version           # Should show Git version
```

---

## Part 1: SQL Server Local Installation

### Option A: SQL Server LocalDB (Recommended for Development)

**LocalDB** is a lightweight SQL Server instance perfect for local development. It's included with Visual Studio.

#### Step 1: Verify LocalDB Installation

Open PowerShell as Administrator and run:

```powershell
# Check if LocalDB is installed
sqlcmd -l

# If successful, you'll see connection output
# If not found, you need to install SQL Server
```

#### Step 2: Create a Local Database Instance

```powershell
# Create a new LocalDB instance (if default instance doesn't exist)
SqlLocalDB create "mssqllocaldb"

# Start the LocalDB instance
SqlLocalDB start "mssqllocaldb"

# Verify it's running
SqlLocalDB info "mssqllocaldb"
```

#### Step 3: Connect to LocalDB with SQL Server Management Studio

If you have **SQL Server Management Studio (SSMS)** installed:

1. Open SSMS
2. Server name: `(localdb)\mssqllocaldb`
3. Authentication: **Windows Authentication**
4. Click **Connect**

### Option B: Full SQL Server Installation (Alternative)

If you prefer a full SQL Server instance:

1. Download **SQL Server 2019/2022 Developer Edition** from [Microsoft](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. Run the installer and choose **Developer Edition (Free)**
3. Select **SQL Server Database Engine** during installation
4. Configure with default settings
5. Connection string will be: `Server=YOUR_MACHINE_NAME;Database=YOUR_DATABASE_NAME;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;Trusted_Connection=true;`

### Troubleshooting SQL Server Connection

If you struggle to connect to SQL Server:

```powershell
# Check if SQL Server services are running
Get-Service MSSQLSERVER

# Start SQL Server if stopped
Start-Service MSSQLSERVER

# For LocalDB
SqlLocalDB status "mssqllocaldb"
SqlLocalDB start "mssqllocaldb"
```

---

## Part 2: API Project Setup & Database Migrations

The API project contains the business logic and database access layer. It uses **Entity Framework Core** to manage the database schema through migrations.

### Understanding the Architecture

```
┌─────────────────────────────────────────┐
│       Frontend / Admin Apps             │
│       (Razor Pages - No DB)             │
└──────────────────┬──────────────────────┘
				   │ HTTP Requests
				   ▼
┌─────────────────────────────────────────┐
│    API Project (REST Endpoints)         │
│    - QuestionnaireService               │
│    - QuestionService                    │
│    - AnswerService                      │
│    - ContentService                     │
└──────────────────┬──────────────────────┘
				   │ Entity Framework Core
				   ▼
┌─────────────────────────────────────────┐
│  Common/Infrastructure/Persistence      │
│  - GetToAnAnswerDbContext (DbContext)   │
│  - Entities (Question, Answer, etc.)    │
│  - Migrations                           │
└──────────────────┬──────────────────────┘
				   │ SQL Server Driver
				   ▼
┌─────────────────────────────────────────┐
│         SQL Server (LocalDB)            │
│    - Tables, Indexes, Constraints       │
└─────────────────────────────────────────┘
```

### Step 1: Set the SQL Server Connection String

The connection string tells Entity Framework where your SQL Server is located.

#### In Visual Studio (Recommended)

1. Right-click the **Api** project → **Manage User Secrets**

2. Add the following JSON (or modify if it exists):

```json
{
  "DEFAULT_CONNECTION_STRING": "Server=YOUR-SERVER-NAME;Database=YOUR-DATABASE-NAME;UserId=YOUR-USERNAME;Password=YOUR-PASSWORD;TrustServerCertificate=True;",
  "SQLSERVER_SA_PASSWORD": "YOUR-PASSWORD",
  "E2E_AZURITE_CONNECTION_STRING": "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite"
}
```

**Connection String Breakdown:**
- `Server=YOUR-SERVER-NAME` - SQL Server instance
- `Database=YOUR-DATABASE-NAME` - Database name
- `UserId=YOUR-USERNAME` - SQL Server username
- `Password=YOUR-PASSWORD` - SQL Server password
- `TrustServerCertificate=True` - Skip SSL certificate validation (dev only)

#### Alternative: appsettings.Local.json

If user secrets don't work, create `src/Api/appsettings.Local.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR-SERVER-NAME;Database=YOUR-DATABASE-NAME;UserId=YOUR-USERNAME;Password=YOUR-PASSWORD;TrustServerCertificate=True;"
  }
}
```

### Step 2: Understand the Migration Process

**Migrations** are version-controlled SQL scripts that build/rebuild your database schema.

**Key Files:**
- `src/Common/Infrastructure/Persistence/GetToAnAnswerDbContext.cs` - Defines the database model
- `src/Common/Migrations/` - Contains all migration scripts (001_InitialCreate.cs, etc.)
- `src/Common/Migrations/GetToAnAnswerDbContextModelSnapshot.cs` - Current schema snapshot

**How Migrations Work:**

1. Developer makes changes to entities
2. Developer runs `Add-Migration MigrationName` (creates new migration file)
3. Developer runs `Update-Database` (applies migration to SQL Server)
4. Changes are committed to Git
5. Other developers run `Update-Database` to sync their local database

### Step 3: Run Entity Framework Migrations

Open **Package Manager Console** in Visual Studio:

1. **View** → **Other Windows** → **Package Manager Console**

2. Set the default project to **Common** (contains migrations):

```
Default project: Common
```

3. Apply all pending migrations to your local database:

```powershell
Update-Database
```

**Expected Output:**
```
Build started...
Build completed successfully
Applying migration '20240101000001_InitialCreate'
...
Done.
```

### What Migrations Create

The migrations create the following tables in SQL Server:

| Table | Purpose |
|-------|---------|
| `Questionnaires` | Main questionnaire records (title, description, status) |
| `Questions` | Individual questions within a questionnaire |
| `Answers` | Answer options for each question |
| `Contents` | Rich text content for various pages |
| `QuestionnaireVersions` | Version history of questionnaires |
| `__EFMigrationsHistory` | Entity Framework's internal migration tracking |

### Step 4: Verify Database Creation

Open SQL Server Management Studio or SQL Server Object Explorer in Visual Studio:

1. **View** → **SQL Server Object Explorer**
2. Expand **(localdb)\mssqllocaldb** → **Databases**
3. You should see **getToAnAnswer_dev** database
4. Expand it to see tables: Questionnaires, Questions, Answers, etc.

### Step 5: Seed Sample Data (Optional)

The project may include a seeding script. Check `src/Api/Program.cs`:

```csharp
// Look for a section like:
if (app.Environment.IsDevelopment())
{
	using (var scope = app.Services.CreateScope())
	{
		var db = scope.ServiceProvider.GetRequiredService<GetToAnAnswerDbContext>();
		db.Database.EnsureCreated();
		// SeedData.Initialize(db); // If seeding is available
	}
}
```

If seeding is available, sample data will be inserted when the API starts.

### Troubleshooting Migrations

**Error: "Cannot open database 'getToAnAnswer_dev'"**
```powershell
# Migrations create the database automatically, but if not:
Update-Database -Context GetToAnAnswerDbContext
```

**Error: "A network-related or instance-specific error occurred"**
- Verify SQL Server is running: `SqlLocalDB start "mssqllocaldb"`
- Check connection string spelling and server name

**Error: "Multiple DbContext types found"**
- Ensure **Common** project is set as default in Package Manager Console

**Need to Create Fresh Database?**
```powershell
# Drop and recreate (CAUTION: deletes all data)
Update-Database -Migration 0  # Removes all migrations
Update-Database               # Reapplies all migrations
```

---

## Part 3: Frontend & Admin Environments

The **Frontend** and **Admin** projects are ASP.NET Core Razor Pages applications. They don't directly access the database; instead, they make HTTP requests to the **API** project.

### Architecture Overview

```
┌──────────────────────────────────────────┐
│   User Browser                           │
├──────────────────────────────────────────┤
│   Frontend (Questionnaire Pages)         │  ← http://localhost:7204
│   Admin (Questionnaire Management)       │  ← http://localhost:7082
│   (Both MAKE HTTP CALLS TO)              │
└────────────────┬─────────────────────────┘
				 │ HTTP REST API Calls
				 ▼
┌──────────────────────────────────────────┐
│   API (Endpoints)                        │  ← http://localhost:7243
│   - GET /api/questionnaires              │
│   - POST /api/questions                  │
│   - GET /api/answers/{questionId}        │
└──────────────────────────────────────────┘
```

### Step 1: Set Up Frontend

#### 1.1: Configure API Base URL

The Frontend needs to know where the API is running.

**Option A: User Secrets (Recommended)**

```powershell
# Open PowerShell and navigate to Frontend project
cd src/Frontend

# Set the API base URL
dotnet user-secrets set "ApiSettings:BaseUrl" "http://localhost:7243"

# Verify it's set
dotnet user-secrets list
```

**Option B: appsettings.Local.json**

Create `src/Frontend/appsettings.Local.json`:

```json
{
  "ApiSettings": {
	"BaseUrl": "http://localhost:7243"
  },
  "BlobStorage": {
	"ConnectionString": "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite",
	"ContainerName": "localimages"
  }
}
```

### Step 2: Set Up Admin

#### 2.1: Configure Admin API URL

Just like Frontend, Admin needs the API base URL:

```powershell
cd src/Admin

dotnet user-secrets set "ApiSettings:BaseUrl" "http://localhost:7243"
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite"
dotnet user-secrets set "BlobStorage:ContainerName" "localimages"
```

Or create `src/Admin/appsettings.Local.json`:

```json
{
  "ApiSettings": {
	"BaseUrl": "http://localhost:7243"
  },
  "BlobStorage": {
	"ConnectionString": "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite",
	"ContainerName": "localimages"
  }
}
```

#### 2.2: Configure Azure AD (Local Mock)

The Admin app uses Azure AD for authentication. Locally, it uses a **mock** implementation.

In `src/Admin/Program.cs`, when running in **Local** environment:

```csharp
if (builderIsLocalEnvironment)  // Environment = "Local"
{
	builder.Services.AddMockAzureAdForMvc();  // Uses mock authentication
}
```

**To enable Local environment:**
- Create launch profile or set environment variable: `ASPNETCORE_ENVIRONMENT=Local`

### Step 3: Running the Applications Locally

#### Option A: Run from Visual Studio (Easiest)

1. **Set Startup Projects:**
   - Right-click Solution → **Properties**
   - Select **Startup Project** → **Multiple startup projects**
   - Set to start:
	 - ✅ **Api** (Port 7243)
	 - ✅ **Frontend** (Port 7204)
	 - ✅ **Admin** (Port 7082)

2. **Press F5** to start all three projects

#### Option B: Run from Command Line

**Terminal 1 - API:**
```powershell
cd src/Api
dotnet run --launch-profile https
```

**Terminal 2 - Frontend:**
```powershell
cd src/Frontend
dotnet run --launch-profile https
```

**Terminal 3 - Admin:**
```powershell
cd src/Admin
dotnet run --launch-profile https
```

#### Access the Applications

- **Frontend:** https://localhost:7204
- **Admin:** https://localhost:7082 (Mock login available)
- **API Docs:** https://localhost:5000/api/docs (if Swagger/Scalar is enabled)

### Step 4: Troubleshooting Frontend/Admin Setup

**Error: "Cannot connect to API"**
- Ensure API is running on http://localhost:7243
- Verify ApiSettings:BaseUrl is set correctly
- Check Network tab in browser (F12) for failed requests

**Error: "Port 7243 already in use"**
```powershell
# Find what's using port 7243
netstat -ano | findstr :7243

# Kill the process (replace XXXX with PID)
taskkill /PID XXXX /F
```

**Error: "TLS connection failed" / "Certificate error"**
- For local development, disable HTTPS certificate validation
- In `appsettings.Local.json`, ensure proper configuration

---

## Part 4: Integration Tests with Docker

### What Are Integration Tests?

Integration tests verify that multiple components work together correctly:
- API endpoints
- Database operations
- Authentication flows
- External service mocks (Azure AD via WireMock)

### How the Setup Works

```
┌────────────────────────────────────────────┐
│   Integration Tests (xUnit)                │
│   tests/Integration.Tests/                 │
└────────────┬─────────────────────────────┬─┘
			 │                             │
			 ▼                             ▼
	 ┌──────────────────┐        ┌─────────────────────┐
	 │  Docker Desktop  │        │  WireMock Server    │
	 │  (Testcontainers)│        │  (Fake Azure AD)    │
	 └────────┬─────────┘        └─────────────────────┘
			  │
			  ▼
	 ┌──────────────────┐
	 │ SQL Server (DB)  │
	 │ Running in       │
	 │ Container        │
	 └──────────────────┘
```

**Key Components:**

1. **Testcontainers:** Automatically spins up SQL Server in Docker
2. **WireMock:** Simulates Azure AD authentication server
3. **CustomWebApplicationFactory:** Hosts Admin/Api apps with test config

### Prerequisites: Docker Desktop Setup

#### Windows PC Setup

**Step 1: Install Docker Desktop**

1. Download [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
2. Run the installer
3. Choose "Install required Windows components for WSL 2" 
4. Restart your computer

**Step 2: Verify Docker Installation**

```powershell
docker --version
docker run hello-world
```

Expected output:
```
Hello from Docker!
```

**Step 3: Configure Docker for Testing**

Docker Desktop on Windows uses **WSL 2** backend by default (good for performance).

Verify:
- Docker Desktop → Settings → General → Check "Use WSL 2 based engine"

#### Mac PC Setup

**Step 1: Install Docker Desktop for Mac**

1. Download [Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
2. Install to `/Applications`
3. Launch Docker.app

**Step 2: Verify Installation**

```zsh
docker --version
docker run hello-world
```

### Differences Between Windows and Mac Setup

| Aspect | Windows | Mac |
|--------|---------|-----|
| **Backend** | WSL 2 or Hyper-V | Apple Silicon or Intel virtualization |
| **Port Binding** | Works normally | Works normally |
| **Volume Mounts** | May have permission issues | Generally smoother |
| **Performance** | Good with WSL 2 | Excellent on Apple Silicon |
| **Connection String** | `localhost` works | `host.docker.internal` for host access |

**Important for Mac:** If tests fail to connect to host services, use:
```
host.docker.internal:7243  # Instead of localhost:7243
```

### Step 1: Understanding Integration Test Structure

```
tests/Integration.Tests/
├── Fixture/
│   ├── AdminWithWireMockFixture.cs    ← Hosts Admin + WireMock
│   ├── ApiFixture.cs                  ← Hosts API + DB container
│   └── CustomWebApplicationFactory.cs ← Base test factory
├── Fake/
│   ├── WireMockAadServer.cs           ← Mock Azure AD
│   └── JwtTestTokenGenerator.cs       ← Creates fake JWT tokens
├── Auth/
│   └── AdminAuthenticationLifetimeTests.cs
├── Answer/
├── Questionnaire/
├── Question/
└── ModuleInitializer.cs               ← Docker config initialization
```

### Step 2: ModuleInitializer - Docker Configuration

The `ModuleInitializer.cs` runs before any tests and configures Docker connectivity:

```csharp
[ModuleInitializer]
public static void Initialize()
{
	var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");

	// Handles invalid Docker Desktop Linux Engine paths
	if (!string.IsNullOrEmpty(dockerHost))
	{
		if (dockerHost.Contains("dockerDesktopLinuxEngine") || 
			dockerHost.Contains("////pipe"))
		{
			// Clear invalid path, let Testcontainers auto-detect
			Environment.SetEnvironmentVariable("DOCKER_HOST", "");
		}
	}
}
```

**Why this matters:**
- Windows Docker Desktop sometimes sets malformed `DOCKER_HOST` paths
- This initialization auto-corrects them
- **No action needed** - it's automatic

### Step 3: Testcontainers Configuration

Testcontainers automatically downloads and runs SQL Server in Docker.

**First Run Setup:**

```powershell
# Navigate to test project
cd tests/Integration.Tests

# Run a test (first run downloads Docker image ~500MB)
dotnet test --filter "ClassName"
```

**First run output:**
```
Pulling image: mcr.microsoft.com/mssql/server:latest
Downloaded: 500MB
Container starting...
Database initialized...
Tests running...
```

**Subsequent runs:** Much faster (container cached)

### Step 4: Running Integration Tests

#### From Visual Studio Test Explorer

1. **Test** → **Test Explorer** (Ctrl+E, Ctrl+T)
2. See all tests in `tests/Integration.Tests`
3. Examples:
   - `AdminAuthenticationLifetimeTests`
   - `QuestionnaireTests`
   - `QuestionTests`
4. Right-click and **Run** or **Debug**

#### From Command Line

```powershell
# Navigate to test project
cd tests/Integration.Tests

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=AdminAuthenticationLifetimeTests"

# Run with verbose output
dotnet test --verbosity detailed

# Run with specific configuration
dotnet test --configuration Debug
```

#### Example: Running Authentication Tests

```powershell
cd tests/Integration.Tests
dotnet test --filter "ClassName=AdminAuthenticationLifetimeTests" --verbosity detailed
```

Expected output:
```
Starting test run
Configuring Testcontainers for SQL Server
Starting WireMock Azure AD server on port 54XXX
Running 5 tests...
  ✓ Test_Admin_Login_Success
  ✓ Test_Admin_Token_Refresh
  ✓ Test_Admin_Logout
...
Test run completed: 5 passed
```

### Step 5: Understanding WireMock in Tests

**WireMockAadServer** is a lightweight HTTP server that simulates Azure AD.

```csharp
public class WireMockAadServer
{
	public string BaseUrl { get; }        // http://localhost:54XXX
	public string TenantId { get; }       // "mock-tenant"
	public string Issuer { get; }         // http://localhost:.../mock-tenant/v2.0

	private void SetupDiscovery()         // /.well-known/openid-configuration
	private void SetupTokenEndpoint()     // /oauth2/v2.0/token
	private void SetupAuthorizeEndpoint() // /oauth2/v2.0/authorize
}
```

**Mocked Endpoints:**
- `/{tenantId}/.well-known/openid-configuration` - OpenID Connect discovery
- `/{tenantId}/oauth2/v2.0/token` - Token generation (returns signed JWT)
- `/{tenantId}/oauth2/v2.0/authorize` - Authorization endpoint
- `/{tenantId}/openid/userinfo` - User information

**How Tests Use It:**

```csharp
public class AdminWithWireMockFixture : WebApplicationFactory<Program>
{
	private WireMockAadServer _aad;

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		_aad = new WireMockAadServer();  // Start mock server

		builder.ConfigureAppConfiguration((ctx, config) =>
		{
			// Point Admin app to mock Azure AD
			config["AzureAd:Instance"] = $"{_aad.BaseUrl}/";
			config["AzureAd:Authority"] = $"{_aad.BaseUrl}/{_aad.TenantId}/v2.0";
		});
	}
}
```

### Step 6: Common Integration Test Patterns

#### Pattern 1: Testing API Endpoint with Database

```csharp
public class QuestionnaireTests : IClassFixture<ApiFixture>
{
	private readonly ApiFixture _fixture;

	public QuestionnaireTests(ApiFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task GetQuestionnaire_Returns_Success()
	{
		// Arrange
		var questionnaireId = Guid.NewGuid();
		var client = _fixture.Client;  // Already configured HTTP client

		// Act
		var response = await client.GetAsync($"/api/questionnaires/{questionnaireId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
```

#### Pattern 2: Testing with Mock Authentication

```csharp
public class AdminAuthenticationTests : IClassFixture<AdminWithWireMockFixture>
{
	private readonly AdminWithWireMockFixture _fixture;

	[Fact]
	public async Task Admin_Login_Success_With_Valid_Token()
	{
		// Arrange
		var client = _fixture.CreateClient();
		var token = JwtTestTokenGenerator.ValidJwtToken;

		// Act
		client.DefaultRequestHeaders.Authorization = 
			new AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync("/admin/manage-questionnaires");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}
```

### Troubleshooting Integration Tests

**Error: "Cannot connect to Docker daemon"**
```powershell
# Ensure Docker Desktop is running
docker ps

# If not running, start Docker Desktop or:
dockerd  # On Linux
```

**Error: "Port already in use"**
```powershell
# Testcontainers uses random free ports; this shouldn't happen
# But if it does, check Docker containers:
docker ps -a

# Stop conflicting container:
docker stop <container_id>
```

**Error: "Insufficient disk space"**
- SQL Server Docker image is ~500MB
- Ensure at least 2GB free space on disk
- Clean up old Docker images: `docker image prune`

**Error: "WireMock server failed to start"**
- Random port allocation failed
- Try: `docker system prune` to free resources
- Restart Docker Desktop

**Tests Very Slow on First Run?**
- Docker is downloading SQL Server image (~500MB)
- Subsequent runs use cached image
- First test run can take 2-3 minutes

**Mac-Specific: "Cannot connect to localhost in container"**
- Use `host.docker.internal` instead of `localhost`
- Check Docker Desktop → Resources → Network settings

---

## Part 5: End-to-End (E2E) Tests

End-to-End tests verify complete user workflows by simulating real browser interactions.

### What Are E2E Tests?

E2E tests automate user actions:
- Navigate to a page
- Fill in a form
- Click buttons
- Verify page content
- Check database changes

Example flow:
1. User clicks "Create Questionnaire"
2. Form appears
3. User fills name and description
4. User clicks "Save"
5. Redirect to questionnaire list
6. New questionnaire appears in list

### E2E Test Technology

The project uses **Playwright** (or **Selenium**) for browser automation:
- Launches real browser (Chrome, Firefox, Edge)
- Simulator user interactions
- Checks rendered HTML
- Verifies navigation

### Step 1: Check for E2E Test Project

Look for E2E test project:

```powershell
# From solution root
ls tests/  # List test projects
```

If you see `e2e-tests` or similar folder:

```
tests/
├── Integration.Tests/
├── Unit.Tests/
└── src/e2e/gtaa.e2etests  ← E2E tests (if exists)
```

### Step 2: Install E2E Test Dependencies

E2E tests use Playwright:

```powershell
# Navigate to e2e test folder
cd src/e2e/gtaa.e2etests

# Restore dependencies
dotnet restore

# Install Playwright browsers
pwsh ./bin/Debug/net10.0/playwright.ps1 install

# Or via Playwright CLI
playwright install
```

### Step 3: E2E Test Structure

```csharp
[Collection("E2E Tests")]
public class QuestionnaireUserWorkflow
{
	private IPage _page;
	private IBrowser _browser;

	[Fact]
	public async Task User_Can_Create_Questionnaire()
	{
		// Arrange
		_browser = await Playwright.Chromium.LaunchAsync();
		_page = await _browser.NewPageAsync();

		// Act - Navigate to Admin
		await _page.GotoAsync("https://localhost:5002");

		// Authenticate (mock or real)
		await _page.ClickAsync("text=Sign In");
		await _page.FillAsync("input[name='username']", "testuser");
		await _page.FillAsync("input[name='password']", "password");
		await _page.ClickAsync("button:has-text('Login')");

		// Create questionnaire
		await _page.ClickAsync("text=Create New");
		await _page.FillAsync("input[placeholder='Name']", "Test Questionnaire");
		await _page.FillAsync("textarea[name='description']", "Test Description");
		await _page.ClickAsync("button:has-text('Save')");

		// Assert
		var successMessage = await _page.TextContentAsync(".success");
		successMessage.Should().Contain("Created Successfully");

		// Cleanup
		await _browser.CloseAsync();
	}
}
```

### Step 4: Running E2E Tests Locally

#### Prerequisites

1. **All Applications Running:**
   - API on http://localhost:7243
   - Frontend on http://localhost:7204
   - Admin on http://localhost:7082

2. **Database Seeded with Test Data:**
   ```powershell
   # Ensure database has test questionnaires
   cd src/Api
   dotnet run
   ```

#### Run E2E Tests

```powershell
# Navigate to e2e test folder
cd tests/e2e

# Run all E2E tests
dotnet test

# Run specific test
dotnet test --filter "TestName=QuestionnaireUserWorkflow"

# Run with headed browser (see automation)
dotnet test -- --runner.head

# Run specific browser
dotnet test -- --runner.chrome
```

### Troubleshooting E2E Tests

**Error: "Browser launch failed"**
```powershell
# Ensure browsers are installed
playwright install

# Install all browsers
playwright install --with-deps
```

**Error: "Timeout waiting for element"**
- Increase timeout: `WaitForSelectorAsync(selector, new() { Timeout = 10000 })`
- Check if element selector is wrong: Use browser DevTools to inspect

**Error: "Application not responding"**
- Ensure all three apps are running
- Check ports: API (7243), Frontend (7204), Admin (7082)

**Error: "Element is not clickable"**
- Element might be hidden or covered by another element
- Use `WaitForVisibleAsync()` before clicking
- Scroll into view: `await element.ScrollIntoViewIfNeededAsync();`

---

## Troubleshooting

### General Troubleshooting Steps

#### Issue: "Cannot connect to database"

1. **Verify SQL Server is running:**
   ```powershell
   SqlLocalDB start "mssqllocaldb"
   ```

2. **Check connection string:**
   ```powershell
   # Via user secrets in Api project
   cd src/Api
   dotnet user-secrets list
   ```

3. **Test connection directly:**
   ```powershell
   sqlcmd -S (localdb)\mssqllocaldb -E
   ```

#### Issue: "Migration failed"

1. **Verify correct DbContext in migrations:**
   ```powershell
   cd src/Common
   Update-Database -Context GetToAnAnswerDbContext
   ```

2. **Check for model conflicts:**
   ```powershell
   Add-Migration CheckModel
   ```

3. **Reset database (CAREFUL - deletes all data):**
   ```powershell
   Update-Database -Migration 0
   Update-Database
   ```

#### Issue: "Port already in use"

```powershell
# Find process using port
netstat -ano | findstr :5000

# Kill process (taskkill for Windows, kill for Mac)
taskkill /PID <PID> /F  # Windows
kill -9 <PID>           # Mac
```

#### Issue: "Docker tests fail on Mac"

Use `host.docker.internal` instead of `localhost`:

```csharp
// In test configuration
var connectionString = "Server=host.docker.internal;Database=test;...";
```

#### Issue: "E2E Tests timeout on first run"

First playwright run downloads browsers (~500MB). This is normal and slow.
- Subsequent runs use cached browsers
- First run can take 5-10 minutes

#### Issue: "All tests fail, no specific error"

Try these in order:

1. **Clean build:**
   ```powershell
   dotnet clean
   dotnet build
   ```

2. **Restore packages:**
   ```powershell
   dotnet restore
   ```

3. **Delete bin/obj folders:**
   ```powershell
   Remove-Item -Recurse bin, obj
   dotnet build
   ```

4. **Check .NET version:**
   ```powershell
   dotnet --version  # Should be 10.x.x
   ```

### Quick Reference: Common Commands

```powershell
# SQL Server
SqlLocalDB start "mssqllocaldb"
SqlLocalDB stop "mssqllocaldb"

# Entity Framework Migrations
Update-Database                              # Apply all pending migrations
Update-Database -Migration 0                 # Revert all migrations
Add-Migration MigrationName                  # Create new migration
Remove-Migration                             # Remove last migration

# Running Applications
dotnet run                                   # Run from project folder
dotnet run --launch-profile https           # Run with HTTPS

# Running Tests
dotnet test                                 # Run all tests
dotnet test --filter "TestName"             # Run specific test
dotnet test --verbosity detailed            # Detailed output

# User Secrets
dotnet user-secrets init                    # Initialize user secrets
dotnet user-secrets set "key" "value"       # Set secret
dotnet user-secrets list                    # List all secrets
dotnet user-secrets remove "key"            # Remove secret

# Docker (Integration Tests)
docker ps                                   # List running containers
docker images                               # List downloaded images
docker system prune                         # Clean up unused resources
```

---

## Summary

Congratulations! You've successfully set up the GTAA project locally. Here's what you've accomplished:

✅ **Part 1:** Installed SQL Server LocalDB
✅ **Part 2:** Set connection string and ran Entity Framework migrations
✅ **Part 3:** Configured Frontend and Admin with API base URL and Blob Storage
✅ **Part 4:** Set up Docker and ran Integration tests with WireMock
✅ **Part 5:** Ran E2E tests to verify complete user workflows

### Next Steps for Development

1. **Review Code Structure:** Explore `src/` folder to understand project layout
2. **Understand Architecture:** Read about API services, database entities
3. **Make Your First Change:** Modify a service, run tests to verify
4. **Commit to Git:** Practice the development workflow
5. **Read Code Comments:** Many complex areas have detailed comments

### Resources

- **Microsoft Docs:** [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- **ASP.NET Docs:** [Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/)
- **Playwright:** [Playwright Testing Guide](https://playwright.dev/dotnet/)
- **Testcontainers:** [Testcontainers for .NET](https://testcontainers.com/modules/mssqlserver/)

---

## Document Version

- **Created:** 2026
- **Last Updated:** 2026
- **.NET Version:** 10.0
- **SQL Server:** 2019/2022 or LocalDB
- **Tested On:** Windows 10+, Windows 11, macOS 12+

For questions or updates, refer to the project's GitHub repository or team documentation.
