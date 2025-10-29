import {APIResponse} from "@playwright/test";

export async function parseBody(response: APIResponse) {
    const ct = (response.headers()['content-type'] || '').toLowerCase();

    try {
        const raw = await response.text();

        // If empty body, return null
        if (!raw || raw.trim() === '') {
            return null;
        }

        // If JSON content type, try to parse
        if (ct.includes('application/json')) {
            try {
                return JSON.parse(raw);
            } catch (parseError) {
                throw new Error(`Failed to parse JSON response: ${parseError instanceof Error ? parseError.message : parseError}\nRaw text: ${raw}`);
            }
        }

        // Return raw text for non-JSON responses
        return raw;
    } catch (error) {
        throw new Error(`Failed to read response body: ${error instanceof Error ? error.message : error}`);
    }
}
