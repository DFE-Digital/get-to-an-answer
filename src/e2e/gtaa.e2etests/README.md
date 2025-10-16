
### Installation

1. Clone the repository:

2. Install dependencies at gtaa.e2etests:

    yarn install

3. Install Playwright browsers:

    yarn playwright install

## Running Tests

### Run All Tests

yarn playwright test

### Run a Specific Test

yarn playwright test src/tests/example.spec.ts

### Run Tests in a Specific Browser

yarn playwright test --project=Chromium

### Run Tests in Debug Mode

yarn playwright test --debug

### Reporting and Debugging

### View Test Report

After running tests, generate and view an HTML report:

yarn playwright show-report

### Screenshots and Videos

Screenshots: Captured on failures.

Videos: Retained on test failures (configurable in playwright.config.ts).


### Configuration

Playwright Config (playwright.config.ts)

Key settings:

testDir: Directory containing test files.

timeout: Global timeout for each test.

retries: Number of retries on failure.

projects: Defines configurations for browsers and devices.

use: Default settings (e.g., trace, screenshot, video).

### Contribution Guidelines

1. Fork the repository.

2. Create a new branch for your feature/bug fix.

3. Commit changes with clear messages.

4. Submit a pull request for review.

### License

This project is licensed under the MIT License. See the LICENSE file for details.

### Support

For questions or issues, please open an issue on the repository or contact the maintainer.