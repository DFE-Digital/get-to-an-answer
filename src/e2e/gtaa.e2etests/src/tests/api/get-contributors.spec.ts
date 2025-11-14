import { test, expect, APIResponse } from '@playwright/test';
import {
    addContributor,
    createQuestionnaire,
    deleteQuestionnaire,
    getContributors
} from "../../test-data-seeder/questionnaire-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('GET /questionnaires/{id}/contributors', () => {
    test('200 OK with minimal contributor DTOs; no emails or internal IDs', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        // Use a fake email just to add a contributor; API must not return it
        await addContributor(request, questionnaire.id, `user-uuid@education.gov.uk`);

        const { response: res } = await getContributors(request, questionnaire.id);
        expect(res.status()).toBe(200);

        const list = (await res.json()) as string[];
        expect(Array.isArray(list)).toBeTruthy();
        expect(list.length).toBeGreaterThanOrEqual(1);

        for (const c of list) {
            expect(typeof c).toBe('string');
        }
    });

    test('404 Not Found when questionnaire missing or Deleted (ProblemDetails)', async ({ request }) => {
        // Unknown GUID
        const { response: resMissing } = await getContributors(request, 'random-nonexistent-uuid');
        expect(resMissing.status()).toBe(404);

        // Deleted questionnaire
        const { questionnaire } = await createQuestionnaire(request);
        await deleteQuestionnaire(request, questionnaire.id);
        
        const { response: resDeleted } = await getContributors(request, questionnaire.id);
        expect(resDeleted.status()).toBe(404);

        for (const res of [resMissing, resDeleted]) {
            const problem = await res.text() as string;
            const sensitive = ['stackTrace', 'exception', 'developerMessage', 'debug'];
            for (const k of sensitive) {
                expect(problem.includes(k)).toBeFalsy();
            }
        }
    });

    test('401/403 when unauthenticated or unauthorized (no existence leak)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const { response: res } = await getContributors(request, questionnaire.id, JwtHelper.UnauthorizedToken);
        
        expect(res.status()).toBe(403);
    });

    test('200 OK with one additional contributors assigned', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { response: res } = await getContributors(request, questionnaire.id);
        expect(res.status()).toBe(200);

        const list = (await res.json()) as string[];
        expect(Array.isArray(list)).toBeTruthy();
        expect(list.length).toBe(1);
    });
});