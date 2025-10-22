import {test, expect} from '@playwright/test';
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import { GUID_REGEX } from "../../constants/test-data-constants";
import { JwtHelper } from '../../helpers/JwtHelper';

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({request}) => {

        const bearerToken = JwtHelper.ValidToken; // Use default valid token
        
        const {
            questionnairePostResponse, 
            questionnaire, 
            payload
        } = await createQuestionnaire(request, bearerToken);

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeTruthy();
        expect(questionnairePostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionnaire).toHaveProperty('id');
        expect(questionnaire).toHaveProperty('title');
        expect(questionnaire).toHaveProperty('description');
        expect(questionnaire).toHaveProperty('slug');

        // --- Type sanity checks ---
        expect(typeof questionnaire.id).toBe('string');
        expect(typeof questionnaire.title).toBe('string');
        expect(typeof questionnaire.description).toBe('string');
        expect(typeof questionnaire.slug).toBe('string');

        // --- Basic content sanity ---
        expect(questionnaire.title.trim().length).toBeGreaterThan(0);
        expect(questionnaire.description.trim().length).toBeGreaterThan(0);

        // --- I/O checks ---
        expect(questionnaire.id).toMatch(GUID_REGEX);
        expect(questionnaire.title).toBe(payload.title);
        expect(questionnaire.description).toBe(payload.description);
        expect(questionnaire.slug).toBe(payload.slug);
    });
});

test.describe('GET questionnaire api tests', () => {
    test('Validate GET questionnaire', async ({request}) => {
        const bearerToken = JwtHelper.ValidToken;
        const {questionnaire, payload} = await createQuestionnaire(request, bearerToken);
        const questionnaireId = await questionnaire.id;
        
        const questionnaireGetResponse = await getQuestionnaire(request, questionnaireId, bearerToken);

        //Schema checks
        expect(questionnaireGetResponse.id).toMatch(GUID_REGEX);
        expect(questionnaireGetResponse).toHaveProperty('title');
        expect(questionnaireGetResponse).toHaveProperty('description');
        expect(questionnaireGetResponse).toHaveProperty('slug');
        
        //Type sanity checks
        expect(typeof questionnaireGetResponse.title).toBe('string');
        expect(typeof questionnaireGetResponse.description).toBe('string');
        expect(typeof questionnaireGetResponse.slug).toBe('string');

        //Content sanity
        expect(questionnaireGetResponse.title.trim().length).toBeGreaterThan(0);
        expect(questionnaireGetResponse.description.trim().length).toBeGreaterThan(0);
        expect(questionnaireGetResponse.slug.trim().length).toBeGreaterThan(0);
    });
});