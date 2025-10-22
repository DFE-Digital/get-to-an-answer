import {test, expect} from '@playwright/test';
import {
    createQuestionnaire,
    updateQuestionnaire,
    getQuestionnaire, deleteQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {ClaimTypes, JwtHelper, SimpleDate} from '../../helpers/JwtHelper';
import {
    expectHttp,
    expectQuestionnaireSchema,
    expectQuestionnaireTypes,
    expectQuestionnaireContent,
    expectQuestionnaireIO
} from '../../helpers/api-assertions-helper'

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request);

        // --- HTTP-level checks ---
        expectHttp(questionnairePostResponse, 201);

        // // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireTypes(questionnaire);

        // --- Basic content sanity ---
        expectQuestionnaireContent(questionnaire);

        // --- I/O checks ---
        expectQuestionnaireIO(questionnaire, payload, GUID_REGEX);
    });

    test('Validate POST create new questionnaire with Title only', async ({request}) => {
        const nullDescription: any = null;
        const nullSlug: any = null;

        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(
            request,
            undefined,
            'TitleOnly',
            nullDescription,
            nullSlug
        );

        // --- HTTP-level checks ---
        expectHttp(questionnairePostResponse, 201);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireTypes(questionnaire);

        // --- Basic content sanity ---
        expectQuestionnaireContent(questionnaire);

        // --- I/O checks ---
        expectQuestionnaireIO(questionnaire, payload, GUID_REGEX);
    });

    test('Validate POST create new questionnaire with Title and Description only', async ({request}) => {
        const nullSlug: any = null;

        const {questionnairePostResponse, questionnaire, payload} = await createQuestionnaire(
            request,
            undefined,
            'TitleAndDescriptionOnly',
            'Custom test questionnaire description',
            nullSlug
        );

        // --- HTTP-level checks ---
        expectHttp(questionnairePostResponse, 201);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireTypes(questionnaire);

        // --- Basic content sanity ---
        expectQuestionnaireContent(questionnaire);

        // --- I/O checks ---
        expectQuestionnaireIO(questionnaire, payload, GUID_REGEX);
    });

    test('Validate POST create new questionnaire with matching Title to another questionnaire', async ({request}) => {

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
            request,
            undefined,
            'Custom test questionnaire title',
            'Custom test first questionnaire description',
            undefined
        );

        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
            request,
            undefined,
            'Custom test questionnaire title',
            'Custom test second questionnaire description',
            undefined
        );

        // --- HTTP-level checks ---
        expectHttp(q1Response, 201);
        expectHttp(q2Response, 201);

        // --- Content sanity ---
        expect(q1.title).toBe(q2.title);
        expect(q1.id).not.toBe(q2.id)
    });

    test('Validate POST create new questionnaire with missing Title', async ({request}) => {
        const nullTitle: any = null;

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            undefined,
            nullTitle,
            'Custom test questionnaire description',
            'Custom-slud'
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(400);
    });

    test('Validate POST create new questionnaire with invalid payload', async ({request}) => {
        const wrongTypeDescription: any = 12345;

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            undefined,
            'Custom test questionnaire title',
            wrongTypeDescription,
            undefined
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(400);
    });

    test('Validate POST create new questionnaire with invalid token', async ({request}) => {
        const wrongTypeDescription: any = 12345;

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            JwtHelper.InvalidToken
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(401);
    });

    test('Validate POST create new questionnaire with expired token', async ({request}) => {
        const wrongTypeDescription: any = 12345;

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            JwtHelper.ExpiredToken
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(401);
    });

    test('Validate title length POST create new questionnaire', async ({request}) => {

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            undefined,
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghij" +
            "klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXY" +
            "ZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOopioidfjsdlfjsdjflksdfjsjfsdlkfsjdfsjfjsdfjsdjfsdlfj" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghij" +
            "klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXY" +
            "ZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOopioidfjsdlfjsdjflksdfjsjfsdlkfsjdfsjfjsdfjsdjfsdlfj"
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(400);
    });

    // test('Validate slug length POST create new questionnaire', async ({request}) => {
    //
    //     const {questionnairePostResponse} = await createQuestionnaire(
    //         request,
    //         undefined,
    //         undefined,
    //         undefined,
    //         "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghij" +
    //         "klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXY" +
    //         "ZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOopioidfjsdlfjsdjflksdfjsjfsdlkfsjdfsjfjsdfjsdjfsdlfj" +
    //         "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghij" +
    //         "klmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXY" +
    //         "ZabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOopioidfjsdlfjsdjflksdfjsjfsdlkfsjdfsjfjsdfjsdjfsdlfj"
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(questionnairePostResponse.ok()).toBeFalsy();
    //     expect(questionnairePostResponse.status()).toBe(400);
    // });

    // test('Validate access to another questionnaire not permitted', async ({request}) => {
    //
    //     const q1Token = JwtHelper.ValidToken;
    //     const q2Token = JwtHelper.UnauthorizedToken;
    //
    //     const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
    //         request,
    //         q1Token,
    //         'Custom test questionnaire title',
    //         'Custom test first questionnaire description',
    //         'slug'
    //     );
    //
    //     const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
    //         request,
    //         q2Token,
    //         'Custom test questionnaire title',
    //         'Custom test second questionnaire description',
    //         'slug'
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expectHttp(q1Response, 201);
    //     expectHttp(q2Response, 201);
    //
    //     const updatePayload = {
    //         ...q2,
    //         title: 'Updated title'
    //     }
    //     const update = await updateQuestionnaire(
    //         request,
    //         q2.id,
    //         updatePayload,
    //         q1Token
    //     )
    //
    //     //should be forbidden
    //     expect(update.updatedQuestionnairePostResponse.status()).toBe(403);
    //
    // });
});

