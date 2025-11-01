import {expect, test} from '@playwright/test';
import {
    createQuestionnaire, deleteQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire,
    updateQuestionnaire,
} from "../../test-data-seeder/questionnaire-data";
import {EntityStatus, QuestionType} from "../../constants/test-data-constants";
import {JwtHelper} from '../../helpers/JwtHelper';
import {expect200HttpStatusCode} from '../../helpers/api-assertions-helper'
import {createQuestion} from "../../test-data-seeder/question-data";

test.describe('PUT Publish questionnaire api request', () => {
    test('Validate PUT publish questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);
        
        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        const { response } = await publishQuestionnaire(request, questionnaire.id);
        
        expect(response.status()).toBe(204);
        
        const { questionnaireGetBody: publishedQuestionnaire } = await getQuestionnaire(request, questionnaire.id);
        
        expect(publishedQuestionnaire.status).toBe(EntityStatus.Published);
    });

    test('Validate PUT republish published questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        await publishQuestionnaire(request, questionnaire.id);
        
        // Republish
        const { response } = await publishQuestionnaire(request, questionnaire.id);

        expect(response.status()).toBe(400);
    });

    test('Validate PUT invalid questionnaire id format', async ({request}) => {
        const { response } = await publishQuestionnaire(request, 'some-guid');

        // no questions in questionnaire
        expect(response.status()).toBe(404);
    });
    
    test('Validate PUT publish deleted questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        await deleteQuestionnaire(request, questionnaire.id);
        
        const { response } = await publishQuestionnaire(request, 'some-guid');

        // no questions in questionnaire
        expect(response.status()).toBe(404);
    });

    test('Validate POST publish unauthorised questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        const { response } = await publishQuestionnaire(request, questionnaire.id, JwtHelper.UnauthorizedToken);

        // no questions in questionnaire
        expect(response.status()).toBe(403);
    });

    test('Validate POST publish unauthorised questionnaire with expired token', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MULTIPLE, undefined);

        const { response } = await publishQuestionnaire(request, questionnaire.id, JwtHelper.ExpiredToken);

        // no questions in questionnaire
        expect(response.status()).toBe(401);
    });
});