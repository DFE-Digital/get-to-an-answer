import {test, expect} from '@playwright/test';
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({request}) => {

        const {questionnairePostResponse, payload} = await createQuestionnaire(request);

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeTruthy();
        expect(questionnairePostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionnairePostResponse).toHaveProperty('id');
        expect(questionnairePostResponse).toHaveProperty(payload.title);
        expect(questionnairePostResponse).toHaveProperty(payload.description);
        expect(questionnairePostResponse).toHaveProperty(payload.slug);

        // --- Type sanity checks ---
        expect(typeof questionnairePostResponse.id).toBe('string');
        expect(typeof questionnairePostResponse.title).toBe('string');
        expect(typeof questionnairePostResponse.description).toBe('string');
        expect(typeof questionnairePostResponse.slug).toBe('string');

        // --- Basic content sanity ---
        expect(questionnairePostResponse.title.trim().length).toBeGreaterThan(0);
        expect(questionnairePostResponse.description.trim().length).toBeGreaterThan(0);
    });
});

test.describe('GET questionnaire api tests', () => {
    test('Validate GET questionnaire', async ({request}) => {
        const {questionnairePostResponse, payload} = await createQuestionnaire(request);
        const questionnaireId = await questionnairePostResponse.id;
        
        const questionnaireGetResponse = await getQuestionnaire(request, questionnaireId);

        //Schema checks
        expect(questionnaireGetResponse).toHaveProperty('id');
        expect(questionnaireGetResponse).toHaveProperty(payload.title);
        expect(questionnaireGetResponse).toHaveProperty(payload.description);
        expect(questionnaireGetResponse).toHaveProperty(payload.slug);
        
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