test.describe('GET questionnaire api tests', () => {
    test('Validate GET questionnaire', async ({request}) => {
        const {questionnaire, payload} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

        const response = await getQuestionnaire(request, questionnaireId);

        // --- HTTP-level checks ---
        expectHttp(response.questionnaireGetResponse, 200);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(response.questionnaireGetBody);

        // --- Type sanity checks ---
        expectQuestionnaireTypes(response.questionnaireGetBody);

        // --- Basic content sanity ---
        expectQuestionnaireContent(response.questionnaireGetBody);

        // --- I/O checks ---
        expectQuestionnaireIO(response.questionnaireGetBody, payload, GUID_REGEX);
    });

    test('Validate GET questionnaire with invalid token', async ({request}) => {
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

    test('Validate GET questionnaire with expired token', async ({request}) => {
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

    test('Validate GET questionnaire with invalid questionnaire id', async ({request}) => {
        const {questionnaire} = await createQuestionnaire(request);
        const questionnaireId = await questionnaire.id;

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

    // test('Validate GET for a questionnaire where access is not permitted ', async ({request}) => {
    //     const q1Token = JwtHelper.ValidToken;
    //     const q2Token = JwtHelper.UnauthorizedToken;
    //
    //     const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
    //         request,
    //         q1Token,
    //         'Custom test questionnaire title',
    //         'Custom test first questionnaire description',
    //         'slug'
    //     );
    //
    //     const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
    //         request,
    //         q2Token,
    //         'Custom test questionnaire title',
    //         'Custom test second questionnaire description',
    //         'slug'
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expectHttp(q1Response, 201);
    //     expectHttp(q2Response, 201);
    //
    //     const getResponse = await getQuestionnaire(
    //         request,
    //         q1.questionnaireId,
    //         q2Token
    //     );
    //
    //     // // --- HTTP-level checks ---
    //     expect(getResponse.questionnaireGetResponse.ok()).toBeFalsy();
    //     expect(getResponse.questionnaireGetResponse.status()).toBe(403);
    // });
});