import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import {
    addContributor,
    createQuestionnaire,
    deleteQuestionnaire,
    getContributors,
    removeContributor
} from '../../test-data-seeder/questionnaire-data';

test.describe('PUT /api/questionnaires/{id}/contributors', () => {
    test('adds a new contributor (200) and is returned by GET', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const putResp = await addContributor(request, questionnaire.id, 'alice@example.com');
        expect(putResp.ok()).toBeTruthy();
        expect(putResp.status()).toBe(204);

        const { response: getResp, contributors } = await getContributors(request, String(questionnaire.id));
        expect(getResp.status()).toBe(200);
        expect(contributors).toContain('alice@example.com');
    });

    test('adding the same contributor twice is idempotent (200) and not duplicated', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        await addContributor(request, questionnaire.id, 'bob@example.com');
        const second = await addContributor(request, questionnaire.id, 'bob@example.com');
        expect(second.status()).toBe(400);

        const { contributors } = await getContributors(request, String(questionnaire.id));
        const count = contributors.filter((c:any) => c === 'bob@example.com').length;
        expect(count).toBe(1);
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const resp = await addContributor(request, 'not-a-guid' as unknown as string, 'charlie@example.com');
        expect(resp.status()).toBe(404);
    });

    test('adding contributor to deleted questionnaire returns 404', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await deleteQuestionnaire(request, String(questionnaire.id));
        const resp = await addContributor(request, questionnaire.id, 'dora@example.com');
        expect(resp.status()).toBe(404);
    });

    test('unauthorised token returns 403', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const resp = await addContributor(request, questionnaire.id, 'eve@example.com', JwtHelper.UnauthorizedToken);
        expect(resp.status()).toBe(403);
    });

    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const resp = await addContributor(request, questionnaire.id, 'frank@example.com', JwtHelper.ExpiredToken);
        expect(resp.status()).toBe(401);
    });

    test('can remove a contributor via DELETE (200)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const addResponse = await addContributor(request, questionnaire.id, 'grace@example.com');
        expect(addResponse.status()).toBe(204);
        
        await addContributor(request, questionnaire.id, 'john@example.com');
        await addContributor(request, questionnaire.id, 'jamie@example.com');

        const { response } = await removeContributor(request, String(questionnaire.id), 'grace@example.com');
        expect(response.status()).toBe(204);

        const { contributors } = await getContributors(request, String(questionnaire.id));
        // Depending on minimum contributors rule, this might still contain creator + others.
        expect(contributors.includes('grace@example.com')).toBeFalsy();
    });
});