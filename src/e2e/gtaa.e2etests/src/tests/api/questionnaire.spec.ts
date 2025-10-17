import {test, expect} from '@playwright/test';
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({page, request}) => {
        
        const questionnairePostResponse = await createQuestionnaire(request);

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeTruthy();
        expect(questionnairePostResponse.status()).toBe(201);

        // --- Schema-level checks ---
        expect(questionnairePostResponse).toHaveProperty('id');
        expect(questionnairePostResponse).toHaveProperty('Title');
        expect(questionnairePostResponse).toHaveProperty('Description');
        expect(questionnairePostResponse).toHaveProperty('Slug');

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