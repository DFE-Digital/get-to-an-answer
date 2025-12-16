import { test, expect } from '@playwright/test';
import {
    addContributor,
    createQuestionnaire,
    getQuestionnaire,
    publishQuestionnaire, updateQuestionnaire,
} from '../../test-data-seeder/questionnaire-data';

import { signIn, goToDesignQuestionnairePageByUrl } from '../../helpers/admin-test-helper';
import { PublishQuestionnaireConfirmationPage } from '../../pages/admin/PublishQuestionnaireConfirmationPage';
import { JwtHelper } from '../../helpers/JwtHelper';
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType, EntityStatus, QuestionType} from "../../constants/test-data-constants";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {QuestionnaireNextPage} from "../../pages/fe/QuestionnaireNextPage";
import {AddQuestionnaireStartPage} from "../../pages/admin/AddQuestionnaireStartPage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {UnpublishQuestionnaireConfirmationPage} from "../../pages/admin/UnpublishQuestionnaireConfirmationPage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";

test.describe('Get to an answer unpublish questionnaire', () => {
    let token: string;

    test.beforeEach(async ({ request }) => {
        token = JwtHelper.NoRecordsToken();
    });

    test('Unpublish questionnaire successfully (happy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);
        
        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        const { question } = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
        
        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        // Go to Edit a Questionnaire page and trigger publish flow
        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        // This mirrors the existing pattern used for delete confirmation flows
        await editQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // Verify via API that the questionnaire is now published
        let refreshed = await getQuestionnaire(request, questionnaireId, token);
        let body = refreshed.questionnaireGetBody;

        // Adjust the assertion here if your API uses a different property for the publish state
        expect(body.status === EntityStatus.Published).toBeTruthy();
        
        // Now unpublish the questionnaire
        await editQuestionnairePage.unpublishQuestionnaire();

        const confirmUnpublishPage = new UnpublishQuestionnaireConfirmationPage(page);
        await confirmUnpublishPage.expectTwoRadiosPresent();
        await confirmUnpublishPage.chooseYes();
        await confirmUnpublishPage.clickContinue();

        // Verify via API that the questionnaire is now unpublished
        refreshed = await getQuestionnaire(request, questionnaireId, token);
        body = refreshed.questionnaireGetBody;

        // Adjust the assertion here if your API uses a different property for the unpublish state
        expect(body.status === EntityStatus.Private).toBeTruthy();
        
        // Display as draft, straight after unpublishing
        const designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.assertQuestionnaireStatus('Draft');
    });
    
    test('Unpublish questionnaire not possible if not published (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        const { question } = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        // Go to Edit a Questionnaire page and trigger publish flow
        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        // Because questionnaire hasn't been published, the unpublish button is not visible
        await editQuestionnairePage.assertUnpublishQuestionnaireButtonNotVisible();
    })
});