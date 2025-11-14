import { test, expect, APIResponse } from '@playwright/test';
import {
    createQuestionnaire, deleteQuestionnaire,
    getLastInfo,
    publishQuestionnaire,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";
import {QuestionnaireDtoModel} from "../../models/api-models";
import {createQuestion} from "../../test-data-seeder/question-data";
import {GUID_REGEX, QuestionType} from "../../constants/test-data-constants";

test.describe('GET /questionnaires/{slug}/publishes/last/info', () => {
    test('200 OK with minimal DTO when a published version exists', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await updateQuestionnaire(request, questionnaire.id, { 
            displayTitle: 'Custom display title',
            slug: 'custom-slug'
        });
        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        await publishQuestionnaire(request, questionnaire.id);

        const { response: res, questionnaireInfo } = await getLastInfo(request, 'custom-slug');
        expect(res.status()).toBe(200);

        const body = (await res.json()) as QuestionnaireDtoModel;

        // Minimal DTO only
        expect(Object.keys(body).sort()).toEqual(['description', 'displayTitle', 'id', 'slug'].sort());
        // Types and values present
        expect(body.id).toMatch(
            GUID_REGEX,
        );
        expect(typeof body.displayTitle).toBe('string');
        expect(typeof body.description).toBe('string');
        expect(typeof body.slug).toBe('string');

        // Ensure no contributor emails or audit fields leak
        const forbiddenFields = ['contributors', 'contributorEmails', 'createdBy', 'updatedBy', 'createdAt', 'updatedAt', 'audit'];
        for (const f of forbiddenFields) {
            expect((body as any)[f]).toBeUndefined();
        }
    });

    test('400 Bad Request when no published version exists', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);

        const { response: res, questionnaireInfo } = await getLastInfo(request, questionnaire.slug);
        expect([400]).toContain(res.status());

        const problem = await res.json();
        // ProblemDetails shape without sensitive info
        expect(typeof problem.type === 'string' || problem.type === undefined).toBeTruthy();
        expect(typeof problem.title === 'string' || problem.title === undefined).toBeTruthy();
        expect(problem.detail === undefined || typeof problem.detail === 'string').toBeTruthy();
        // No internal exception detail keys
        const sensitive = ['stackTrace', 'exception', 'errors', 'developerMessage', 'debug'];
        for (const k of sensitive) {
            expect(problem[k]).toBeUndefined();
        }
    });

    test('400 Bad Request when questionnaire is deleted', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);
        await publishQuestionnaire(request, questionnaire.id);
        await deleteQuestionnaire(request, questionnaire.id);

        const { response: res, questionnaireInfo } = await getLastInfo(request, questionnaire.slug);
        expect([400]).toContain(res.status());

        const problem = await res.json();
        const sensitive = ['stackTrace', 'exception', 'errors', 'developerMessage', 'debug'];
        for (const k of sensitive) {
            expect(problem[k]).toBeUndefined();
        }
    });

    test('400 Bad Request for unknown slug (or 404 if convention changes)', async ({ request }) => {
        const unknownSlug = `unknown-uuid`;

        const { response: res, questionnaireInfo } = await getLastInfo(request, unknownSlug);
        expect([400, 404]).toContain(res.status());

        const problem = await res.json();
        const sensitive = ['stackTrace', 'exception', 'errors', 'developerMessage', 'debug'];
        for (const k of sensitive) {
            expect(problem[k]).toBeUndefined();
        }
    });

    test('200 OK when stored published questionnaire JSON is valid', async ({ request }) => {
        const { questionnaire } = await createQuestionnaire(request);
        
        await updateQuestionnaire(request, questionnaire.id, {
            displayTitle: `Valid JSON uuid`,
            description: 'Valid JSON description',
            slug: 'valid-json'
        })

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        await publishQuestionnaire(request, questionnaire.id);

        const { response: res, questionnaireInfo } = await getLastInfo(request, 'valid-json');
        expect(res.status()).toBe(200);

        const body = (await res.json()) as QuestionnaireDtoModel;
        expect(body.displayTitle).toContain('Valid JSON');
        expect(body.description).toBe('Valid JSON description');
        expect(body.slug).toBe('valid-json');
    });

    test('400 Bad Request when deserialization fails (generic ProblemDetails)', async ({ request }) => {
        // This test assumes there may be seeded malformed publish in certain environments.
        // If not applicable, it will be skipped when API returns non-400 statuses.
        const malformedSlug = process.env.MALFORMED_PUBLISH_SLUG;
        test.skip(!malformedSlug, 'No malformed publish slug configured');

        const { response: res, questionnaireInfo } = await getLastInfo(request, malformedSlug!);
        expect(res.status()).toBe(400);

        const problem = await res.json();
        // Generic problem with no internal exception leakage
        expect(typeof problem.title === 'string' || problem.title === undefined).toBeTruthy();
        const sensitive = ['stackTrace', 'exception', 'errors', 'developerMessage', 'debug'];
        for (const k of sensitive) {
            expect(problem[k]).toBeUndefined();
        }
    });
});