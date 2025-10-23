# End-to-End (E2E) Testing (In-progress)

Run Playwright-based E2E tests against services started with Docker Compose. The `compose-ci.yaml` file brings up the API and SQL Server; the API waits until SQL is ready.

## Prerequisites
- Docker and Docker Compose
- Copy `Infrastructure/.env.example` to `Infrastructure/.env` and provide valid secrets/connection strings
- Keep `.env.example` updated whenever `.env` changes
- If you change the SQL Server service name in `compose-ci.yaml`, update the `Server` value in connection strings in `.env` and relevant GitHub Actions secrets
- Ensure API and test projects are built if required by your workflow

## Installation
1. Clone the repository
2. From `src/e2e/gtaa.e2etests`:
   ```sh
   yarn install
   yarn playwright install
   ```

## Running E2E Tests (in Docker  environment)
1. Start the environment:
   ```sh
   docker compose -f compose-ci.yaml up --build
   ```
   API and SQL containers are built and started; API waits for SQL.


2. Run tests:
   - Via Docker Compose: add your test runner (e.g., Playwright) as a service in `compose-ci.yaml` to run automatically against the API container.
   - From your machine (point to the API port, e.g., `http://localhost:8080`):
     ```sh
     # all tests
     yarn playwright test
     # specific test
     yarn playwright test src/tests/example.spec.ts
     # specific browser
     yarn playwright test --project=Chromium
     # debug mode
     yarn playwright test --debug
     ```

3. Tear down:
   ```sh
   docker compose -f compose-ci.yaml down -v
   ```
   The `-v` flag removes volumes for a clean state.

## Reporting and Artifacts
- HTML report:
  ```sh
  yarn playwright show-report
  ```
- Screenshots: captured on failures
- Videos: retained on failures (configurable in `playwright.config.ts`)

## Configuration
Key `playwright.config.ts` settings:
- `testDir`: where tests live
- `timeout`: per-test timeout
- `retries`: retry count on failure
- `projects`: browser/device configs
- `use`: defaults (e.g., trace, screenshot, video)

## Debugging
- Compose logs:
  ```sh
  docker compose -f compose-ci.yaml logs
  ```
- For .NET build/runtime issues: build and run/debug locally, or attach a debugger to the container

## Notes
- Keep `.env` up to date and out of source control
- Extend `compose-ci.yaml` with additional dependencies or test runners as needed

## Contribution
1. Fork and create a feature/bugfix branch
2. Commit with clear messages
3. Open a pull request for review

## License
MIT. See the `LICENSE` file.

## Support
Open an issue or contact the maintainer.
