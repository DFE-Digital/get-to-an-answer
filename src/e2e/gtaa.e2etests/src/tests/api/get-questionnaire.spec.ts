import {test, expect} from '@playwright/test';
import {
    createQuestionnaire,
    getQuestionnaire, deleteQuestionnaire, listQuestionnaires
} from "../../test-data-seeder/questionnaire-data";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {JwtHelper} from '../../helpers/JwtHelper';
import {
    expectHttpStatusCode,
    expectQuestionnaireSchema,
    expectQuestionnaireContent,
    expectQuestionnaireInitStateTypes, expectQuestionnaireInitStateIO
} from '../../helpers/api-assertions-helper'

test.describe('GET questionnaire api tests', () => {
    test('Validate GET specific questionnaire', async ({request}) => {
        const {questionnaire, payload} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

        const response = await getQuestionnaire(request, questionnaireId);

        // --- HTTP-level checks ---
        expectHttpStatusCode(response.questionnaireGetResponse, 200);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(response.questionnaireGetBody);

        // --- Type sanity checks ---
        expectQuestionnaireInitStateTypes(response.questionnaireGetBody);

        // --- Basic content sanity ---
        expectQuestionnaireContent(response.questionnaireGetBody);

        // --- I/O checks ---
        expectQuestionnaireInitStateIO(response.questionnaireGetBody, payload, GUID_REGEX);
    });

    test('Validate GET specific questionnaire with invalid token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

        const response = await getQuestionnaire(
            request,
            questionnaireId,
            JwtHelper.InvalidToken
        );

        // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(401);
    });

    test('Validate GET specific questionnaire with expired token', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

        const response = await getQuestionnaire(
            request,
            questionnaireId,
            JwtHelper.ExpiredToken
        );

        // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(401);
    });

    test('Validate GET specific questionnaire with invalid questionnaire id', async ({request}) => {
        const response = await getQuestionnaire(
            request,
            12345,
            JwtHelper.ExpiredToken
        );

        // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(401);
    });

    test('Validate GET for a deleted questionnaire ', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

        const token = JwtHelper.ValidToken;

        const response = await deleteQuestionnaire(
            request,
            questionnaireId,
            token
        )

        expect(response.deleteQuestionnaireResponse.status()).toBe(204);

        const getResponse = await getQuestionnaire(
            request,
            questionnaireId,
            token
        );

        // // --- HTTP-level checks ---
        expect(getResponse.questionnaireGetResponse.ok()).toBeFalsy();
        expect(getResponse.questionnaireGetResponse.status()).toBe(404);
    });

    test('Validate GET for a specific questionnaire where access is not permitted ', async ({request}) => {
        const q1Token = JwtHelper.ValidToken;
        const q2Token = JwtHelper.UnauthorizedToken;

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(request, q1Token);
        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(request, q2Token);

        // --- HTTP-level checks ---
        expectHttpStatusCode(q1Response, 201);
        expectHttpStatusCode(q2Response, 201);

        const response = await getQuestionnaire(
            request,
            q1.id,
            q2Token
        );

        // // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(403);
    });

    test('Validate GET all questionnaires', async ({request}) => {
        const qToken = JwtHelper.ValidToken;

        await createQuestionnaire(request, qToken);
        await createQuestionnaire(request, qToken);

        const response = await listQuestionnaires(request, qToken);

        // --- HTTP-level checks ---
        expectHttpStatusCode(response.questionnaireGetResponse, 200);

        const list: any[] = response.questionnaireGetBody
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBeGreaterThan(0);

        const sample = list.slice(0, 10); //pick top 5 from list
        for (const q of sample) {

            // --- Schema-level checks ---
            expectQuestionnaireSchema(q);

            // --- Type sanity checks ---
            expectQuestionnaireInitStateTypes(q);

            // --- Basic content sanity ---
            expectQuestionnaireContent(q);
        }
    });

    test('Validate GET should not include questionnaire that is not permitted in list questionnaires', async ({request}) => {
        const q1Token = JwtHelper.NoRecordsToken();
        const q2Token = JwtHelper.NoRecordsToken();

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
            request,
            q1Token,
            'Custom test questionnaire title - user 1'
        );

        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
            request,
            q2Token,
            'Custom test questionnaire title - user 2'
        );

        // --- HTTP-level checks ---
        expectHttpStatusCode(q1Response, 201);
        expectHttpStatusCode(q2Response, 201);

        const response = await listQuestionnaires(request, q2Token);

        // // --- HTTP-level checks ---
        expectHttpStatusCode(response.questionnaireGetResponse, 200);

        const list: any[] = response.questionnaireGetBody
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBeGreaterThan(0);

        for (const q of list) {
            expect(q.id).not.toEqual(q1.id);
        }
    });

    test('Validate GET all should return empty list if no questionnaire exists', async ({request}) => {

        const response = await listQuestionnaires(request, JwtHelper.NoRecordsToken());

        // // --- HTTP-level checks ---
        expectHttpStatusCode(response.questionnaireGetResponse, 200);

        const list: any[] = response.questionnaireGetBody
        expect(Array.isArray(list)).toBe(true);
        expect(list.length).toBe(0);
    });

    test('Validate GET all questionnaire with invalid token', async ({request}) => {
        const response = await listQuestionnaires(request, JwtHelper.InvalidToken);

        // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(401);
    });

    test('Validate GET all questionnaire with expired token', async ({request}) => {
        const response = await listQuestionnaires(request, JwtHelper.ExpiredToken);

        // --- HTTP-level checks ---
        expect(response.questionnaireGetResponse.ok()).toBeFalsy();
        expect(response.questionnaireGetResponse.status()).toBe(401);
    });
});