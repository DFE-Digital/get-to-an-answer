# End-to-End (E2E) Test Setup (In-progress)

This project uses Docker Compose to run end-to-end (E2E) tests in a consistent environment. The `compose-ci.yaml` file spins up the required services (API and SQL Server) with the correct configuration for integration and E2E testing.

## Prerequisites
- Docker and Docker Compose installed
- Copy `.env.example` to `.env` in the `Infrastructure` folder and update it with valid connection strings and secrets
- The API and test projects built (if required by your workflow)
- **If any updates are made to the `.env` file, please update the `.env.example` file accordingly.**\
- If the SQL Server "service name" is changed in `compose-ci.yaml`, ensure to update the "Server" field in the connection strings within the `.env` file and also secrets in Github Actions workflows.

## Running E2E Tests (to be confirmed)

1. **Start the test environment:**
   ```sh
   docker compose -f compose-ci.yaml up --build
   ```
   This will build and start the API and SQL Server containers. The API will wait for SQL Server to be ready before starting.

2. **Run your E2E tests:**
   - You can run E2E tests in two ways:
     - **From Docker Compose:** Add your E2E test runner (e.g., Playwright) as a service in `compose-ci.yaml`. The tests will run automatically against the API container when the stack is up.
     - **From your local machine:** Point your local Playwright (or other test runner) to the API container's exposed port (e.g., `http://localhost:8080`) after the environment is started.
   - If your E2E tests are run from your host machine, execute them after the services are up and healthy. For example:
     ```sh
     # Example: using Playwright command
     yarn playwright test <directory-or-file> -c playwright.config.ts
     ```

3. **Tear down the environment after tests:**
   ```sh
   docker compose -f compose-ci.yaml down -v
   ```
   The `-v` flag removes any volumes, ensuring a clean state for the next run.

## Notes
- Ensure your `.env` file is up to date and not checked into source control.
- The API service uses a wait-for-db entrypoint to ensure it only starts after SQL Server is ready.
- You can extend `compose-ci.yaml` to add other dependencies or test runners as needed.

---

For more details, see the main project README or contact the maintainers.
