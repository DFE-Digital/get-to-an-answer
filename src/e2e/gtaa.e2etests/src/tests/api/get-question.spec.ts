import {expect, test} from "@playwright/test";
import {createQuestionnaire, listQuestionnaires} from "../../test-data-seeder/questionnaire-data";
import {
    expect200HttpStatusCode, expectQuestionContent, expectQuestionIO, expectQuestionSchema, expectQuestionTypes
} from "../../helpers/api-assertions-helper";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestion, getQuestion, listQuestions} from "../../test-data-seeder/question-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('GET all questions api tests', () => {
    test('Validate GET list questions for a questionnaire', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        await createQuestion(request, questionnaireId);
        await createQuestion(request, questionnaireId);

        const response = await listQuestions(request, questionnaireId);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(response.questionGetResponse, 200);

        const list: any[] = response.questionGetBody;
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBeGreaterThan(0);

        const sample = list.slice(0, 10); //pick top 5 from list
        for (const s of sample) {
            // --- Schema-level checks ---
            expectQuestionSchema(s);

            // --- Type sanity checks ---
            expectQuestionTypes(s);

            // --- Basic content sanity ---
            expectQuestionContent(s);
        }
    });

    test('Validate GET all questions with invalid token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        await createQuestion(request, questionnaireId);

        const response = await listQuestions(request, questionnaireId, JwtHelper.InvalidToken);

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(401);
    });

    test('Validate GET all questions with expired token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        await createQuestion(request, questionnaireId);

        const response = await listQuestions(request, questionnaireId, JwtHelper.ExpiredToken);

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(401);
    });

    test('Validate GET all questions with invalid questionnaire id', async ({request}) => {
        const response = await listQuestions(request, '12345');

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(404);
    });

    test('Validate GET list questions where questions belongs to this questionnaire only', async ({request}) => {
        const q1Token = JwtHelper.NoRecordsToken();
        const q2Token = JwtHelper.NoRecordsToken();

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(request, q1Token);
        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(request, q2Token);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(q1Response, 201);
        expect200HttpStatusCode(q2Response, 201);

        const {question: question1} = await createQuestion(request, q1.id, q1Token);
        await createQuestion(request, q2.id, q2Token);

        const response = await listQuestions(request, q2.id, q2Token);

        // // --- HTTP-level checks ---
        expect200HttpStatusCode(response.questionGetResponse, 200);

        const list: any[] = response.questionGetBody
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBeGreaterThan(0);

        for (const currentQuestion of list) {
            expect(currentQuestion.id).not.toEqual(question1.id);
        }
    });

    test('Validate GET list questions where questionnaire exists but without questions', async ({request}) => {
        const q1Token = JwtHelper.NoRecordsToken();

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(request, q1Token);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(q1Response, 201);

        const response = await listQuestions(request, q1.id, q1Token);

        // // --- HTTP-level checks ---
        expect200HttpStatusCode(response.questionGetResponse, 200);

        const list: any[] = response.questionGetBody
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBe(0);
    });

    test('Validate GET a specific question for a questionnaire', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {question: question1, payload} = await createQuestion(request, questionnaireId);
        await createQuestion(request, questionnaireId);

        const response = await getQuestion(request, question1.id);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(response.questionGetResponse, 200);

        // --- Schema-level checks ---
        expectQuestionSchema(response.questionGetBody);

        // --- Type sanity checks ---
        expectQuestionTypes(response.questionGetBody);

        // --- Basic content sanity ---
        expectQuestionContent(response.questionGetBody);

        // --- I/O checks ---
        expectQuestionIO(response.questionGetBody, payload, GUID_REGEX);
    });

    test('Validate GET specific question that belongs to a questionnaire with no access', async ({request}) => {
        const q1Token = JwtHelper.NoRecordsToken();
        const q2Token = JwtHelper.NoRecordsToken();

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(request, q1Token);
        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(request, q2Token);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(q1Response, 201);
        expect200HttpStatusCode(q2Response, 201);

        const {question: question1} = await createQuestion(request, q1.id, q1Token);
        const {question: question2} = await createQuestion(request, q2.id, q2Token);

        const response = await getQuestion(request, question2.id, q1Token);

        // // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(403);
    });

    test('Validate GET specific question with invalid token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {question} = await createQuestion(request, questionnaireId);

        const response = await getQuestion(request, question.id, JwtHelper.InvalidToken);

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(401);
    });

    test('Validate GET specific question with expired token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {question} = await createQuestion(request, questionnaireId);

        const response = await getQuestion(request, question.id, JwtHelper.ExpiredToken);

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(401);
    });

    test('Validate GET specific question with invalid questionnaire id', async ({request}) => {
        const response = await getQuestion(request, '12345');

        // --- HTTP-level checks ---
        expect(response.questionGetResponse.ok()).toBeFalsy();
        expect(response.questionGetResponse.status()).toBe(404);
    });
    
    test('Validate GET questions for a questionnaire that has no questions returns an empty list', async ({ request }) => {
        const token = JwtHelper.NoRecordsToken();
        
        const { questionnaire } = await createQuestionnaire(request, token);

        // Get questions for the questionnaire that has no questions
        const response = await listQuestions(request, questionnaire.id, token);

        // --- Verify empty list ---
        const list: any[] = response.questionGetBody;
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBe(0);
    });
});