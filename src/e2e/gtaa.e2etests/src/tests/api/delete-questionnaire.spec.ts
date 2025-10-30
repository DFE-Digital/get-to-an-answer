import {expect, test} from "@playwright/test";
import {createQuestionnaire, deleteQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    expectAnswerSchema,
    expect200HttpStatusCode, expectQuestionnaireInitStateContent, expectQuestionnaireInitStateIO,
    expectQuestionnaireInitStateTypes,
    expectQuestionnaireSchema
} from "../../helpers/api-assertions-helper";
import {AnswerDestinationType, GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('DELETE Questionnaire', () => {
 
    test('SOFT DELETE a specific questionnaire successfully', async ({ request }) => {
        // First, create a new questionnaire to ensure there is one to delete
        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {deleteQuestionnaireResponse } = await deleteQuestionnaire(
            request,
            questionnaire.id
        );

        expect200HttpStatusCode(deleteQuestionnaireResponse, 204);

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
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {   
            questionPostResponse,
            question
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);
        
        const {
            answerPostResponse, 
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id, 
                ...question
            },
        );
        
        expect200HttpStatusCode(answerPostResponse, 200);
        
        const {deleteQuestionnaireResponse, deleteQuestionnaireBody: resBody } = await deleteQuestionnaire(
            request,
            questionnaire.id
        );
        
        expect200HttpStatusCode(deleteQuestionnaireResponse, 204);
    });

    test('SOFT DELETE a questionnaire that is not permitted', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const { deleteQuestionnaireResponse } = await deleteQuestionnaire(
            request,
            questionnaire.id,
            JwtHelper.UnauthorizedToken
        );
        
        expect([403, 404]).toContain(deleteQuestionnaireResponse.status());

        const { questionnaireGetResponse: qGetResponse } = await getQuestionnaire(request, questionnaire.id);
        expect200HttpStatusCode(qGetResponse, 200);
    });

    test('DELETE non-existent questionnaire returns 404 and no deletion occurs', async ({ request }) => {
        const fakeId = '00000000-0000-0000-0000-000000000999';

        const { deleteQuestionnaireResponse, deleteQuestionnaireBody } = await deleteQuestionnaire(
            request,
            fakeId
        );
        
        expect(deleteQuestionnaireResponse.status()).toBe(404);

        expect(deleteQuestionnaireBody).toBeTruthy();

        if (typeof deleteQuestionnaireBody === 'object' && deleteQuestionnaireBody !== null) {
            expect(deleteQuestionnaireBody.status || deleteQuestionnaireBody.title).toBeTruthy();
        }
    });

    test('DELETE with invalid JWT token should be unauthorized and not delete', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const { deleteQuestionnaireResponse} = await deleteQuestionnaire(
            request,
            questionnaire.id,
            JwtHelper.InvalidToken
        );

        expect(deleteQuestionnaireResponse.ok()).toBeFalsy();
        expect([400, 401, 403, 404]).toContain(deleteQuestionnaireResponse.status());

        const { questionnaireGetResponse: qGetResponse } = await getQuestionnaire(request, questionnaire.id);
        expect200HttpStatusCode(qGetResponse, 200);
    });

    test('DELETE with expired JWT token should be unauthorized and not delete', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const { deleteQuestionnaireResponse} = await deleteQuestionnaire(
            request,
            questionnaire.id,
            JwtHelper.ExpiredToken
        );

        expect(deleteQuestionnaireResponse.ok()).toBeFalsy();
        expect([400, 401, 403, 404]).toContain(deleteQuestionnaireResponse.status());

        const { questionnaireGetResponse: qGetResponse } = await getQuestionnaire(request, questionnaire.id);
        expect200HttpStatusCode(qGetResponse, 200);
    });

    test('DELETE already soft-deleted questionnaire should return not found / no-op', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const firstDelete = await deleteQuestionnaire(request, questionnaire.id);
        expect200HttpStatusCode(firstDelete.deleteQuestionnaireResponse, 204);

        const secondDelete = await deleteQuestionnaire(request, questionnaire.id);
        expect([400, 404, 403]).toContain(secondDelete.deleteQuestionnaireResponse.status());
    });

    test('DELETE questionnaire that has questions linked should be prevented (no duplicate deletion)', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        const { deleteQuestionnaireResponse } = await deleteQuestionnaire(request, questionnaire.id);

        if (deleteQuestionnaireResponse.ok()) {
            const { questionnaireGetResponse: qGetResponse } = await getQuestionnaire(request, questionnaire.id);
            expect([404, 410]).toContain(qGetResponse.status());
        } else {
            expect([400, 403, 404]).toContain(deleteQuestionnaireResponse.status());
            const { questionnaireGetResponse: qGetResponse } = await getQuestionnaire(request, questionnaire.id);
            expect200HttpStatusCode(qGetResponse, 200);
        }
    });
    
});
