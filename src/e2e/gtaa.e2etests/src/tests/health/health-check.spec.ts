import { test, expect } from '@playwright/test';

test.describe.configure({ retries: 5 });

test.describe('Health checks', () => {
  test('GET /scalar should return 200 and a non-empty body', async ({ request }) => {
    const base = getApiBaseUrl();
    if (!base) {
      // Skip this test when the API URL is not provided in the environment.
      test.skip(true, 'API base URL not set in environment (API_URL)');
      return;
    }

    const url = `${base}/scalar`;

    const res = await request.get(url);
    expect(res.status(), `Expected 200 from ${url}, got ${res.status()}`).toBe(200);

    await assertResponseHasBody(res, url);
  });
});

function getApiBaseUrl(): string | null {
    const raw = process.env.API_URL;
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
