import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {
    createQuestionnaire,
    deleteQuestionnaire,
    getQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, updateQuestion, getQuestion} from "../../test-data-seeder/question-data";
import {
    expectHttpStatusCode, expectQuestionContent, expectQuestionIO, expectQuestionSchema, expectQuestionTypes
} from "../../helpers/api-assertions-helper";
import {GUID_REGEX, QuestionType} from "../../constants/test-data-constants";

test.describe('PUT Update question api request', () => {
    test('Validate PUT update question successfully', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: 'Updated question content',
            description: 'Updated question description'
        };

        const {updatedQuestionPostResponse, UpdatedQuestion} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expectHttpStatusCode(updatedQuestionPostResponse, 204);

        // Verify update by fetching the question
        const {questionGetBody} = await getQuestion(request, question.id);

        // --- Schema-level checks ---
        expectQuestionSchema(questionGetBody);

        // --- Type sanity checks ---
        expectQuestionTypes(questionGetBody);

        // --- Basic content sanity ---
        expectQuestionContent(questionGetBody);

        // --- I/O checks ---
        expectQuestionIO(questionGetBody, updatePayload, GUID_REGEX);

        // Verify updated fields
        expect(questionGetBody.content).toBe('Updated question content');
        expect(questionGetBody.description).toBe('Updated question description');
    });

    test('Validate PUT update question type', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id, undefined, 'Original content', QuestionType.SINGLE);

        const updatePayload = {
            ...question,
            type: QuestionType.MULTIPLE
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expectHttpStatusCode(updatedQuestionPostResponse, 204);

        const {questionGetBody} = await getQuestion(request, question.id);
        expect(questionGetBody.type).toBe(QuestionType.MULTIPLE);
    });

    test('Validate access to another question not permitted', async ({request}) => {
        const q1Token = JwtHelper.ValidToken;
        const q2Token = JwtHelper.UnauthorizedToken;

        const {questionnaire: q1} = await createQuestionnaire(request, q1Token);
        const {questionnaire: q2} = await createQuestionnaire(request, q2Token);

        const {question: question1} = await createQuestion(request, q1.id, q1Token);
        const {question: question2} = await createQuestion(request, q2.id, q2Token);

        const updateQuestionPayload = {
            ...question2,
            content: 'Updated content'
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question2.id,
            updateQuestionPayload,
            q1Token
        );

        // should be forbidden
        expect(updatedQuestionPostResponse.status()).toBe(403);
    });

    test('Validate PUT update question with invalid token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: 'Updated content'
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload,
            JwtHelper.InvalidToken
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(401);
    });

    test('Validate PUT update question with expired token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: 'Updated content'
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload,
            JwtHelper.ExpiredToken
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(401);
    });

    test('Validate PUT update question with invalid question id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);

        const updatePayload = {
            questionnaireId: questionnaire.id,
            content: 'Updated content',
            description: 'Updated description',
            type: QuestionType.SINGLE
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            'invalid-id-12345',
            updatePayload
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(400);
    });

    test('Validate PUT update question with missing content', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: null as any
        };

        const {updatedQuestionPostResponse, UpdatedQuestion} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(400);

        // And error message to display
        if (UpdatedQuestion) {
            expect(UpdatedQuestion).toBeDefined();
            const parsedError = JSON.parse(UpdatedQuestion)
            expect(parsedError.errors.Content[0]).toBeDefined();
        }
    });

    test('Validate PUT update question with empty content', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: ''
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(400);
    });

    test('Validate PUT update question content length exceeds maximum', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            content: "A".repeat(600) // Exceeds max length
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(400);
    });

    test('Validate PUT update question with invalid type', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        const updatePayload = {
            ...question,
            type: 999 as any // Invalid type
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(400);
    });

    test('Validate PUT update question with matching content to existing question maintains different IDs', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const duplicateContent = 'What is your age?';

        // Create a first question with specific content
        const {question: question1} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            duplicateContent,
            QuestionType.SINGLE,
            'First question description'
        );

        // Create a second question with different content initially
        const {question: question2} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            'What is your gender?',
            QuestionType.SINGLE,
            'Second question description'
        );

        // Store original question IDs
        const originalQuestion1Id = question1.id;
        const originalQuestion2Id = question2.id;

        // When I send a PUT request with Content matching an existing question
        const updatePayload = {
            ...question2,
            content: duplicateContent // Same content as question1
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question2.id,
            updatePayload
        );

        // Update should succeed
        expectHttpStatusCode(updatedQuestionPostResponse, 204);

        // Then the response payload should have a different Id to the existing question
        const {questionGetBody: updatedQuestion2} = await getQuestion(request, question2.id);
        const {questionGetBody: unchangedQuestion1} = await getQuestion(request, question1.id);

        // Verify both questions have the same content
        expect(updatedQuestion2.content).toBe(duplicateContent);
        expect(unchangedQuestion1.content).toBe(duplicateContent);
        expect(updatedQuestion2.content).toBe(unchangedQuestion1.content);

        // Verify IDs remain different (each question maintains its own identity)
        expect(updatedQuestion2.id).toBe(originalQuestion2Id);
        expect(unchangedQuestion1.id).toBe(originalQuestion1Id);
        expect(updatedQuestion2.id).not.toBe(unchangedQuestion1.id);

        // Additional validation: Verify they are distinct questions
        expect(updatedQuestion2.description).toBe('Second question description');
        expect(unchangedQuestion1.description).toBe('First question description');
    });

    test('Validate PUT update question with invalid questionnaire id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Attempt to update with a non-existent questionnaire ID
        const invalidQuestionnaireId = '00000000-0000-0000-0000-000000000000';
        const updatePayload = {
            ...question,
            questionnaireId: invalidQuestionnaireId,
            content: 'Updated content'
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            invalidQuestionnaireId,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect(updatedQuestionPostResponse.status()).toBe(404);
    });

    test('Validate PUT update question with soft deleted questionnaire id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Soft delete the questionnaire using the delete endpoint
        const {deleteQuestionnaireResponse} = await deleteQuestionnaire(request, questionnaire.id);
        expect([200, 204]).toContain(deleteQuestionnaireResponse.status());

        // Verify questionnaire is soft-deleted (should return 404)
        const {questionnaireGetResponse} = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetResponse.status()).toBe(404);

        // Attempt to update the question that belongs to the soft-deleted questionnaire
        const updatePayload = {
            ...question,
            content: 'Updated content',
        };

        const {updatedQuestionPostResponse} = await updateQuestion(
            request,
            question.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedQuestionPostResponse.ok()).toBeFalsy();
        expect([400, 404]).toContain(updatedQuestionPostResponse.status());
    });
});