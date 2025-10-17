import {test, expect} from '@playwright/test';
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";

test.describe('POST Create question api request', () => {
    test('Validate POST create new question', async ({page, request}) => {

        const questionnairePostResponse = await createQuestionnaire(request);
        const questionnaireId = await questionnairePostResponse.id;
        
        const questionPostResponse = await createQuestion(request, questionnaireId)
        
    });
});