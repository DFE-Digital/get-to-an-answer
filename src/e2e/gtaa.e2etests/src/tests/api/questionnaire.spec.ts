import {test, expect} from '@playwright/test';
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({page, request}) => {
        
        const questionnairePostResponse = await createQuestionnaire(request);
        
    });
});