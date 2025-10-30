import {expect, test} from "@playwright/test";
import {createQuestion, updateQuestion} from "../../test-data-seeder/question-data";
import {
    expect200HttpStatusCode, expectQuestionTypes, expectQuestionContent, expectQuestionIO,
    expectQuestionSchema
} from "../../helpers/api-assertions-helper";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('POST Create question api request', () => {
    test('Validate POST create new question', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {
            questionPostResponse,
            question,
            payload
        } = await createQuestion(request, questionnaireId);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(questionPostResponse, 201);

        // --- Schema-level checks ---
        expectQuestionSchema(question);

        // --- Type sanity checks ---
        expectQuestionTypes(question);

        // --- Basic content sanity ---
        expectQuestionContent(question);

        // --- I/O checks ---
        expectQuestionIO(question, payload, GUID_REGEX);
    });

    test('Validate POST create new question with invalid token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            JwtHelper.InvalidToken,
            undefined,
            1
        );

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(401);
    });

    test('Validate POST create new question with expired token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            JwtHelper.ExpiredToken,
            'some content',
            1
        );

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(401);
    });

    test('Validate POST create new question with invalid payload', async ({request}) => {
        const wrongTypeContent: any = 12345;

        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;


        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            wrongTypeContent,
            1
        );

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(400);
    });

    test('Validate POST create new question with missing content', async ({request}) => {
        const nullContent: any = null;

        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            nullContent,
            1
        );

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(400);
    });

    test('Validate POST create new question with missing type', async ({request}) => {
        const nullType: any = null;

        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            undefined,
            nullType
        );

        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(400);
    });

    test('Validate POST create new question with missing description', async ({request}) => {
        const nullDescription: any = null;

        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            undefined,
            1,
            nullDescription
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(questionPostResponse, 201);
    });

    test('Validate POST specific question with invalid questionnaire id', async ({request}) => {
        const response = await createQuestion(
            request,
            '123456',
            undefined,
            undefined,
            1
        );

        // --- HTTP-level checks ---
        expect(response.questionPostResponse.ok()).toBeFalsy();
        expect(response.questionPostResponse.status()).toBe(400);
    });

    test('Validate content length POST create new question', async ({request}) => {

        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = questionnaire.id;

        const {questionPostResponse} = await createQuestion(
            request,
            questionnaireId,
            undefined,
            "A".repeat(600), // Exceeds max length
            1
        );
        
        // --- HTTP-level checks ---
        expect(questionPostResponse.ok()).toBeFalsy();
        expect(questionPostResponse.status()).toBe(400);
    });

    test('Validate access to another questionnaire not permitted', async ({request}) => {

        const q1Token = JwtHelper.ValidToken;
        const q2Token = JwtHelper.UnauthorizedToken;

        //create questionnaire1 for user1
        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(request, q1Token);

        //create questionnaire2 for user2
        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(request, q2Token,)

        // --- HTTP-level checks ---
        expect200HttpStatusCode(q1Response, 201);
        expect200HttpStatusCode(q2Response, 201);

        //create question1 for questionnaire1
        const {question: question1} = await createQuestion(request, q1.id, q1Token);

        //create question2 for questionnaire2
        const {question: question2} = await createQuestion(request, q2.id, q2Token);

        //update payload for question2
        const updateQuestionPayload = {
            ...question2,
            content: 'Updated content'
        }

        //update question2 with user1 - not permitted to update
        const update = await updateQuestion(
            request,
            question2.id,
            updateQuestionPayload,
            q1Token //user1
        )

        //should be forbidden
        expect(update.updatedQuestionPostResponse.status()).toBe(403);
    });
});