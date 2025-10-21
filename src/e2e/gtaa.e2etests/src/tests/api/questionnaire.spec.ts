import {test, expect} from '@playwright/test';
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {GUID_REGEX} from "../../constants/test-data-constants";
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
            'Custom test questionnaire title',
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

    // test('Validate POST create new questionnaire with Title and Description only', async ({request}) => {
    //     const nullSlug: any = null;
    //
    //     const {questionnairePostResponse, questionnaire, payload} = await createQuestionnaire(
    //         request,
    //         undefined,
    //         'TitleAndDescriptionOnly',
    //         'Custom test questionnaire title',
    //         'Custom test questionnaire description',
    //         nullSlug
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expectHttp(questionnairePostResponse, 201);
    //
    //     // --- Schema-level checks ---
    //     expectQuestionnaireSchema(questionnaire);
    //
    //     // --- Type sanity checks ---
    //     expectQuestionnaireTypes(questionnaire);
    //
    //     // --- Basic content sanity ---
    //     expectQuestionnaireContent(questionnaire);
    //
    //     // --- I/O checks ---
    //     expectQuestionnaireIO(questionnaire, payload, GUID_REGEX);
    // });
    //
    // test('Validate POST create new questionnaire with matching Title to another questionnaire', async ({request}) => {
    //
    //     const {questionnairePostResponse: q1Response, questionnaire: q1} = await createQuestionnaire(
    //         request,
    //         undefined,
    //         undefined,
    //         'Custom test questionnaire title',
    //         'Custom test first questionnaire description',
    //         undefined
    //     );
    //
    //     const {questionnairePostResponse: q2Response, questionnaire: q2} = await createQuestionnaire(
    //         request,
    //         undefined,
    //         undefined,
    //         'Custom test questionnaire title',
    //         'Custom test second questionnaire description',
    //         undefined
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expectHttp(q1Response, 201);
    //     expectHttp(q2Response, 201);
    //
    //     // --- Content sanity ---
    //     expect(q1.title).toBe(q2.title);
    //     expect(q1.id).not.toBe(q2.id)
    // });
    //
    // test('Validate POST create new questionnaire with missing Title', async ({request}) => {
    //     const nullTitle: any = null;
    //
    //     const {questionnairePostResponse} = await createQuestionnaire(
    //         request,
    //         undefined,
    //         nullTitle,
    //         'Custom test questionnaire description',
    //         undefined
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(questionnairePostResponse.ok()).toBeFalsy();
    //     expect(questionnairePostResponse.status()).toBe(400);
    // });
    // test('Validate POST create new questionnaire with invalid payload', async ({request}) => {
    //     const wrongTypeDescription: any = 12345;
    //
    //     const {questionnairePostResponse} = await createQuestionnaire(
    //         request,
    //         'Invalid payload',
    //         'Custom test questionnaire title',
    //         wrongTypeDescription,
    //         undefined
    //     );
    //
    //     // --- HTTP-level checks ---
    //     expect(questionnairePostResponse.ok()).toBeFalsy();
    //     expect(questionnairePostResponse.status()).toBe(400);
    // });
});

// test.describe('GET questionnaire api tests', () => {
//     test('Validate GET questionnaire', async ({request}) => {
//         const {questionnaire, payload} = await createQuestionnaire(request);
//         const questionnaireId = await questionnaire.id;
//
//         const questionnaireGetResponse = await getQuestionnaire(request, questionnaireId);
//
//         //Schema checks
//         expect(questionnaireGetResponse.id).toMatch(GUID_REGEX);
//         expect(questionnaireGetResponse).toHaveProperty('title');
//         expect(questionnaireGetResponse).toHaveProperty('description');
//         expect(questionnaireGetResponse).toHaveProperty('slug');
//
//         //Type sanity checks
//         expect(typeof questionnaireGetResponse.title).toBe('string');
//         expect(typeof questionnaireGetResponse.description).toBe('string');
//         expect(typeof questionnaireGetResponse.slug).toBe('string');
//
//         //Content sanity
//         expect(questionnaireGetResponse.title.trim().length).toBeGreaterThan(0);
//         expect(questionnaireGetResponse.description.trim().length).toBeGreaterThan(0);
//         expect(questionnaireGetResponse.slug.trim().length).toBeGreaterThan(0);
//     });
//});