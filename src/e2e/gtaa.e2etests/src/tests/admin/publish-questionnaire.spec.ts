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

test.describe('Get to an answer publish questionnaire', () => {
    let token: string;

    test.beforeEach(async ({ request }) => {
        token = JwtHelper.NoRecordsToken();
    });

    test('Publish questionnaire successfully (happy path)', async ({ page, request }) => {
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
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // Adjust the assertion here if your API uses a different property for the publish state
        expect(body.status === EntityStatus.Published).toBeTruthy();
    });

    test('Cancel publish returns without publishing (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

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
        await confirmPublishPage.chooseNo();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // Verify via API that the questionnaire is now published
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // For the unhappy path we expect the questionnaire to remain in a non-published state
        expect(body.status === EntityStatus.Draft).toBeTruthy();
    });
    
    test('Publish questionnaire with no question returns error publishing (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await editQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // You should see an error banner at the top of the page
        await editQuestionnairePage.assertGtaaApiErrorBanner(
            'Error: Publishing the questionnaire failed.',
            'The questionnaire has no questions'
        );
        
        // Verify via API that the questionnaire is now published
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // For the unhappy path we expect the questionnaire to remain in a non-published state
        expect(body.status === EntityStatus.Draft).toBeTruthy();
    })

    test('Publish questionnaire with 1 question (but no answers) returns error publishing (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await editQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // You should see an error banner at the top of the page
        await editQuestionnairePage.assertGtaaApiErrorBanner(
            'Error: Publishing the questionnaire failed.',
            'Question 1 has no answers'
        );

        // Verify via API that the questionnaire is now published
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // For the unhappy path we expect the questionnaire to remain in a non-published state
        expect(body.status === EntityStatus.Draft).toBeTruthy();
    })

    test('Publish questionnaire with last question (but answer wrong destination type) returns error publishing (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        const { question } = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        const { question: question2 } = await createQuestion(request, questionnaire.id, token, 'second question', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.Question, destinationQuestionId: question2.id
        }, token)

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question2.id, content: 'A1',
            destinationType: undefined
        }, token)

        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await editQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // You should see an error banner at the top of the page
        await editQuestionnairePage.assertGtaaApiErrorBanner(
            'Error: Publishing the questionnaire failed.',
            "Answer 'A1' of the last question should have an external link or results page as it's destination."
        );

        // Verify via API that the questionnaire is now published
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // For the unhappy path we expect the questionnaire to remain in a non-published state
        expect(body.status === EntityStatus.Draft).toBeTruthy();
    })

    test('Publish questionnaire with cyclic flow returns error publishing (unhappy path)', async ({ page, request }) => {
        const { questionnaire } = await createQuestionnaire(request, token);

        await updateQuestionnaire(request, questionnaire.id, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaire.id, 'user-1', token)

        const { question } = await createQuestion(request, questionnaire.id, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        const { question: question2 } = await createQuestion(request, questionnaire.id, token, 'second question', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.Question, destinationQuestionId: question2.id
        }, token)

        await createSingleAnswer(request, {
            questionnaireId: questionnaire.id, questionId: question2.id, content: 'A1',
            destinationType: AnswerDestinationType.Question, destinationQuestionId: question.id
        }, token)

        await signIn(page, token);

        const questionnaireId = questionnaire.id;

        const editQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await editQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        // You should see an error banner at the top of the page
        await editQuestionnairePage.assertGtaaApiErrorBanner(
            'Error: Publishing the questionnaire failed.',
            "Question 1 was referenced twice in the same flow."
        );

        // Verify via API that the questionnaire is now published
        const refreshed = await getQuestionnaire(request, questionnaireId, token);
        const body = refreshed.questionnaireGetBody;

        // For the unhappy path we expect the questionnaire to remain in a non-published state
        expect(body.status === EntityStatus.Draft).toBeTruthy();
    })
    
    // CARE-1601 Test
    test('View history link is always accessible after the initial publish', async ({ page, request }) => {
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
        
        // Check that the view history link is a span and in the disabled state
        // and checks the status
        await editQuestionnairePage.assertViewHistoryLinkDisabled()

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
        
        // check the view history link is accessible
        await editQuestionnairePage.assertViewHistoryLinkEnabled();
        await editQuestionnairePage.openViewVersionHistory();
        await editQuestionnairePage.waitForPageLoad()

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/version-history`));
        
        // ============================================================
        // check the view history page is accessible when back to draft
        // ============================================================
        
        await page.goBack()
        await editQuestionnairePage.waitForPageLoad()
        
        await editQuestionnairePage.openEditQuestionnaireName();
        
        const editQuestionnaireNamePage = new AddQuestionnairePage(page, 'update');
        await editQuestionnaireNamePage.waitForPageLoad()

        await editQuestionnaireNamePage.addQuestionnaire('updated questionnaire title')

        refreshed = await getQuestionnaire(request, questionnaireId, token);
        body = refreshed.questionnaireGetBody;
        
        expect(body.title).toEqual('updated questionnaire title');

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/track`));

        await editQuestionnairePage.assertViewHistoryLinkEnabled();
        await editQuestionnairePage.openViewVersionHistory()
        await editQuestionnairePage.waitForPageLoad()

        // Should land back on the tracking page for this questionnaire
        await expect(page).toHaveURL(new RegExp(`/admin/questionnaires/${questionnaireId}.*/version-history`));
        
    });
});