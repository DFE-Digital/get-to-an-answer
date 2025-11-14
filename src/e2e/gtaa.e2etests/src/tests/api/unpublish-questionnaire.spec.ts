import { expect, test } from '@playwright/test';
import {
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire,
} from '../../test-data-seeder/questionnaire-data';
import { JwtHelper } from '../../helpers/JwtHelper';
import { EntityStatus, QuestionType } from '../../constants/test-data-constants';
import { createQuestion } from '../../test-data-seeder/question-data';

test.describe('PATCH Unpublish questionnaire api request', () => {
    test('unpublishes a published questionnaire (204) and status becomes Private', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Q', QuestionType.MULTIPLE, undefined);
        await publishQuestionnaire(request, questionnaire.id);

        const response = await request.patch(
            `${process.env.API_URL}/api/questionnaires/${questionnaire.id}?action=Unpublish`,
            {
                data: {},
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${JwtHelper.ValidToken}`,
                },
            }
        );

        expect(response.status()).toBe(204);

        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetBody.status).toBe(EntityStatus.Private);
    });

    test('unpublishing a draft questionnaire returns 204 but remains Private/Draft', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const response = await request.patch(
            `${process.env.API_URL}/api/questionnaires/${questionnaire.id}?action=Unpublish`,
            {
                data: {},
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${JwtHelper.ValidToken}`,
                },
            }
        );

        expect(response.status()).toBe(204);

        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        // Controller delegates to service which sets status to Private on unpublish path.
        expect([EntityStatus.Private, EntityStatus.Draft]).toContain(questionnaireGetBody.status);
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const response = await request.patch(
            `${process.env.API_URL}/api/questionnaires/not-a-guid?action=Unpublish`,
            {
                data: {},
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${JwtHelper.ValidToken}`,
                },
            }
        );

        expect(response.status()).toBe(404);
    });

    test('unauthorised token returns 403', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const response = await request.patch(
            `${process.env.API_URL}/api/questionnaires/${questionnaire.id}?action=Unpublish`,
            {
                data: {},
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${JwtHelper.UnauthorizedToken}`,
                },
            }
        );

        expect(response.status()).toBe(403);
    });

    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const response = await request.patch(
            `${process.env.API_URL}/api/questionnaires/${questionnaire.id}?action=Unpublish`,
            {
                data: {},
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${JwtHelper.ExpiredToken}`,
                },
            }
        );

        expect(response.status()).toBe(401);
    });
});