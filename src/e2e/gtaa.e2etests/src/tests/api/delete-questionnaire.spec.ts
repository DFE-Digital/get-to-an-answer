import {expect, test} from "@playwright/test";
import {createQuestionnaire, deleteQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    expectHttpStatusCode, expectQuestionnaireInitStateContent, expectQuestionnaireInitStateIO,
    expectQuestionnaireInitStateTypes,
    expectQuestionnaireSchema
} from "../../helpers/api-assertions-helper";
import {AnswerDestinationType, GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

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
    
    test('SOFT DELETE a specific questionnaire with questions & answers', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
            payload: questionnairePayload
        } = await createQuestionnaire(request);

        expectHttpStatusCode(questionnairePostResponse, 201);

        const {   
            questionPostResponse,
            question,
            payload: questionPayload
        } = await createQuestion(request, questionnaire.id);

        expectHttpStatusCode(questionPostResponse, 201);
        
        const {
            res: answerPostResponse, 
            responseBody,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id, 
                ...question
            },
        );
        
        expectHttpStatusCode(answerPostResponse, 200);
        
        const {deleteQuestionnaireResponse, deleteQuestionnaireBody} = await deleteQuestionnaire(
            request,
            questionnaire.id
        );

        expectHttpStatusCode(deleteQuestionnaireResponse, 204);
    });
});
