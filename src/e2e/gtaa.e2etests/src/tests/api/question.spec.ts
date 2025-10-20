import {test, expect} from '@playwright/test';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {QuestionType} from '../../constants/test-data-constants';

test.describe('POST Create question api request', () => {
    test('Validate POST create new question', async ({page, request}) => {

        const {questionnairePostResponse} = await createQuestionnaire(request);
        const questionnaireId = await questionnairePostResponse.id;

        const {questionPostResponse, payload} = await createQuestion(request, questionnaireId)

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeTruthy();
        expect(questionPostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionPostResponse).toHaveProperty(payload.questionnaireId);
        expect(questionPostResponse).toHaveProperty(payload.content);
        expect(questionPostResponse).toHaveProperty(payload.description);
        expect(questionPostResponse).toHaveProperty(payload.type);

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