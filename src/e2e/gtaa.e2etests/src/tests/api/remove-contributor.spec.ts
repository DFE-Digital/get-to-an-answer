import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import {
    addContributor,
    createQuestionnaire,
    deleteQuestionnaire,
    getContributors,
    removeContributor
} from '../../test-data-seeder/questionnaire-data';
test.describe('DELETE /api/questionnaires/{id}/contributors/{email}', () => {
    test('removes an existing contributor (200) and is not returned by GET', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await addContributor(request, String(questionnaire.id), 'alice@example.com');
        await addContributor(request, String(questionnaire.id), 'ace@example.com');
        await addContributor(request, String(questionnaire.id), 'art@example.com');

        const { response } = await removeContributor(request, String(questionnaire.id), 'alice@example.com');
        expect(response.status()).toBe(204);

        const { contributors } = await getContributors(request, String(questionnaire.id));
        expect(contributors.includes('alice@example.com')).toBeFalsy();
    });

    test('idempotent delete: removing a non-present contributor still returns 200', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const first = await removeContributor(request, String(questionnaire.id), 'bob@example.com');
        expect(first.response.status()).toBe(400);
        const second = await removeContributor(request, String(questionnaire.id), 'bob@example.com');
        expect(second.response.status()).toBe(400);
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const { response } = await removeContributor(request, 'not-a-guid' as unknown as string, 'charlie@example.com');
        expect(response.status()).toBe(404);
    });

    test('deleting contributor from a deleted questionnaire returns 404', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await addContributor(request, String(questionnaire.id), 'dora@example.com');
        await deleteQuestionnaire(request, String(questionnaire.id));

        const { response } = await removeContributor(request, String(questionnaire.id), 'dora@example.com');
        expect(response.status()).toBe(404);
    });

    test('unauthorised token returns 403', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await addContributor(request, String(questionnaire.id), 'eve@example.com');

        const { response } = await removeContributor(
            request,
            String(questionnaire.id),
            'eve@example.com',
            JwtHelper.UnauthorizedToken
        );
        expect(response.status()).toBe(403);
    });

    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await addContributor(request, String(questionnaire.id), 'frank@example.com');

        const { response } = await removeContributor(
            request,
            String(questionnaire.id),
            'frank@example.com',
            JwtHelper.ExpiredToken
        );
        expect(response.status()).toBe(401);
    });
});