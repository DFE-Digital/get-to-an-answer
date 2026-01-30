import { expect, test } from '@playwright/test';
import {
    addContributor,
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire, unpublishQuestionnaire, updateQuestionnaire,
} from '../../test-data-seeder/questionnaire-data';
import { JwtHelper } from '../../helpers/JwtHelper';
import {AnswerDestinationType, EntityStatus, QuestionType} from '../../constants/test-data-constants';
import { createQuestion } from '../../test-data-seeder/question-data';
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

test.describe('PATCH Unpublish questionnaire api request', () => {
    test('unpublishes a published questionnaire (204) and status becomes Draft but flagged IsUnpublished', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        
        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaire.id);

        expect(publishResponse.status()).toBe(204);

        const { questionnaireGetBody: publishedQuestionnaire } = await getQuestionnaire(request, questionnaire.id);

        expect(publishedQuestionnaire.status).toBe(EntityStatus.Published);

        const { response } = await unpublishQuestionnaire(request, questionnaire.id);

        expect(response.status()).toBe(204);

        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetBody.status).toBe(EntityStatus.Draft);
        expect(questionnaireGetBody.isUnpublished).toBeTruthy();
    });

    test('unpublishing a draft questionnaire returns 400 and in the same status', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        
        const status = questionnaire.status;

        const { response } = await unpublishQuestionnaire(request, questionnaire.id);

        expect(response.status()).toBe(400);

        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        
        expect(questionnaireGetBody.status).toBe(status);
    });

    test('unpublishing a draft questionnaire (after previous publish) returns 204 and in the same status', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })

        // Publish the questionnaire
        const { response: publishResponse } = await publishQuestionnaire(request, questionnaire.id);

        expect(publishResponse.status()).toBe(204);

        const { questionnaireGetBody: publishedQuestionnaire } = await getQuestionnaire(request, questionnaire.id);

        expect(publishedQuestionnaire.status).toBe(EntityStatus.Published);

        // Edit the questionnaire after publish to make it a draft again
        await updateQuestionnaire(request, questionnaire.id, { title: 'Draft Q' });

        const { questionnaireGetBody: draftQuestionnaire } = await getQuestionnaire(request, questionnaire.id);

        expect(draftQuestionnaire.status).toBe(EntityStatus.Draft);

        // Unpublish the draft questionnaire (that was previously published)
        const { response } = await unpublishQuestionnaire(request, questionnaire.id);

        expect(response.status()).toBe(204);

        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetBody.status).toBe(EntityStatus.Draft);
        expect(questionnaireGetBody.isUnpublished).toBeTruthy();
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const { response } = await unpublishQuestionnaire(request, 'not-a-guid');

        expect(response.status()).toBe(404);
    });

    test('unauthorised token returns 403', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { response } = await unpublishQuestionnaire(request, questionnaire.id, JwtHelper.UnauthorizedToken);

        expect(response.status()).toBe(403);
    });

    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { response } = await unpublishQuestionnaire(request, questionnaire.id, JwtHelper.ExpiredToken);

        expect(response.status()).toBe(401);
    });
});