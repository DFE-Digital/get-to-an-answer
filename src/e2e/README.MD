# End-to-End (E2E) Testing

Run Playwright-based E2E tests against services started with Docker Compose. The `compose-ci.yaml` file brings up the
API and SQL Server; the API waits until SQL is ready.

## Prerequisites

- Docker and Docker Compose
- Copy `Infrastructure/.env.example` to `Infrastructure/.env` and provide valid secrets/connection strings for DB & API
- Keep `.env.example` updated whenever `.env` changes
- If you change the SQL Server service name in `compose-ci.yaml`, update the `Server` value in connection strings in
  `.env` and relevant GitHub Actions secrets for the E2E tests
- Ensure API and test projects are built if required by your workflow

## Installation

1. Clone the repository
2. Change directory to `src/e2e/gtaa.e2etests`:

   ```sh
   yarn install
   yarn playwright install
   ```

## Spinning up the Environment (API & SQL Database)

1. As mentioned in the Prerequisites, copy `src/Infrastructure/.env.example` to `src/Infrastructure/.env` and provide
   valid secrets/connection strings for DB & API.


2. Start the environment with Docker Compose:
   ```sh
   docker compose -f compose-ci.yaml up --build
   ```
   API and SQL containers are built and started; API waits for SQL.


3. Tear down:
   ```sh
   docker compose -f compose-ci.yaml down -v
   ```
   Note: **The `-v` flag removes volumes for a clean state.**

## Running the Tests from your local machine

1. Change directory to `src/e2e/gtaa.e2etests`, modify the `.env` to point to the correct API URL (e.g.,
   `http://localhost:8080` for Docker Compose network).
2. Run tests (commands):

```sh
     # all tests
     yarn playwright test
```

```sh
     # specific test
     yarn playwright test src/tests/example.spec.ts
```

```sh
     # specific browser
     yarn playwright test --project=Chromium   
```

```sh
    # debug mode
    yarn playwright test --debug
```

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
