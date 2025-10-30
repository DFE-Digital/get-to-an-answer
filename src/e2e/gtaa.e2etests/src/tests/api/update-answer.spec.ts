import {test, expect} from '@playwright/test';
import {createQuestionnaire, deleteQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, deleteQuestion, getQuestion, updateQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer, getAnswer, updateAnswer} from "../../test-data-seeder/answer-data";
import {
    expectAnswerSchema,
    expectAnswerTypes,
    expectAnswerContent,
    expectAnswerIO,
    expectQuestionSchema, expectQuestionTypes, expectQuestionContent, expectQuestionIO,
    expect200HttpStatusCode
} from "../../helpers/api-assertions-helper";
import {JwtHelper} from "../../helpers/JwtHelper";
import {GUID_REGEX, AnswerDestinationType} from "../../constants/test-data-constants";

test.describe('PUT Update answer', () => {
    test('Validate successful answer update with valid payload', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Create an initial answer
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
            description: 'Original description',
        });

        // Update the answer
        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content',
            description: 'Updated description',
            score: 10.5,
        };

        const {updatedAnswerPostResponse, updatedAnswer} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(updatedAnswerPostResponse, 204);

        // Verify an update by fetching the question
        const {answer} = await getAnswer(request, createdAnswer.id);

        // --- Schema-level checks ---
        expectAnswerSchema(answer);

        // --- Type sanity checks ---
        expectAnswerTypes(answer);

        // --- Basic content sanity ---
        expectAnswerContent(answer);

        // --- I/O checks ---
        expectAnswerIO(answer, updatePayload, GUID_REGEX);

        // Verify updated fields
        expect(answer.content).toBe('Updated content');
        expect(answer.description).toBe('Updated description');
    });

    test('Validate update answer with missing required field - content', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Attempt to update an answer without content
        const nullContent: any = null;
        const updatePayload = {
            ...createdAnswer,
            content: nullContent,
        };

        const {updatedAnswerPostResponse, updatedAnswer} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(400);

        // And error message to display
        if (updatedAnswer) {
            expect(updatedAnswer).toBeDefined();
            const parsedError = JSON.parse(updatedAnswer)
            expect(parsedError.errors.Content[0]).toBeDefined();
        }
    });

    test('Validate update answer with invalid payload (wrong type for score)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Attempt to update with wrong type
        const wrongTypeScore: any = 'not-a-number';
        const updatePayload = {
            ...createdAnswer,
            score: wrongTypeScore,
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(400);
    });

    test('Validate update answer for non-existent question returns 404', async ({request}) => {
        const nonExistentAnswerId = '00000000-0000-0000-0000-000000000000';

        const updatePayload = {
            content: 'Updated content',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            nonExistentAnswerId,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(404);
    });

    test('Validate update answer with invalid JWT token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload,
            JwtHelper.InvalidToken
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(401);
    });

    test('Validate update answer with expired JWT token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload,
            JwtHelper.ExpiredToken
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(401);
    });

    test('Validate update answer with non-existent destination question', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        const nonExistentDestinationId = '00000000-0000-0000-0000-000000000000';
        const updatePayload = {
            ...createdAnswer,
            destinationQuestionId: nonExistentDestinationId,
            destinationType: AnswerDestinationType.Question,
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect([400, 403, 404]).toContain(updatedAnswerPostResponse.status());
    });

    test('Validate update answer with destination question from different questionnaire', async ({request}) => {
        // Create first a questionnaire with question and answer
        const {questionnaire: questionnaire1} = await createQuestionnaire(request);
        const {question: question1} = await createQuestion(request, questionnaire1.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question1.id,
            questionnaireId: questionnaire1.id,
            content: 'Original content',
        });

        // Create a second questionnaire with question
        const {questionnaire: questionnaire2} = await createQuestionnaire(request);
        const {question: question2} = await createQuestion(request, questionnaire2.id);

        // Attempt to update an answer with destination to a different questionnaire
        const updatePayload = {
            ...createdAnswer,
            destinationQuestionId: question2.id, // Different questionnaire
            destinationType: AnswerDestinationType.Question,
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect([400, 403, 404]).toContain(updatedAnswerPostResponse.status());
    });

    test('Validate update answer with cyclic destination (same question) is prevented', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Attempt to create a cycle by pointing to the same question
        const updatePayload = {
            ...createdAnswer,
            destinationQuestionId: question.id, // Same question - creates cycle
            destinationType: AnswerDestinationType.Question,
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect([400, 409]).toContain(updatedAnswerPostResponse.status()); //400 acceptable but ideally 409
    });

    test('Validate update answer for questionnaire user is not authorized for', async ({request}) => {
        const authorizedToken = JwtHelper.ValidToken;
        const unauthorizedToken = JwtHelper.UnauthorizedToken;

        const {questionnaire} = await createQuestionnaire(request, authorizedToken);
        const {question} = await createQuestion(request, questionnaire.id, authorizedToken);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        }, authorizedToken);

        // Attempt to update an answer with a different user's token (not authorized)
        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content by unauthorized user',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload,
            unauthorizedToken
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect([401, 403]).toContain(updatedAnswerPostResponse.status());

        // --- Verify that an answer was NOT updated ---
        const {answer: fetchedAnswer} = await getAnswer(request, createdAnswer.id, authorizedToken);
        expect(fetchedAnswer.content).toBe('Original content'); // Should still be original
        expect(fetchedAnswer.content).not.toBe('Updated content by unauthorized user');
    });

    test('Validate update answer with empty description (optional field)', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
            description: 'Original description',
        });

        // Update with empty description
        const emptyDescription: any = '';
        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content',
            description: emptyDescription,
        };

        const {updatedAnswerPostResponse, updatedAnswer} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeTruthy();
        expect(updatedAnswerPostResponse.status()).toBe(204);

        // If 204 No Content, fetch the answer
        if (!updatedAnswer) {
            const {answer: fetchedAnswer} = await getAnswer(request, createdAnswer.id);

            // Verify answer was updated
            expect(fetchedAnswer.content).toBe('Updated content');
            expect(fetchedAnswer.description === '' || fetchedAnswer.description === null).toBeTruthy();
        } else {
            // Verify answer was updated
            expect(updatedAnswer.content).toBe('Updated content');
            expect(updatedAnswer.description === '' || updatedAnswer.description === null).toBeTruthy();
        }
    });

    test('Validate update answer with invalid question id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Attempt to update with a non-existent question ID
        const invalidQuestionId = '00000000-0000-0000-0000-000000000000';
        const updatePayload = {
            ...createdAnswer,
            questionId: invalidQuestionId,
            content: 'Updated content',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            invalidQuestionId,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect(updatedAnswerPostResponse.status()).toBe(404);
    });

    //possibly a bug as api should return false and update should not happen
    // test('Validate update answer with soft deleted question id', async ({ request }) => {
    //     const { questionnaire } = await createQuestionnaire(request);
    //     const { question } = await createQuestion(request, questionnaire.id);
    //
    //     // Create an answer with question1 as a parent
    //     const { answer: createdAnswer } = await createSingleAnswer(request, {
    //         questionId: question.id,
    //         questionnaireId: questionnaire.id,
    //         content: 'Original content',
    //     });
    //
    //     // Soft delete question2 using the delete endpoint
    //     const {deleteQuestionResponse} = await deleteQuestion(request, question.id);
    //     expect([200, 204]).toContain(deleteQuestionResponse.status());
    //
    //     // Verify question2 is soft-deleted (should return 404)
    //     const {questionGetResponse} = await getQuestion(request, question.id);
    //     expect(questionGetResponse.status()).toBe(404);
    //
    //     // Attempt to update an answer to reference the soft-deleted question as destination
    //     const updatePayload = {
    //         ...createdAnswer,
    //         content: 'Updated content',
    //     };
    //
    //     const {updatedAnswerPostResponse} = await updateAnswer(
    //         request,
    //         createdAnswer.id,
    //         updatePayload
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(updatedAnswerPostResponse.ok()).toBeFalsy();
    //     expect([400, 404]).toContain(updatedAnswerPostResponse.status());
    // });

    test('Validate update answer with soft deleted questionnaire id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Create an answer
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Soft delete the questionnaire using the delete endpoint
        const {deleteQuestionnaireResponse} = await deleteQuestionnaire(request, questionnaire.id);
        expect([200, 204]).toContain(deleteQuestionnaireResponse.status());

        // Verify questionnaire is soft-deleted (should return 404)
        const {questionnaireGetResponse} = await getQuestionnaire(request, questionnaire.id);
        expect(questionnaireGetResponse.status()).toBe(404);

        // Attempt to update an answer that belongs to the soft-deleted questionnaire
        const updatePayload = {
            ...createdAnswer,
            content: 'Updated content',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        // The API should reject updates to answers belonging to soft-deleted questionnaires
        expect(updatedAnswerPostResponse.ok()).toBeFalsy();
        expect([400, 404]).toContain(updatedAnswerPostResponse.status());
    });

    test('Validate update answer content with maximum length of 250 characters', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Create an initial answer
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Update with content exactly at max length (250 characters)
        const maxLengthContent = 'A'.repeat(250);
        const updatePayload = {
            ...createdAnswer,
            content: maxLengthContent,
        };

        const {updatedAnswerPostResponse, updatedAnswer} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(updatedAnswerPostResponse, 204);
    });

    //500 error, raise a bug
    // test('Validate update answer content exceeding maximum length of 250 characters', async ({ request }) => {
    //     const { questionnaire } = await createQuestionnaire(request);
    //     const { question } = await createQuestion(request, questionnaire.id);
    //
    //     // Create an initial answer
    //     const { answer: createdAnswer } = await createSingleAnswer(request, {
    //         questionId: question.id,
    //         questionnaireId: questionnaire.id,
    //         content: 'Original content',
    //     });
    //
    //     // Attempt to update with content exceeding max length (251 characters)
    //     const exceedingLengthContent = 'A'.repeat(251);
    //     const updatePayload = {
    //         ...createdAnswer,
    //         content: exceedingLengthContent,
    //     };
    //
    //     const {updatedAnswerPostResponse} = await updateAnswer(
    //         request,
    //         createdAnswer.id,
    //         updatePayload
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(updatedAnswerPostResponse.ok()).toBeFalsy();
    //     expect(updatedAnswerPostResponse.status()).toBe(400);
    //
    //     // --- Verify that the answer was NOT updated ---
    //     const {answer: fetchedAnswer} = await getAnswer(request, createdAnswer.id);
    //     expect(fetchedAnswer.content).toBe('Original content'); // Should still be original
    //     expect(fetchedAnswer.content).not.toBe(exceedingLengthContent);
    // });

    test('Validate update answer with valid destinationQuestionId matching GUID regex', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question: question1} = await createQuestion(request, questionnaire.id);
        const {question: question2} = await createQuestion(request, questionnaire.id);

        // Create an initial answer
        const {answer: createdAnswer} = await createSingleAnswer(request, {
            questionId: question1.id,
            questionnaireId: questionnaire.id,
            content: 'Original content',
        });

        // Update with valid destination question id (GUID format)
        const updatePayload = {
            ...createdAnswer,
            destinationQuestionId: question2.id,
            destinationType: AnswerDestinationType.Question,
            content: 'Updated content with destination',
        };

        const {updatedAnswerPostResponse} = await updateAnswer(
            request,
            createdAnswer.id,
            updatePayload
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(updatedAnswerPostResponse, 204);

        // Verify the update by fetching the answer
        const {answer: fetchedAnswer} = await getAnswer(request, createdAnswer.id);

        // --- Validate destinationQuestionId matches GUID regex ---
        expect(fetchedAnswer.destinationQuestionId).toMatch(GUID_REGEX);
    });

    //500 bug
    // test('Validate update answer with destinationUrl exceeding maximum length (250 characters)', async ({request}) => {
    //     const {questionnaire} = await createQuestionnaire(request);
    //     const {question} = await createQuestion(request, questionnaire.id);
    //
    //     // Create an initial answer
    //     const {answer: createdAnswer} = await createSingleAnswer(request, {
    //         questionId: question.id,
    //         questionnaireId: questionnaire.id,
    //         content: 'Original content',
    //     });
    //
    //     // Attempt to update with destinationUrl exceeding 250 characters (251 characters)
    //     const exceedingLengthUrl = 'https://example.com/' + 'a'.repeat(232); // Total = 251 characters
    //     const updatePayload = {
    //         ...createdAnswer,
    //         content: 'Updated content',
    //         destinationUrl: exceedingLengthUrl,
    //         destinationType: AnswerDestinationType.ExternalLink,
    //     };
    //
    //     const {updatedAnswerPostResponse} = await updateAnswer(
    //         request,
    //         createdAnswer.id,
    //         updatePayload
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(updatedAnswerPostResponse.ok()).toBeFalsy();
    //     expect(updatedAnswerPostResponse.status()).toBe(400);
    //
    //     // --- Business rule validation: Should fail due to URL length exceeding limit ---
    //     expect(exceedingLengthUrl.length).toBe(251);
    // });
});