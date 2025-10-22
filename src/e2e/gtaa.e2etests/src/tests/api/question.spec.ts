import {test, expect} from '@playwright/test';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {GUID_REGEX, QuestionType} from '../../constants/test-data-constants';
import { JwtHelper } from '../../helpers/JwtHelper';

test.describe('POST Create question api request', () => {
    test('Validate POST create new question', async ({page, request}) => {
        const bearerToken = JwtHelper.ValidToken;
        const {questionnaire} = await createQuestionnaire(request, bearerToken);
        const questionnaireId = await questionnaire.id;

        const {questionPostResponse, payload} = await createQuestion(request, questionnaireId, bearerToken)

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeTruthy();
        expect(questionPostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionPostResponse).toHaveProperty('questionnaireId');
        expect(questionPostResponse).toHaveProperty('content');
        expect(questionPostResponse).toHaveProperty('description');
        expect(questionPostResponse).toHaveProperty('type');

        // --- Type sanity checks ---
        expect(typeof questionPostResponse.questionnaireId).toMatch(GUID_REGEX);
        expect(typeof questionPostResponse.content).toBe('string');
        expect(typeof questionPostResponse.description).toBe('string');
        expect(typeof questionPostResponse.type).toBe(QuestionType);

        // --- Basic content sanity ---
        expect(questionPostResponse.content.trim().length).toBeGreaterThan(0);
        expect(questionPostResponse.description.trim().length).toBeGreaterThan(0);
        expect(questionPostResponse.type.trim().length).toBeGreaterThan(0);
    });
});