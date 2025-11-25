import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import {AnswerDestinationType, EntityStatus, QuestionType} from '../../constants/test-data-constants';
import {
    createQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire,
    getQuestionnaire, getInitialState
} from '../../test-data-seeder/questionnaire-data';
import { createQuestion } from '../../test-data-seeder/question-data';
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

test.describe('GET /api/questionnaires/{questionnaireId}/initial-state', () => {
    test('returns initial question for published questionnaire (200)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        // seed at least one question to allow publish
        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Start Q', QuestionType.SingleSelect);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })
        
        await publishQuestionnaire(request, questionnaire.id);

        const { response: res } = await getInitialState(request, questionnaire.id, false);
        expect(res.status()).toBe(200);

        const body = await res.json();
        expect(body).toBeTruthy();
        expect(body.id).toBeTruthy();
        expect(body.content).toBeTruthy();
    });

    test('preview=true returns initial question for draft questionnaire (200)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await createQuestion(request, questionnaire.id, undefined, 'Draft Q', QuestionType.MultiSelect);

        // Ensure not published
        await updateQuestionnaire(request, questionnaire.id, { title: 'Remain Draft' });
        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect([EntityStatus.Draft, EntityStatus.Private]).toContain(questionnaireGetBody.status);

        const { response: res } = await getInitialState(request, questionnaire.id, true)
        expect(res.status()).toBe(200);

        const body = await res.json();
        expect(body.id).toBeTruthy();
        expect(body.content).toBeTruthy();
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const { response: res } = await getInitialState(request, 'not-a-guid')
        expect(res.status()).toBe(404);
    });

    test('non-existent questionnaire returns 404', async ({ request }) => {
        const missingId = '00000000-0000-0000-0000-000000000000';
        const { response: res } = await getInitialState(request, missingId)
        expect(res.status()).toBe(404);
    });

    test('unpublished questionnaire without preview returns 400/404', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await createQuestion(request, questionnaire.id, undefined, 'Q', QuestionType.SingleSelect);

        const { response: res } = await getInitialState(request, questionnaire.id, false)
        expect([400, 404]).toContain(res.status());
    });
});