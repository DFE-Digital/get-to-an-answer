import { expect, test } from '@playwright/test';
import { JwtHelper } from '../../helpers/JwtHelper';
import {
    createQuestionnaire,
    getQuestionnaire, postClone
} from '../../test-data-seeder/questionnaire-data';
import {
    createQuestion,
    listQuestions,
} from '../../test-data-seeder/question-data';
import {
    listAnswers,
    createAnswer
} from '../../test-data-seeder/answer-data';
import { QuestionType } from '../../constants/test-data-constants';


test.describe('POST /api/questionnaires/{id}/clones', () => {
    test('clones questionnaire (201) with new id and copies questions/answers', async ({ request }) => {
        // Arrange original questionnaire with 2 questions and answers
        const { questionnaire } = await createQuestionnaire(request);
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.SINGLE);
        const { question: q2 } = await createQuestion(request, questionnaire.id, undefined, 'Q2', QuestionType.MULTIPLE);

        await createAnswer(request, questionnaire.id, q1.id, 'A1', 'D1', 1);
        await createAnswer(request, questionnaire.id, q1.id, 'A2', 'D2', 2);
        await createAnswer(request, questionnaire.id, q2.id, 'B1', 'E1', 1);

        // Act
        const res = await postClone(request, questionnaire.id, { title: 'Cloned Q' });
        expect(res.status()).toBe(201);
        const cloned = await res.json();

        // New id and requested title
        expect(cloned.id).toBeTruthy();
        expect(cloned.id).not.toBe(questionnaire.id);
        expect(cloned.title).toBe('Cloned Q');

        // Assert cloned questions count and content
        const { questionGetBody: clonedQuestions } = await listQuestions(request, cloned.id);
        expect(clonedQuestions.length).toBe(2);
        const cq1 = clonedQuestions.find((x: any) => x.content === 'Q1');
        const cq2 = clonedQuestions.find((x: any) => x.content === 'Q2');
        expect(cq1).toBeTruthy();
        expect(cq2).toBeTruthy();

        // Answers cloned for each corresponding question
        const { answers: cq1Answers } = await listAnswers(request, cq1.id);
        const { answers: cq2Answers } = await listAnswers(request, cq2.id);
        expect(cq1Answers.length).toBe(2);
        expect(cq2Answers.length).toBe(1);

        // Original remains unchanged
        const { questionGetBody: originalQuestions } = await listQuestions(request, questionnaire.id);
        expect(originalQuestions.length).toBe(2);
    });

    
    test('unauthorized user cannot clone (401/403)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request, JwtHelper.ValidToken);

        const res = await postClone(request, questionnaire.id, { title: 'Clone' }, JwtHelper.UnauthorizedToken);
        expect([401, 403]).toContain(res.status());

        // Original still accessible to owner
        const { questionnaireGetResponse } = await getQuestionnaire(request, questionnaire.id, JwtHelper.ValidToken);
        expect(questionnaireGetResponse.status()).toBe(200);
    });

    
    test('invalid token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const res = await postClone(request, questionnaire.id, { title: 'Clone' }, JwtHelper.InvalidToken);
        expect(res.status()).toBe(401);
    });

    
    test('expired token returns 401', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const res = await postClone(request, questionnaire.id, { title: 'Clone' }, JwtHelper.ExpiredToken);
        expect(res.status()).toBe(401);
    });

    
    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const res = await postClone(request, 'not-a-guid', { title: 'Clone' });
        expect(res.status()).toBe(404);
    });

    
    test('non-existent questionnaire returns 404', async ({ request }) => {
        const res = await postClone(request, '00000000-0000-0000-0000-000000000000', { title: 'Clone' });
        expect(res.status()).toBe(404);
    });
});