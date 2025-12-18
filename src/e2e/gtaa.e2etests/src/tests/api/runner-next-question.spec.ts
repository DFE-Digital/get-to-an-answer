import {expect, test} from '@playwright/test';
import {AnswerDestinationType, EntityStatus, QuestionType} from '../../constants/test-data-constants';
import {
    addContributor,
    createQuestionnaire, getNextState,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire
} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {createAnswer, createSingleAnswer} from '../../test-data-seeder/answer-data';
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('POST /api/questionnaires/{questionnaireId}/next-state', () => {
    test('returns next destination for a valid published questionnaire path (200)', async ({ request }) => {
        const token = JwtHelper.NoRecordsToken();
        
        // Create questionnaire -> 2 questions -> publish -> call next-state
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { 
            slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)
        
        const { question: q1 } = await createQuestion(request, questionnaire.id, token, 'Q1', QuestionType.MultiSelect);
        const { question: q2 } = await createQuestion(request, questionnaire.id, token, 'Q2', QuestionType.SingleSelect);

        // Link q1 -> q2 via answer destination
        const {answer} = await createSingleAnswer(request, {
            questionnaireId: questionnaire.id,
            questionId: q1.id, 
            content: 'GoNext',
            priority: 1, 
            destinationType: AnswerDestinationType.Question, 
            destinationQuestionId: q2.id
        }, token);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id,
            questionId: q1.id,
            content: 'GoNext',
            priority: 1,
            destinationType: AnswerDestinationType.ExternalLink, 
            destinationUrl: 'https://example.org/help'
        }, token);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id,
            questionId: q2.id,
            content: 'GoNext',
            priority: 1,
            destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl: 'https://example.org/help'
        }, token);

        // Must be published to use runner endpoints (non-preview)
        await publishQuestionnaire(request, questionnaire.id, token);

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
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.SingleSelect);
        const { question: q2 } = await createQuestion(request, questionnaire.id, undefined, 'Q2', QuestionType.SingleSelect);

        const { answer } = await createAnswer(request, questionnaire.id, q1.id, 'Next', undefined, 1, AnswerDestinationType.Question, q2.id);

        // Ensure draft
        await updateQuestionnaire(request, questionnaire.id, { title: 'Keep Draft' });
        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetBody.status).toBe(EntityStatus.Draft);
        
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

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')
        
        const { question: q1 } = await createQuestion(request, questionnaire.id, undefined, 'Q1', QuestionType.SingleSelect);

        const {answer} = await createAnswer(request, questionnaire.id, q1.id, 'Go External', undefined, 1,
            AnswerDestinationType.ExternalLink, undefined, undefined, 'https://example.org/help');

        await publishQuestionnaire(request, questionnaire.id);

        const { response: res } =await getNextState(request, questionnaire.id, {
            currentQuestionId: q1.id, selectedAnswerId: null, selectedAnswerIds: [answer.id]
        });
        
        const body = await res.json();

        expect(res.status()).toBe(200);
        expect(body.content).toBe('https://example.org/help');
    });
});