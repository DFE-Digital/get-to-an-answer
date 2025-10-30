import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestion, deleteQuestion, getQuestion, listQuestions} from "../../test-data-seeder/question-data";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {QuestionType} from "../../constants/test-data-constants";

test.describe('DELETE Question API request', () => {
    test('Validate DELETE question successfully for a questionnaire', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            undefined,
            'Question to be deleted',
            QuestionType.SINGLE,
            'This question will be deleted'
        );

        const questionId = question.id;
        const createdAt = question.createdAt;
        const createdBy = question.createdBy;

        // Verify question exists before deletion
        const {questionGetResponse: beforeDelete} = await getQuestion(request, questionId);
        expect200HttpStatusCode(beforeDelete, 200);

        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            questionId
        );

        expect(deleteQuestionResponse.status()).toBeGreaterThanOrEqual(200);
        expect(deleteQuestionResponse.status()).toBeLessThan(300);
        expect([200, 204]).toContain(deleteQuestionResponse.status());

        const {questionGetResponse: afterDelete} = await getQuestion(request, questionId);
        expect(afterDelete.status()).toBe(404);

        const {questionGetBody: questionsList} = await listQuestions(request, questionnaire.id);
        if (Array.isArray(questionsList)) {
            const deletedQuestionInList = questionsList.find((q: any) => q.id === questionId);
            expect(deletedQuestionInList).toBeUndefined();
        }

        expect(createdAt).toBeDefined();
        expect(typeof createdAt).toBe('string');

        const createdAtTimestamp = new Date(createdAt).getTime();
        expect(isNaN(createdAtTimestamp)).toBe(false);
    });

    test('Validate DELETE question that is not permitted (unauthorized user)', async ({request}) => {
        const ownerToken = JwtHelper.ValidToken;
        const unauthorizedToken = JwtHelper.UnauthorizedToken;

        // Create questionnaire and question with owner token
        const {questionnaire} = await createQuestionnaire(request, ownerToken);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            ownerToken,
            'Question owned by another user',
            QuestionType.SINGLE
        );

        const {deleteQuestionResponse} = await deleteQuestion(
            request,
            question.id,
            unauthorizedToken
        );

        expect([403, 404]).toContain(deleteQuestionResponse.status());

        //no deletion should be applied
        const {questionGetResponse: verifyExists} = await getQuestion(
            request,
            question.id,
            ownerToken
        );
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE question not found (invalid question id)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const nonExistentQuestionId = '00000000-0000-0000-0000-000000000000';

        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            nonExistentQuestionId
        );

        expect([400, 404]).toContain(deleteQuestionResponse.status());
    });

    test('Validate DELETE question with invalid format question id', async ({request}) => {
        const invalidQuestionId = 'invalid-id-format-12345';

        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            invalidQuestionId
        );

        expect(deleteQuestionResponse.status()).toBe(400);

        // And error message to display
        if (deleteQuestionBody) {
            expect(deleteQuestionBody).toBeDefined();
            const parsedError = JSON.parse(deleteQuestionBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }
    });

    test('Validate DELETE question with invalid JWT bearer token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            undefined,
            'Question for invalid token test'
        );

        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            question.id,
            JwtHelper.InvalidToken
        );

        expect(deleteQuestionResponse.status()).toBe(401);

        // And error message to display
        if (deleteQuestionBody) {
            expect(deleteQuestionBody).toBeDefined();
            const parsedError = JSON.parse(deleteQuestionBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }

        // And no deletion occurs
        const {questionGetResponse: verifyExists} = await getQuestion(request, question.id);
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE question with expired JWT bearer token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            undefined,
            'Question for expired token test'
        );
        
        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            question.id,
            JwtHelper.ExpiredToken
        );
        
        expect(deleteQuestionResponse.status()).toBe(401);

        // And error message to display
        if (deleteQuestionBody) {
            expect(deleteQuestionBody).toBeDefined();
            const parsedError = JSON.parse(deleteQuestionBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }

        // And no deletion occurs
        const {questionGetResponse: verifyExists} = await getQuestion(request, question.id);
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE already deleted question (idempotency)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            undefined,
            'Question to delete twice',
            QuestionType.SINGLE
        );

        // Delete the question first time
        const {deleteQuestionResponse: firstDelete} = await deleteQuestion(
            request,
            question.id
        );

        // First deletion should succeed
        expect(firstDelete.status( ) === 204).toBe(true);

        // Send a DELETE request for a question which is already soft-deleted
        const {deleteQuestionResponse: secondDelete} = await deleteQuestion(
            request,
            question.id
        );

        expect(secondDelete.status( ) === 404).toBe(true);
        
        const {questionGetResponse: verifyDeleted} = await getQuestion(request, question.id);
        expect(verifyDeleted.status()).toBe(404);
    });
});