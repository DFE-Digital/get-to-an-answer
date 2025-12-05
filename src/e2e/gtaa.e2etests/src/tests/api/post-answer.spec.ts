import {expect, test} from "@playwright/test";
import {createQuestionnaire, deleteQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    expectAnswerContent, expectAnswerIO,
    expectAnswerSchema,
    expectAnswerTypes,
    expect200HttpStatusCode
} from "../../helpers/api-assertions-helper";
import {createQuestion, deleteQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {AnswerDestinationType, GUID_REGEX} from "../../constants/test-data-constants";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('POST answers', () => {
    test('POST an answer for a question in a questionnaire validations', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
            payload: questionnairePayload
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
            payload: questionPayload
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
            },
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(answerPostResponse, 201);

        // --- Schema-level checks ---
        expectAnswerSchema(answer);

        // --- Type sanity checks ---
        expectAnswerTypes(answer);

        // --- Basic content sanity ---
        expectAnswerContent(answer);

        // --- I/O checks ---
        expectAnswerIO(answer, payload, GUID_REGEX);
    });

    test('Validate POST create answer with missing required field - content', async ({request}) => {
        const nullContent: any = null;

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        const invalidPayload = {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: nullContent,
            description: 'Test description',
        };

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                ...invalidPayload
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);

        // --- Validation error checks ---
        const parsedError = JSON.parse(answer)
        expect(parsedError.errors.Content[0]).toBeDefined();
    });

    test('Validate POST create answer with invalid type field - content', async ({request}) => {
        const wrongTypeContent: any = 12345;

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        const invalidPayload = {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: wrongTypeContent,
            description: 'Test description',
        };

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                ...invalidPayload
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);

        // --- Validation error checks ---
        const parsedError = JSON.parse(answer)
        expect(parsedError.errors.request[0]).toBeDefined();
    });

    test('Validate POST create answer with invalid question id (question does not exist)', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        // Use a valid GUID format but for a question that doesn't exist
        const nonExistentQuestionId = '00000000-0000-0000-0000-000000000000';

        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: nonExistentQuestionId,
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(404);
    });

    test('Validate POST create answer with invalid JWT token', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Attempt to create answer with invalid JWT token
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
            },
            JwtHelper.InvalidToken
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(401);
    });

    test('Validate POST create answer with expired JWT token', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
            },
            JwtHelper.ExpiredToken
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(401);
    });

    test('Validate POST create answer with destination question id from different questionnaire', async ({request}) => {
        //first questionnaire
        const {
            questionnairePostResponse: questionnaire1Response,
            questionnaire: questionnaire1,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnaire1Response, 201);

        const {
            questionPostResponse: question1Response,
            question: question1,
        } = await createQuestion(request, questionnaire1.id);

        expect200HttpStatusCode(question1Response, 201);

        //second questionnaire
        const {
            questionnairePostResponse: questionnaire2Response,
            questionnaire: questionnaire2,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnaire2Response, 201);

        const {
            questionPostResponse: question2Response,
            question: question2,
        } = await createQuestion(request, questionnaire2.id);

        expect200HttpStatusCode(question2Response, 201);

        // Create an answer for question1 with destinationQuestionId pointing to question2 (from different questionnaire)
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question1.id,
                questionnaireId: questionnaire1.id,
                destinationQuestionId: question2.id, // This belongs to questionnaire2, not questionnaire1
                content: 'Test answer content',
                destinationType: AnswerDestinationType.Question,
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);
    });

    test('Validate POST create answer with cyclic destination (same question id) is prevented', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Attempt to create an answer with destination pointing to the same question (creates a cycle)
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                destinationQuestionId: question.id, // Same as questionId - creates a cycle
                content: 'Test answer content',
                destinationType: AnswerDestinationType.Question,
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400); //400 acceptable but ideally 409
    });

    test('Validate POST create answer for questionnaire user is not authorized for', async ({ request }) => {
        const authorizedToken = JwtHelper.ValidToken;
        const unauthorizedToken = JwtHelper.UnauthorizedToken;

        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request, authorizedToken);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id, authorizedToken);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Attempt to create an answer with a different user's token (not authorized for this questionnaire)
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
            },
            unauthorizedToken
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(403);
    });

    test('Validate POST create answer with empty description (optional field)', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Create answer with empty description
        const emptyDescription: any = '';

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
                description: emptyDescription,
            },
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(answerPostResponse, 201);

        // --- Schema-level checks ---
        expectAnswerSchema(answer);

        // --- Type sanity checks ---
        expectAnswerTypes(answer);

        // --- Basic content sanity ---
        expectAnswerContent(answer);

        // --- I/O checks ---
        expectAnswerIO(answer, payload, GUID_REGEX);

        // --- Verify an empty description is handled correctly ---
        expect(answer.description === '' || answer.description === null).toBeTruthy();
    });

    test('Validate POST create answer with invalid questionnaire id', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Use an invalid/non-existent questionnaire ID
        const invalidQuestionnaireId = '00000000-0000-0000-0000-000000000000';

        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: invalidQuestionnaireId, // Invalid questionnaire ID
                content: 'Test answer content',
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);
    });

    test('Validate POST create answer for soft deleted questionnaire', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Soft delete the questionnaire
        const {
            deleteQuestionnaireResponse,
        } = await deleteQuestionnaire(request, questionnaire.id);

        expect(deleteQuestionnaireResponse.ok()).toBeTruthy();

        // Attempt to create an answer for the soft-deleted questionnaire
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id, // Soft-deleted questionnaire
                content: 'Test answer content',
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(404);
    });

    test('Validate POST create answer for soft deleted question', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Soft delete the question
        const {
            deleteQuestionResponse,
        } = await deleteQuestion(request, question.id);

        expect(deleteQuestionResponse.ok()).toBeTruthy();

        // Attempt to create an answer for the soft-deleted question
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id, // Soft-deleted question
                questionnaireId: questionnaire.id,
                content: 'Test answer content',
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(404);
    });
    
    test('Validate POST create answer with content at maximum length (250 characters)', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Create an answer with exactly 250 characters (maximum allowed)
        const maxLengthContent = 'A'.repeat(250);

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: maxLengthContent,
            },
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(answerPostResponse, 201);
        
        // --- Business rule validation: Content length should be exactly 250 ---
        expect(answer.content).toBe(maxLengthContent);
        expect(answer.content.length).toBe(250);
    });

    test('Validate POST create answer with destination type Question and valid destinationQuestionId', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        // Create two questions - one will be the source, the other the destination
        const {
            questionPostResponse: question1Response,
            question: question1,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(question1Response, 201);

        const {
            questionPostResponse: question2Response,
            question: question2,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(question2Response, 201);

        // Create an answer with destination type Question and valid destinationQuestionId
        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question1.id,
                questionnaireId: questionnaire.id,
                content: 'Answer leading to another question',
                destinationType: AnswerDestinationType.Question,
                destinationQuestionId: question2.id, // Valid destination question
            },
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(answerPostResponse, 201);
        
        // --- Business rule validation: Destination should match ---
        expect(answer.destinationType).toBe(AnswerDestinationType.Question);
        expect(answer.destinationQuestionId).toBe(question2.id);
        expect(answer.destinationQuestionId).toMatch(GUID_REGEX);
    });

    test('Validate POST create answer with destination type ExternalLink and valid destinationUrl', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Create an answer with destination type ExternalLink and valid URL
        const externalUrl = 'https://www.gov.uk/government/organisations/department-for-education';

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            },
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(answerPostResponse, 201);
        
        // --- Business rule validation: Destination should match ---
        expect(answer.destinationType).toBe(AnswerDestinationType.ExternalLink);
        expect(answer.destinationUrl).toBe(externalUrl);
    });
    
    test('Validate POST create answer with destination type Question without destinationQuestionId', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Create an answer with destination-type as Question but no destinationQuestionId
        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Answer with invalid destination',
                destinationType: AnswerDestinationType.Question,
                // destinationQuestionId is missing
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);
    });

    test('Validate POST create answer with invalid URL format in destinationUrl', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Create an answer with invalid URL format
        const invalidUrl = 'not-a-valid-url';

        const {
            answerPostResponse,
            answer,
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Answer with invalid URL',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: invalidUrl,
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);
    });
    
    test('Validate POST create answer with destinationUrl exceeding maximum length (250 characters)', async ({ request }) => {
        const {
            questionnairePostResponse,
            questionnaire,
        } = await createQuestionnaire(request);

        expect200HttpStatusCode(questionnairePostResponse, 201);

        const {
            questionPostResponse,
            question,
        } = await createQuestion(request, questionnaire.id);

        expect200HttpStatusCode(questionPostResponse, 201);

        // Attempt to create an answer with destinationUrl exceeding 250 characters (251 characters)
        const exceedingLengthUrl = 'https://example.com/' + 'a'.repeat(232); // Total = 251 characters

        const {
            answerPostResponse,
            answer,
            payload
        } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId: questionnaire.id,
                content: 'Answer with exceeding URL length',
                destinationUrl: exceedingLengthUrl,
                destinationType: AnswerDestinationType.ExternalLink,
            },
        );

        // --- HTTP-level checks ---
        expect(answerPostResponse.ok()).toBeFalsy();
        expect(answerPostResponse.status()).toBe(400);

        const errorBody = await answerPostResponse.json();

        // --- Business rule validation: Should fail due to URL length exceeding limit ---
        expect(errorBody.errors.DestinationUrl[0]).toBe("The field DestinationUrl must be a string or array type with a maximum length of '250'.");
    });
});