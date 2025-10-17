import {test, expect} from '@playwright/test';
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({page, request}) => {

        const questionnairePostResponse = await createQuestionnaire(request);

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeTruthy();
        expect(questionnairePostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionnairePostResponse).toHaveProperty('id');
        expect(questionnairePostResponse).toHaveProperty('title');
        expect(questionnairePostResponse).toHaveProperty('description');
        expect(questionnairePostResponse).toHaveProperty('slug');

        // --- Type sanity checks ---
        expect(typeof questionnairePostResponse.id).toBe('string');
        expect(typeof questionnairePostResponse.title).toBe('string');
        expect(typeof questionnairePostResponse.description).toBe('string');
        expect(typeof questionnairePostResponse.slug).toBe('string');

        // --- Basic content sanity ---
        expect(questionnairePostResponse.title.trim().length).toBeGreaterThan(0);
        expect(questionnairePostResponse.description.trim().length).toBeGreaterThan(0);

        //delete test data logic
    });
});

test.describe('GET questionnaire api tests', () => {
    test('Validate GET questionnaire', async ({request}) => {
        const questionnairePostResponse = await createQuestionnaire(request);
        const questionnaireId = await questionnairePostResponse.id;
        
        const questionnaireGetResponse = await getQuestionnaire(request, questionnaireId);

        //Schema checks
        expect(questionnaireGetResponse).toHaveProperty('id');
        expect(questionnaireGetResponse).toHaveProperty('title');
        expect(questionnaireGetResponse).toHaveProperty('description');
        
        //Type sanity checks
        expect(typeof questionnaireGetResponse.title).toBe('string');
        expect(typeof questionnaireGetResponse.description).toBe('string');

        //Content sanity
        expect(questionnaireGetResponse.title.trim().length).toBeGreaterThan(0);

        //delete test data logic
    });
});