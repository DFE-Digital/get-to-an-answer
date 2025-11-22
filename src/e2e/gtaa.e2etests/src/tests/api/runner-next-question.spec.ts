import {expect, test} from '@playwright/test';
import {AnswerDestinationType, EntityStatus, QuestionType} from '../../constants/test-data-constants';
import {
    createQuestionnaire, getNextState,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {createAnswer} from '../../test-data-seeder/answer-data';

test.describe('POST /api/questionnaires/{questionnaireId}/next-state', () => {
    test('returns next destination for a valid published questionnaire path (200)', async ({ request }) => {
        // Create questionnaire -> 2 questions -> publish -> call next-state
        const { questionnaire } = await createQuestionnaire(request);
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.MULTIPLE);
        const { question: q2 } = await createQuestion(request, questionnaire.id, undefined, 'Q2', QuestionType.SINGLE);

        // Link q1 -> q2 via answer destination
        const {answer} = await createAnswer(request, questionnaire.id, q1.id, 'GoNext', undefined, 1, AnswerDestinationType.Question, q2.id);
        await createAnswer(request, questionnaire.id, q2.id, 'GoNext', undefined, 1, AnswerDestinationType.ExternalLink, undefined, 'https://example.org/help');

        // Must be published to use runner endpoints (non-preview)
        await publishQuestionnaire(request, questionnaire.id);

        const payload = {
            currentQuestionId: q1.id,
            selectedAnswerIds: [answer.id], 
            selectedAnswerId: null  // runner model often accepts both shapes; backend will pick what's relevant
        };

        const { response: res } = await getNextState(request, questionnaire.id, payload);

        const body = await res.json();

        expect(res.status()).toBe(200);
        expect(body).toBeTruthy();
        // Expect the destination to reference q2
        expect(body.question.id).toBe(q2.id);
    });

    test('preview=true works on draft questionnaire (200)', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.SINGLE);
        const { question: q2 } = await createQuestion(request, questionnaire.id, undefined, 'Q2', QuestionType.SINGLE);

        const { answer } = await createAnswer(request, questionnaire.id, q1.id, 'Next', undefined, 1, AnswerDestinationType.Question, q2.id);

        // Ensure draft
        await updateQuestionnaire(request, questionnaire.id, { title: 'Keep Draft' });
        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect([EntityStatus.Draft, EntityStatus.Private]).toContain(questionnaireGetBody.status);
        
        const { response: res } = await getNextState(request, questionnaire.id, { 
            currentQuestionId: q1.id, selectedAnswerId: null, selectedAnswerIds: [answer.id] 
        }, true)
        
        const body = await res.json();

        expect(res.status()).toBe(200);
        expect(body.question.id).toBe(q2.id);
    });

    test('invalid questionnaire id format returns 404', async ({ request }) => {
        const { response: res } =await getNextState(request, 'not-a-guid', { 
            currentQuestionId: 'not-a-guid', selectedAnswerIds: [] 
        });

        expect(res.status()).toBe(404);
    });

    test('non-existent questionnaire returns 404', async ({ request }) => {
        const missingId = '00000000-0000-0000-0000-000000000000';
        const { response: res } =await getNextState(request, missingId, {
            currentQuestionId: missingId, selectedAnswerIds: [missingId]
        });
        
        const body = await res.json();

        expect(res.status()).toBe(404);
    });

    test('missing currentQuestionId returns 400', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { response: res } =await getNextState(request, questionnaire.id, {
            /* currentQuestionId missing */ selectedAnswerIds: []
        });

        expect([400, 422]).toContain(res.status());
    });

    test('handles external link destination (200) with url in response', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.SINGLE);

        const {answer} = await createAnswer(request, questionnaire.id, q1.id, 'Go External', undefined, 1,
            AnswerDestinationType.ExternalLink, undefined, 'https://example.org/help');

        await publishQuestionnaire(request, questionnaire.id);

        const { response: res } =await getNextState(request, questionnaire.id, {
            currentQuestionId: q1.id, selectedAnswerId: null, selectedAnswerIds: [answer.id]
        });
        
        const body = await res.json();

        expect(res.status()).toBe(200);
        expect(body.content).toBe('https://example.org/help');
    });
});