import { test, expect } from '@playwright/test';
import {JwtHelper} from "../../helpers/JwtHelper";
import {EnvConfig} from "../../config/environment-config";

test.describe.configure({ retries: 5 });

test.describe('Health checks', () => {
  test('[API] GET /health should return 200 and a non-empty body', async ({ request }) => {
    const base = getApiBaseUrl();
    if (!base) {
      // Skip this test when the API URL is not provided in the environment.
      test.skip(true, 'API base URL not set in environment (API_URL)');
      return;
    }

    const url = `${base}/health`;

    const res = await request.get(url);
    expect(res.status(), `Expected 200 from ${url}, got ${res.status()}`).toBe(200);

    await assertResponseHasBody(res, url);
  });
    test('[ADMIN] GET /dev/login should return 200 and a non-empty body', async ({ request }) => {
        const base = getAdminBaseUrl();
        if (!base) {
            // Skip this test when the API URL is not provided in the environment.
            test.skip(true, 'ADMIN base URL not set in environment (ADMIN_URL)');
            return;
        }

        const url = `${base}/dev/login?jt=${JwtHelper.NoRecordsToken()}`;

        const res = await request.get(url);
        expect(res.status(), `Expected 200 from ${url}, got ${res.status()}`).toBe(200);

        await assertResponseHasBody(res, url);
    });

    test('[FE] GET / should return 200 and a non-empty body', async ({ request }) => {
        const base = getAdminBaseUrl();
        if (!base) {
            // Skip this test when the API URL is not provided in the environment.
            test.skip(true, 'ADMIN base URL not set in environment (FE_URL)');
            return;
        }

        const url = `${base}/`;

        const res = await request.get(url);
        expect(res.status(), `Expected 200 from ${url}, got ${res.status()}`).toBe(200);

        await assertResponseHasBody(res, url);
    });
});

function getApiBaseUrl(): string | null {
    const raw = EnvConfig.API_URL;
    return raw ? raw.replace(/\/+$/, '') : null;
}

function getAdminBaseUrl(): string | null {
    const raw = EnvConfig.ADMIN_URL;
    return raw ? raw.replace(/\/+$/, '') : null;
}

function getFrontendBaseUrl(): string | null {
    const raw = EnvConfig.FE_URL;
    return raw ? raw.replace(/\/+$/, '') : null;
}

async function assertResponseHasBody(res: any, url: string): Promise<void> {
    const contentType = (res.headers()['content-type'] || '').toLowerCase();
    if (contentType.includes('application/json')) {
        const body = await res.json();
        expect(body).toBeTruthy();
    } else {
        const text = await res.text();
        expect(text.length).toBeGreaterThan(0);
    }
}
