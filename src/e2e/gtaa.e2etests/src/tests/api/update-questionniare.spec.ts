import {expect, test} from "@playwright/test";
import {
    createQuestionnaire, deleteQuestionnaire,
    getQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {
  expectHttpStatusCode,
  expectQuestionnaireSchema,
  expectQuestionnaireTypes,
  expectQuestionnaireContent
} from "../../helpers/api-assertions-helper";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('UPDATE Questionnaire API tests', () => {
    test('Validate UPDATE questionnaire successfully', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update
        
        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';
        
        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expectHttpStatusCode(questionnairePostResponse, 201);
        
        // Get existing questionnaire
        const {
            questionnaireGetResponse, 
            questionnaireGetBody : questionnaireBody
        } = await getQuestionnaire(
            request, 
            questionnaire.id);
        
        console.log("Questionnaire body: " + questionnaireBody);
        
        // Change the questionnaire title
        const {updatedQuestionnairePostResponse } = await updateQuestionnaire(
            request,
            questionnaire.id,
            { title: 'Updated title' }
        );
        
        expectHttpStatusCode(updatedQuestionnairePostResponse, 204);
        
        // Check description has not changed but title has changed
        const {
            questionnaireGetResponse: updatedQuestionnaireGetResponse,
            questionnaireGetBody : updatedQuestionnaireBody
        } = await getQuestionnaire(
            request,
            questionnaire.id);

        
        expect(updatedQuestionnaireBody.title).toEqual('Updated title');
        
        expect(questionnaireBody.description).toEqual(updatedQuestionnaireBody.description);
    });

});