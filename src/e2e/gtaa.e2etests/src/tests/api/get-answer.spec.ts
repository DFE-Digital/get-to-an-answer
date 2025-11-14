import {test, expect} from '@playwright/test';
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer, getAnswer, listAnswers} from "../../test-data-seeder/answer-data";
import {
    expect200HttpStatusCode,
    expectAnswerSchema,
    expectAnswerTypes,
    expectAnswerContent
} from "../../helpers/api-assertions-helper";
import {JwtHelper} from "../../helpers/JwtHelper";
import {GUID_REGEX, AnswerDestinationType} from "../../constants/test-data-constants";

test.describe('GET answers for a question', () => {
    test('Validate GET answers for question with no answers returns empty list', async ({request}) => {
        const token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        const {question} = await createQuestion(request, questionnaire.id, token);

        // Get answers for a question that has no answers
        const response = await listAnswers(request, question.id, token);

        // --- Verify empty list ---
        const list: any[] = response.answers;
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBe(0);
    });

    test('Validate GET answers with invalid JWT token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Attempt to get answers with invalid token
        const response = await getAnswer(request, question.id, JwtHelper.InvalidToken);

        // --- HTTP-level checks ---
        expect(response.response.ok()).toBeFalsy();
        expect(response.response.status()).toBe(401);
    });

    test('Validate GET answers with expired JWT token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Attempt to get answers with expired token
        const response = await getAnswer(request, question.id, JwtHelper.ExpiredToken);

        // --- HTTP-level checks ---
        expect(response.response.ok()).toBeFalsy();
        expect(response.response.status()).toBe(401);
    });

    test('Validate GET answers for non-existent question returns 404', async ({request}) => {
        // Use a non-existent question ID
        const nonExistentQuestionId = '00000000-0000-0000-0000-000000000000';

        const response = await getAnswer(request, nonExistentQuestionId);

        // --- HTTP-level checks ---
        expect(response.response.ok()).toBeFalsy();
        expect(response.response.status()).toBe(404);
    });

    test('Validate GET answers returns only answers for the specific question (no cross-leakage)', async ({request}) => {
        const {questionnaire: questionnaire1} = await createQuestionnaire(request);
        const {question: question1} = await createQuestion(request, questionnaire1.id);

        await createSingleAnswer(request, {
            questionId: question1.id,
            questionnaireId: questionnaire1.id,
            content: 'Answer 1 for Question 1',
        });

        await createSingleAnswer(request, {
            questionId: question1.id,
            questionnaireId: questionnaire1.id,
            content: 'Answer 2 for Question 1',
        });

        // Create second questionnaire with question and answers
        const {questionnaire: questionnaire2} = await createQuestionnaire(request);
        const {question: question2} = await createQuestion(request, questionnaire2.id);

        await createSingleAnswer(request, {
            questionId: question2.id,
            questionnaireId: questionnaire2.id,
            content: 'Answer 1 for Question 2',
        });

        // Get answers for question 1
        const responseQuestion1 = await listAnswers(request, question1.id);

        // --- Verify correct answers returned for question 1 ---
        expect(responseQuestion1.answersGetResponse.ok()).toBeTruthy();
        expect(responseQuestion1.answers.length).toBe(2);

        // All answers should belong to question1
        responseQuestion1.answers.forEach((answer: any) => {
            expect(answer.questionId).toBe(question1.id);
            expect(answer.questionnaireId).toBe(questionnaire1.id);
            expectAnswerSchema(answer);
            expectAnswerTypes(answer);
            expectAnswerContent(answer);
        });

        // Get answers for question 2
        const answersForQuestion2 = await listAnswers(request, question2.id);

        // --- Verify correct answers returned for question 2 ---
        expect(answersForQuestion2.answersGetResponse.ok()).toBeTruthy();
        expect(answersForQuestion2.answers.length).toBe(1);

        // All answers should belong to question2
        answersForQuestion2.answers.forEach((answer: any) => {
            expect(answer.questionId).toBe(question2.id);
            expect(answer.questionnaireId).toBe(questionnaire2.id);
            expectAnswerSchema(answer);
            expectAnswerTypes(answer);
            expectAnswerContent(answer);
        });
    });

    test('Validate GET answers for invalid question ID format returns 400', async ({request}) => {
        // Use an invalid GUID format
        const invalidQuestionId = 'not-a-valid-guid';

        const response = await getAnswer(request, invalidQuestionId);

        // --- HTTP-level checks ---
        expect(response.response.ok()).toBeFalsy();
        expect(response.response.status()).toBe(404);
    });

    test('Validate GET answers returns multiple answers with correct schema and content', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const {question} = await createQuestion(request, questionnaire.id);

        // Create multiple answers
        const answer1 = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'First answer option',
            description: 'First description',
        });

        const answer2 = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Second answer option',
            description: 'Second description',
        });

        const answer3 = await createSingleAnswer(request, {
            questionId: question.id,
            questionnaireId: questionnaire.id,
            content: 'Third answer option',
        });

        // Get all answers
        const response = await listAnswers(request, question.id);

        // --- Verify all answers returned ---
        expect(response.answersGetResponse.ok()).toBeTruthy();
        expect(response.answers.length).toBe(3);

        // --- Verify each answer has a correct schema, types, and content ---
        response.answers.forEach((answer: any) => {
            expect(answer.questionId).toBe(question.id);
            expect(answer.questionnaireId).toBe(questionnaire.id);
            expect(answer.id).toMatch(GUID_REGEX);
            expectAnswerSchema(answer);
            expectAnswerTypes(answer);
            expectAnswerContent(answer);
        });
    });
});