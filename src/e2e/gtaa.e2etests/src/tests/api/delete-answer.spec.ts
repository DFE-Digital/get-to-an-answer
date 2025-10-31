import {expect, test} from "@playwright/test";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, deleteQuestion, getQuestion, listQuestions} from "../../test-data-seeder/question-data";
import {QuestionType} from "../../constants/test-data-constants";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {createSingleAnswer, deleteAnswer, getAnswer, listAnswers} from "../../test-data-seeder/answer-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('DELETE Answer API request', () => {
    test('Validate DELETE answer successfully for a question', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(
            request,
            questionnaire.id
        );
        
        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            },
        );

        const answerId = answer.id;
        const createdAt = answer.createdAt;
        const createdBy = answer.createdBy;


        // Verify question exists before deletion
        const {answerGetResponse: beforeDelete} = await getAnswer(request, answerId);
        expect200HttpStatusCode(beforeDelete, 200);

        const {deleteAnswerResponse, deleteAnswerBody} = await deleteAnswer(
            request,
            answerId
        );

        expect(deleteAnswerResponse.status()).toBeGreaterThanOrEqual(200);
        expect(deleteAnswerResponse.status()).toBeLessThan(300);
        expect([200, 204]).toContain(deleteAnswerResponse.status());

        const {answerGetResponse: afterDelete} = await getAnswer(request, answerId);
        expect(afterDelete.status()).toBe(404);

        const {answers: answersList} = await listAnswers(request, questionnaire.id);
        if (Array.isArray(answersList)) {
            const deletedQuestionInList = answersList.find((a: any) => a.id === answerId);
            expect(deletedQuestionInList).toBeUndefined();
        }

        expect(createdAt).toBeDefined();
        expect(typeof createdAt).toBe('string');

        const createdAtTimestamp = new Date(createdAt).getTime();
        expect(isNaN(createdAtTimestamp)).toBe(false);
    });

    test('Validate DELETE answer that is not permitted (unauthorized user)', async ({request}) => {
        const ownerToken = JwtHelper.ValidToken;
        const unauthorizedToken = JwtHelper.UnauthorizedToken;

        // Create a questionnaire and question with an owner token
        const {questionnaire} = await createQuestionnaire(request, ownerToken);
        const {question} = await createQuestion(
            request,
            questionnaire.id,
            ownerToken,
            'Question owned by another user',
            QuestionType.SINGLE
        );

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            },ownerToken
        );
        
        const {deleteAnswerResponse} = await deleteAnswer(
            request,
            answer.id,
            unauthorizedToken
        );

        expect([403, 404]).toContain(deleteAnswerResponse.status());

        //no deletion should be applied
        const {answerGetResponse: verifyExists} = await getAnswer(
            request,
            answer.id,
            ownerToken
        );
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE answer not found (invalid question id)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const nonExistentAnswerId = '00000000-0000-0000-0000-000000000000';

        const {deleteAnswerResponse, deleteAnswerBody} = await deleteAnswer(
            request,
            nonExistentAnswerId
        );

        expect([400, 404]).toContain(deleteAnswerResponse.status());
    });

    test('Validate DELETE answer with invalid format question id', async ({request}) => {
        const invalidAnswerId = 'invalid-id-format-12345';

        const {deleteAnswerResponse, deleteAnswerBody} = await deleteAnswer(
            request,
            invalidAnswerId
        );

        expect(deleteAnswerResponse.status()).toBe(400);

        // And error message to display
        if (deleteAnswerBody) {
            expect(deleteAnswerBody).toBeDefined();
            const parsedError = JSON.parse(deleteAnswerBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }
    });

    test('Validate DELETE answer with invalid JWT bearer token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const {
            answer
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            }
        );
        
        const {deleteAnswerResponse, deleteAnswerBody} = await deleteAnswer(
            request,
            answer.id,
            JwtHelper.InvalidToken
        );

        expect(deleteAnswerResponse.status()).toBe(401);

        // And error message to display
        if (deleteAnswerBody) {
            expect(deleteAnswerBody).toBeDefined();
            const parsedError = JSON.parse(deleteAnswerBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }

        // And no deletion occurs
        const {questionGetResponse: verifyExists} = await getQuestion(request, question.id);
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE question with expired JWT bearer token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const {
            answer
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            }
        );
        
        const {deleteAnswerResponse, deleteAnswerBody} = await deleteAnswer(
            request,
            answer.id,
            JwtHelper.ExpiredToken
        );

        expect(deleteAnswerResponse.status()).toBe(401);

        // And error message to display
        if (deleteAnswerBody) {
            expect(deleteAnswerBody).toBeDefined();
            const parsedError = JSON.parse(deleteAnswerBody)
            expect(parsedError.errors.id[0]).toBeDefined();
        }

        // And no deletion occurs
        const {answerGetResponse: verifyExists} = await getAnswer(request, answer.id);
        expect200HttpStatusCode(verifyExists, 200);
    });

    test('Validate DELETE already deleted answer (idempotency)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const {
            answer
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
            }
        );

        // Delete the answer first time
        const {deleteAnswerResponse: firstDelete} = await deleteAnswer(
            request,
            answer.id
        );

        // First deletion should succeed
        expect(firstDelete.status( ) === 204).toBe(true);

        // Send a DELETE request for an answer which is already soft-deleted
        const {deleteAnswerResponse: secondDelete} = await deleteAnswer(
            request,
            answer.id
        );

        expect(secondDelete.status( ) === 404).toBe(true);

        const {answerGetResponse: verifyDeleted} = await getAnswer(request, answer.id);
        expect(verifyDeleted.status()).toBe(404);
    });
});