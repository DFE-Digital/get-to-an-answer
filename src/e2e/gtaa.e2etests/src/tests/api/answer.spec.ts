import {test, expect} from '@playwright/test';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {createSingleAnswer} from '../../test-data-seeder/answer-data';
import {QuestionType, QuestionPrefix, AnswerDestinationType} from '../../constants/test-data-constants';

test.describe('POST answers api tests', () => {
    test('POST an answer for a single question', async ({request}) => {

        const {questionnairePostResponse} = await createQuestionnaire(request);
        const questionnaireId = await questionnairePostResponse.id;

        const {questionPostResponse} = await createQuestion(request, questionnaireId)
        const qId = await questionPostResponse.id;

        const {answerPostResponse, payload} = await createSingleAnswer(
            request,
            qId,
            undefined,
            undefined,
            'Option 1',
            0.0,
            AnswerDestinationType.PAGE,
            '/page-destination-url'
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeTruthy();
        expect(answerPostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(answerPostResponse).toHaveProperty(payload.questionId);
        expect(answerPostResponse).toHaveProperty(payload.content);
        expect(answerPostResponse).toHaveProperty(payload.description);
        expect(answerPostResponse).toHaveProperty(payload.destination);
        expect(answerPostResponse).toHaveProperty(payload.destinationType);
        expect(answerPostResponse).toHaveProperty((payload.weight).toString());

        // --- Type sanity checks ---
        expect(typeof questionPostResponse.questionnaireId).toBe(Number);
        expect(typeof questionPostResponse.content).toBe('string');
        expect(typeof questionPostResponse.description).toBe('string');
        expect(typeof questionPostResponse.type).toBe(QuestionType);

        // --- Basic content sanity ---
        expect(questionPostResponse.content.trim().length).toBeGreaterThan(0);
        expect(questionPostResponse.description.trim().length).toBeGreaterThan(0);
        expect(questionPostResponse.type.trim().length).toBeGreaterThan(0);
    });
});