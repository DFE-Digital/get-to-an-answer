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
    
    test('Validate UPDATE questionnaire with valid payload & correct response returned', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expectHttpStatusCode(questionnairePostResponse, 201);
        
        
    })
    
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
        
        const newTitle = 'Updated Test Questionnaire Title';
        const newDescription = 'Updated Test Questionnaire Description';
        //random slug each time 
        const newSlug = `updated-test-questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;
        
        // Change the questionnaire title
        const {updatedQuestionnairePostResponse, updatedQuestionnaire: updateQuestionnaireResBody } = await updateQuestionnaire(
            request,
            questionnaire.id,
            { 
                title: newTitle, 
                description: newDescription,
                slug: newSlug
            }
        );
        
        expectHttpStatusCode(updatedQuestionnairePostResponse, 204);
        
        // Check for no content in body - validate structure
        expect(updateQuestionnaireResBody).toBeFalsy();
        
        // Check new entries have been updated in questionnaire
        const {
            questionnaireGetResponse: getUpdatedQuestionnaireGetResponse,
            questionnaireGetBody : getUpdatedQuestionnaireBody
        } = await getQuestionnaire(
            request,
            questionnaire.id);
        
        expect(getUpdatedQuestionnaireBody.title).toEqual(newTitle);
        expect(getUpdatedQuestionnaireBody.description).toEqual(newDescription);
        expect(getUpdatedQuestionnaireBody.slug).toEqual(newSlug);
    });

});