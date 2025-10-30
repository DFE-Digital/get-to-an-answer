import {test, expect} from '@playwright/test';
import {
    createQuestionnaire,
    updateQuestionnaire,
} from "../../test-data-seeder/questionnaire-data";
import {GUID_REGEX} from "../../constants/test-data-constants";
import {ClaimTypes, JwtHelper, SimpleDate} from '../../helpers/JwtHelper';
import {
    expect200HttpStatusCode,
    expectQuestionnaireSchema,
    expectQuestionnaireTypes,
    expectQuestionnaireContent,
    expectQuestionnaireIO, expectQuestionnaireInitStateTypes, expectQuestionnaireInitStateIO,
    expectQuestionnaireInitStateContent
} from '../../helpers/api-assertions-helper'

test.describe('POST Create questionnaire api request', () => {
    test('Validate POST create new questionnaire', async ({request}) => {
        const {
            questionnairePostResponse,
            questionnaire,
            payload
        } = await createQuestionnaire(request);

        // --- HTTP-level checks ---
        expect200HttpStatusCode(questionnairePostResponse, 201);

        // // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireInitStateTypes(questionnaire);

        // --- Basic content sanity ---
        expectQuestionnaireInitStateContent(questionnaire);

        // --- I/O checks ---
        expectQuestionnaireInitStateIO(questionnaire, payload, GUID_REGEX);
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
        expect200HttpStatusCode(questionnairePostResponse, 201);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireInitStateTypes(questionnaire);

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
        expect200HttpStatusCode(questionnairePostResponse, 201);

        // --- Schema-level checks ---
        expectQuestionnaireSchema(questionnaire);

        // --- Type sanity checks ---
        expectQuestionnaireInitStateTypes(questionnaire);

        // --- Basic content sanity ---
        expectQuestionnaireContent(questionnaire);

        // --- I/O checks ---
        expectQuestionnaireInitStateIO(questionnaire, payload, GUID_REGEX);
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
        expect200HttpStatusCode(q1Response, 201);
        expect200HttpStatusCode(q2Response, 201);

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
        const wrongTypeTitle: any = 12345;

        const {questionnairePostResponse} = await createQuestionnaire(
            request,
            undefined,
            wrongTypeTitle
        );

        // --- HTTP-level checks ---
        expect(questionnairePostResponse.ok()).toBeFalsy();
        expect(questionnairePostResponse.status()).toBe(400);
    });

    test('Validate POST create new questionnaire with invalid token', async ({request}) => {
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

    test('Validate access to another questionnaire not permitted', async ({request}) => {

        const q1Token = JwtHelper.ValidToken;
        const q2Token = JwtHelper.UnauthorizedToken;

        const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
            request,
            q1Token,
            'Custom test questionnaire title',
            'Custom test first questionnaire description',
            'slug'
        );

        const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
            request,
            q2Token,
            'Custom test questionnaire title',
            'Custom test second questionnaire description',
            'slug'
        );

        // --- HTTP-level checks ---
        expect200HttpStatusCode(q1Response, 201);
        expect200HttpStatusCode(q2Response, 201);

        const updatePayload = {
            ...q2,
            title: 'Updated title'
        }
        const update = await updateQuestionnaire(
            request,
            q2.id,
            updatePayload,
            q1Token
        )

        //should be forbidden
        expect(update.updatedQuestionnairePostResponse.status()).toBe(403);
    });
});