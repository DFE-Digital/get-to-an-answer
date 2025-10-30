import {expect, test} from "@playwright/test";
import {
    createQuestionnaire, deleteQuestionnaire,
    getQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";

import {
    expect200HttpStatusCode,
    expectQuestionnaireSchema,
    expectQuestionnaireTypes,
    expectQuestionnaireContent,
    expectQuestionnaireInitStateTypes,
    expectQuestionnaireInitStateContent,
    expectQuestionnaireInitStateIO
} from "../../helpers/api-assertions-helper";
import {JwtHelper} from "../../helpers/JwtHelper";
import {AnswerDestinationType, GUID_REGEX} from "../../constants/test-data-constants";

test.describe('UPDATE Questionnaire API tests', () => {
    test('Validate UPDATE questionnaire successfully with updated & valid response', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const newTitle = 'Updated Test Questionnaire Title';
        const newDescription = 'Updated Test Questionnaire Description';
        //random slug each time 
        const newSlug = `updated-test-questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;

        // Change the questionnaire title
        const {
            updatedQuestionnairePostResponse,
            updatedQuestionnaire: updateQuestionnaireResBody
        } = await updateQuestionnaire(
            request,
            questionnaire.id,
            {
                title: newTitle,
                description: newDescription,
                slug: newSlug
            }
        );

        expect200HttpStatusCode(updatedQuestionnairePostResponse, 204);

        // Check for no content in body - validate structure
        expect(updateQuestionnaireResBody).toBeFalsy();

        // Check new entries have been updated in questionnaire
        const {
            questionnaireGetResponse: getUpdatedQuestionnaireGetResponse,
            questionnaireGetBody: getUpdatedQuestionnaireBody
        } = await getQuestionnaire(
            request,
            questionnaire.id);

        // // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // // --- Type sanity checks ---
        expectQuestionnaireInitStateTypes(questionnaire);
        //
        // // --- Basic content sanity ---
        expectQuestionnaireInitStateContent(questionnaire);
        //
        // // --- I/O checks ---
        expectQuestionnaireInitStateIO(questionnaire, payload, GUID_REGEX);

        expect(getUpdatedQuestionnaireBody.title).toEqual(newTitle);
        expect(getUpdatedQuestionnaireBody.description).toEqual(newDescription);
        expect(getUpdatedQuestionnaireBody.slug).toEqual(newSlug);

    });

    test('UPDATE questionnaire with invalid auth token', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const newTitle = 'Updated Test Questionnaire Title';

        // Change the questionnaire title
        const {
            updatedQuestionnairePostResponse,
        } = await updateQuestionnaire(
            request,
            questionnaire.id,
            {
                title: newTitle,
            },
            JwtHelper.InvalidToken
        );
        
        expect(updatedQuestionnairePostResponse.status()).toBe(401);
    });

    test('UPDATE questionnaire with expired auth token', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const newTitle = 'Updated Test Questionnaire Title';

        // Change the questionnaire title
        const {
            updatedQuestionnairePostResponse,
        } = await updateQuestionnaire(
            request,
            questionnaire.id,
            {
                title: newTitle,
            },
            JwtHelper.ExpiredToken
        );

        expect(updatedQuestionnairePostResponse.status()).toBe(401);
    });
    
    test('UPDATE questionnaire with invalid id', async ({request}) => {

        const newTitle = 'Updated Test Questionnaire Title';
        const invalidId = '00000000-0000-0000-0000-000000000999';
        
        const {
            updatedQuestionnairePostResponse, updatedQuestionnaire: updateQuestionnaireResBody
        } = await updateQuestionnaire(
            request,
            invalidId,
            {
                title: newTitle,
            }
        );
        
        expect(updatedQuestionnairePostResponse.status()).toBe(404);
        
    });
    
    test('UPDATE questionnaire with unauthorized access to questionnaire', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const newTitle = 'Updated Test Questionnaire Title';

        const {
            updatedQuestionnairePostResponse, updatedQuestionnaire: updateQuestionnaireResBody
        } = await updateQuestionnaire(
            request,
            questionnaire.id,
            {
                title: newTitle,
            }, 
            JwtHelper.UnauthorizedToken
        );
        
        expect(updatedQuestionnairePostResponse.status()).toBe(403);
    });
    
    test('UPDATE questionnaire with invalid questionnaire data', async ({request}) => {

        // First, create a new questionnaire to ensure there is one to update

        const initialTitle = 'Initial Test Questionnaire Title';
        const initialDescription = 'Initial Test Questionnaire Description';

        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request, undefined, initialTitle, initialDescription);

        expect200HttpStatusCode(questionnairePostResponse, 201);
        
        // Testing with description being boolean, not string

        const newTitle = 'Updated Test Questionnaire Title';
        const newDescription = true;

        // Change the questionnaire title
        const {
            updatedQuestionnairePostResponse,
            updatedQuestionnaire: updateQuestionnaireResBody
        } = await updateQuestionnaire(
            request,
            questionnaire.id,
            {
                title: newTitle,
                description: newDescription
            }
        );
        
        console.log("Body: " + updateQuestionnaireResBody);
        
        const errorMessage = 'One or more validation errors occurred';
        
        expect(updatedQuestionnairePostResponse.status()).toBe(400);
        expect(updateQuestionnaireResBody).toContain(errorMessage);
        
        // Get questionnaire to check questionnaire has not been updated
        const {
            questionnaireGetResponse: getUpdatedQuestionnaireGetResponse,
            questionnaireGetBody: getUpdatedQuestionnaireBody
        } = await getQuestionnaire(
            request,
            questionnaire.id);
        
        expect(getUpdatedQuestionnaireBody.title).toEqual(initialTitle);
        
    });
})