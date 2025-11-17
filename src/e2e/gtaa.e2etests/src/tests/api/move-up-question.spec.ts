import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import { createQuestionnaire } from '../../test-data-seeder/questionnaire-data';
import {createQuestion, getQuestion, listQuestions, moveUpQuestion} from '../../test-data-seeder/question-data';
import { QuestionType } from '../../constants/test-data-constants';
test.describe('PATCH /api/questionnaires/{questionnaireId}/questions/{questionId}?action=MoveUp', () => {
    test('moves a middle question up by one (204) and orders reflect change', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.MULTIPLE);
        const { question: q2 } = await createQuestion(request, questionnaire.id, undefined, 'Q2', QuestionType.MULTIPLE);
        const { question: q3 } = await createQuestion(request, questionnaire.id, undefined, 'Q3', QuestionType.MULTIPLE);

        // Move Q2 up -> it should swap with Q1
        const res = await moveUpQuestion(request, questionnaire.id, q2.id);
        expect(res.status()).toBe(204);

        const { questionGetBody: questions } = await listQuestions(request, questionnaire.id);
        const byId: Record<string, any> = {};
        for (const q of questions) byId[q.id] = q;

        expect(byId[q2.id].order).toBe(1);
        expect(byId[q1.id].order).toBe(2);
        expect(byId[q3.id].order).toBe(3);
    });

    test('moving the first question up returns 400', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { question: first } = await createQuestion(request, questionnaire.id, undefined, 'First', QuestionType.SINGLE);

        const res = await moveUpQuestion(request, questionnaire.id, first.id);
        expect(res.status()).toBe(400);
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const res = await moveUpQuestion(request, 'not-a-guid', 'not-a-guid');
        expect(res.status()).toBe(404);
    });

    test('non-existent ids return 404', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const missing = '00000000-0000-0000-0000-000000000000';

        const res = await moveUpQuestion(request, questionnaire.id, missing);
        expect(res.status()).toBe(404);
    });

    test('invalid token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Q', QuestionType.SINGLE);

        const res = await moveUpQuestion(request, questionnaire.id, question.id, JwtHelper.InvalidToken);
        expect(res.status()).toBe(401);
    });

    test('unauthorised user returns 403/404 and does not change order', async ({ request }) => {
        const owner = JwtHelper.ValidToken;
        const other = JwtHelper.UnauthorizedToken;

        const { questionnaire } = await createQuestionnaire(request, owner);
        const { question: q1 } = await createQuestion(request, questionnaire.id, owner, 'Q1', QuestionType.SINGLE);
        const { question: q2 } = await createQuestion(request, questionnaire.id, owner, 'Q2', QuestionType.SINGLE);

        const res = await moveUpQuestion(request, questionnaire.id, q2.id, other);
        expect([403, 404]).toContain(res.status());

        const { questionGetBody: questions } = await listQuestions(request, questionnaire.id, owner);
        const byId: Record<string, any> = {};
        for (const q of questions) byId[q.id] = q;

        expect(byId[q1.id].order).toBe(1);
        expect(byId[q2.id].order).toBe(2);
    });

    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Q', QuestionType.SINGLE);

        const res = await moveUpQuestion(request, questionnaire.id, question.id, JwtHelper.ExpiredToken);
        expect(res.status()).toBe(401);
    });
});