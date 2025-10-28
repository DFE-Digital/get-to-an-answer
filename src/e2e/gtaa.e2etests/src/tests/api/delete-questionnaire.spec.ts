import {test} from "@playwright/test";
import {createQuestionnaire, deleteQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    expectHttpStatusCode, expectQuestionnaireInitStateContent, expectQuestionnaireInitStateIO,
    expectQuestionnaireInitStateTypes,
    expectQuestionnaireSchema
} from "../../helpers/api-assertions-helper";
import {GUID_REGEX} from "../../constants/test-data-constants";

test.describe('DELETE Questionnaire', () => {
 
    test('SOFT DELETE a specific questionnaire successfully', async ({ request }) => {
        // First, create a new questionnaire to ensure there is one to delete
        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request);
        
        expectHttpStatusCode(questionnairePostResponse, 201);
        
        const {deleteQuestionnaireResponse, deleteQuestionnaireBody } = await deleteQuestionnaire(
            request,
            questionnaire.id
        );
        
        expectHttpStatusCode(deleteQuestionnaireResponse, 204);
        

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
    });
});
