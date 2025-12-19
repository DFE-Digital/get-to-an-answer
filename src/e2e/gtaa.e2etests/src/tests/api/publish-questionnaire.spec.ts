import {expect, test} from '@playwright/test';
import {
    addContributor,
    createQuestionnaire, deleteQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire, unpublishQuestionnaire,
    updateQuestionnaire,
} from "../../test-data-seeder/questionnaire-data";
import {AnswerDestinationType, EntityStatus, QuestionType} from "../../constants/test-data-constants";
import {JwtHelper} from '../../helpers/JwtHelper';
import {expect200HttpStatusCode} from '../../helpers/api-assertions-helper'
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

test.describe('PUT Publish questionnaire api request', () => {
    test('Validate PUT publish questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })
        
        const { response } = await publishQuestionnaire(request, questionnaire.id);
        
        expect(response.status()).toBe(204);
        
        const { questionnaireGetBody: publishedQuestionnaire } = await getQuestionnaire(request, questionnaire.id);
        
        expect(publishedQuestionnaire.status).toBe(EntityStatus.Published);
    });

    test('Validate PUT republish published questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaire.id);
        expect(publishResponse.status()).toBe(204);
        
        // Republish
        const { response } = await publishQuestionnaire(request, questionnaire.id);

        expect(response.status()).toBe(400);
    });

    test('Validate PUT republish unpublished questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })
        
        const { response: publishResponse } = await publishQuestionnaire(request, questionnaire.id);
        expect(publishResponse.status()).toBe(204);

        // Unpublish
        const { response: response2 } = await unpublishQuestionnaire(request, questionnaire.id)
        expect(response2.status()).toBe(204);

        // Republish
        const { response } = await publishQuestionnaire(request, questionnaire.id);
        expect(response.status()).toBe(204);
        
        const { questionnaireGetBody } = await getQuestionnaire(request, questionnaire.id)
        expect(questionnaireGetBody.status).toBe(EntityStatus.Published);
        expect(questionnaireGetBody.isUnpublished).toBeFalsy();
    });

    test('Validate PUT invalid questionnaire id format', async ({request}) => {
        const { response } = await publishQuestionnaire(request, 'some-guid');

        // no questions in questionnaire
        expect(response.status()).toBe(404);
    });
    
    test('Validate PUT publish deleted questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await deleteQuestionnaire(request, questionnaire.id);
        
        const { response } = await publishQuestionnaire(request, 'some-guid');

        // no questions in questionnaire
        expect(response.status()).toBe(404);
    });

    test('Validate POST publish unauthorised questionnaire', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await addContributor(request, questionnaire.id, 'user-1')

        const { question } = await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);
        
        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        })
        
        const { response } = await publishQuestionnaire(request, questionnaire.id, JwtHelper.UnauthorizedToken);

        // no questions in questionnaire
        expect(response.status()).toBe(403);
    });

    test('Validate POST publish unauthorised questionnaire with expired token', async ({request}) => {
        const { questionnaire } = await createQuestionnaire(request);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` });

        await createQuestion(request, questionnaire.id, undefined, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        const { response } = await publishQuestionnaire(request, questionnaire.id, JwtHelper.ExpiredToken);

        // no questions in questionnaire
        expect(response.status()).toBe(401);
    });
});