import {APIRequestContext, APIResponse, Browser, expect, Page, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {
    goToQuestionnaireIntegrationPage,
    goToQuestionnaireStylingPageByUrl,
} from '../../helpers/admin-test-helper';
import {
    addContributor,
    createQuestionnaire,
    getQuestionnaire, publishQuestionnaire, unpublishQuestionnaire,
    updateQuestionnaire,
    updateQuestionnaireContinueButton, updateQuestionnaireStyling
} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {
    createDropdownAnswerSet,
    createMultiSelectAnswerSet,
    createSingleAnswer
} from '../../test-data-seeder/answer-data';
import {expect200HttpStatusCode} from '../../helpers/api-assertions-helper';
import {DesignQuestionnairePage} from '../../pages/admin/DesignQuestionnairePage';
import {QuestionnaireStylingPage} from '../../pages/admin/QuestionnaireStylingPage';
import {AnswerDestinationType, QuestionType} from '../../constants/test-data-constants';
import {createContent} from "../../test-data-seeder/content-data";
import {QuestionnaireStartPage} from "../../pages/fe/QuestionnaireStartPage";
import {QuestionnaireNextPage} from "../../pages/fe/QuestionnaireNextPage";
import {
    goToPublishedQuestionnaireNextPageByUrl,
    goToPublishedQuestionnairePageByUrl, goToPublishedQuestionnairePageByUrlNoWait,
    goToPublishedQuestionnairePageForResponse, waitForRedirectTo
} from "../../helpers/fe-test-helper";
import {EnvConfig} from "../../config/environment-config";
import {SignInPage} from "../../pages/admin/SignInPage";
import {PublishQuestionnaireConfirmationPage} from "../../pages/admin/PublishQuestionnaireConfirmationPage";
import {ErrorPage} from "../../pages/fe/ErrorPage";

test.describe('Questionnaire run (start & next)', () => {
    // Limit parallelism to avoid race conditions
    /*test.describe.configure({
        mode: 'serial',
    } as any);*/
    
    let token: string;
    let questionnaireId: string;
    let questionnaireSlug: string;

    let startPage: QuestionnaireStartPage;
    let nextPage: QuestionnaireNextPage;
    let stylingPage: QuestionnaireStylingPage;

    test.beforeEach(async ({ request }) => {
        token = JwtHelper.NoRecordsToken();

        const { questionnaire, questionnairePostResponse } = await createQuestionnaire(request, token);
        expect200HttpStatusCode(questionnairePostResponse, 201);
        
        questionnaireSlug = `questionnaire-run-preview-${Math.floor(Math.random() * 10000000)}`

        const { updatedQuestionnairePostResponse } = await updateQuestionnaire(request, questionnaire.id, { slug: questionnaireSlug }, token)
        expect200HttpStatusCode(updatedQuestionnairePostResponse, 204);

        questionnaireId = questionnaire.id;
    });

    test('Start without decorative image (non-embedded) shows header H1 and Start now button', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My preview title",
            description: "Some description for preview"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();
        await startPage.expectErrorSummaryIfPresent();
        await startPage.clickStartNow();

        nextPage = await QuestionnaireNextPage.create(page);
        const isQuestionMode = await nextPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    // TODO: add test to confirm preview start page renders the markdown correctly

    test('Start preview with no start page', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);
        
        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        
        nextPage = await QuestionnaireNextPage.create(page);

        const isQuestionMode = await nextPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    test('Start preview with decorative image shows hero image and inner fieldset heading', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await signIntoAdmin(page, token);
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId, EnvConfig.ADMIN_URL);

        await stylingPage.uploadDecorativeImage('../../assets/sample-start-page-image.jpg');
        await stylingPage.acceptAccessibilityAgreement();
        await stylingPage.saveAndContinue();

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "Decorative start",
            description: "Description with image"
        }, token)

        const designQuestionnairePage = new DesignQuestionnairePage(page)
        await designQuestionnairePage.publishQuestionnaire()

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();

        const startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        await startPage.assertStructure();
        await expect(startPage.outerImage, '❌ Decorative image not shown on start preview').toBeVisible();
    });

    // doesn't work locally (unless you set the ASP env to Developer, etc.), 
    // only in Azure environment because we set the developer debugging page to display locally
    test.skip('404 Error Page for non-existing questionnaire', async ({ page }) => {
        await goToPublishedQuestionnairePageByUrlNoWait(page, "fake-questionnaire-slug");

        const errorPage = await ErrorPage.create(page);

        await errorPage.assertErrorText(
            'Page not found.', `
            If you typed the web address, check it is correct.
            If you pasted the web address, check you copied the entire address.
        `)
    });

    test('Embedded questionnaire uses inner fieldset heading even without decorative image', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "Decorative start",
            description: "Description with image"
        }, token)
        
        await publish(request);
        
        const startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        await startPage.assertStructure();
        await expect(startPage.innerFieldset, '❌ Inner fieldset should be hidden').toBeHidden();
    });

    test('Question questionnaire – single-select question with valid selection continues to next state', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)
        
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);
        
        const { content, response } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)
        expect200HttpStatusCode(response, 201);

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Single answer option',
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);
        
        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        
        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertSingleSelectQuestion();

        await nextPage.selectFirstRadioOption();
        await nextPage.clickContinue();

        // Results page is rendered properly
        await nextPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    });

    test('Questionnaire– multi-select question allows multiple choices and continues', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        await createMultiSelectAnswerSet(request, questionnaireId, question.id, content.id, token);
        
        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertMultiSelectQuestion();

        await nextPage.selectFirstCheckboxOption();
        await nextPage.clickContinue();
    });

    test('Questionnaire – dropdown question requires a non-empty selection', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.DropdownSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        await createDropdownAnswerSet(request, questionnaireId, question.id, content.id, token);

        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertDropdownQuestion();

        await nextPage.clickContinue();
        await nextPage.expectErrorSummary();

        await nextPage.selectDropdownByIndex(1);
        await nextPage.clickContinue();
    });

    test('Custom content destination is rendered correctly in questionnaire run', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);
        
        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to custom content',
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);
        
        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertSingleSelectQuestion();

        await nextPage.selectFirstRadioOption();
        await nextPage.clickContinue();

        const isCustomContent = await nextPage.isResultsPageMode();
        expect(isCustomContent).toBeTruthy();

        await nextPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    });

    test('External-link destination in embedded questionnaire – hidden field is rendered instead of redirect', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const externalUrl = 'https://www.gov.uk/';
        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to GOV.UK',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);
        
        await publish(request);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertSingleSelectQuestion(true);

        await nextPage.selectFirstRadioOption();
        await nextPage.clickContinue();

        expect(page.url()).toBe(externalUrl);
    });

    test('questionnaire uses current custom styling (primary colour and button label)', async ({ request, page }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to custom content',
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        await updateQuestionnaire(request, questionnaireId, { 
            displayTitle: `displayTitle`,
            description: `description`,
        }, token);

        const questionnaireResponse = await getQuestionnaire(request, questionnaireId, token);
        let { 
            textColor, 
            backgroundColor, 
            primaryButtonColor,
            secondaryButtonColor,
            stateColor,
            errorMessageColor
        } = questionnaireResponse.questionnaireGetBody;

        /*
        TextColor = "#0b0c0c";
        BackgroundColor = "#ffffff";
        PrimaryButtonColor = "#00703c";
        SecondaryButtonColor = "#1d70b8";
        StateColor = "#ffdd00";
        ErrorMessageColor = "#c3432b";
         */

        const newPrimaryButtonColor = '#005ea5';
        
        await signIntoAdmin(page, token);
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId, EnvConfig.ADMIN_URL);
        await stylingPage.setPrimaryButtonColor(newPrimaryButtonColor);
        await stylingPage.acceptAccessibilityAgreement()
        await stylingPage.saveAndContinue();
        
        const response = await updateQuestionnaireContinueButton(request, questionnaireId, { continueButtonText: `Continue questionnaire` }, token);
        expect200HttpStatusCode(response, 204);

        await publish(request);
        
        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();

        // assert that the start page button text remains the same
        await startPage.assertStartButtonTextAndColor("Start now", newPrimaryButtonColor);

        await startPage.clickStartNow();
        
        const newContinueButtonText = "Continue questionnaire"
        
        nextPage = await QuestionnaireNextPage.create(page);
        
        // to trigger the error state
        await nextPage.clickContinue();

        await nextPage.assertContinueButtonTextAndColor(newContinueButtonText, newPrimaryButtonColor);
        await nextPage.assertTextColor(textColor);
        await nextPage.assertErrorComponentsColor(errorMessageColor);
        
        const newTextColor = '#ff0000';
        const newBackgroundColor = '#222222';
        const newErrorMessageColor = '#00ff00';
        const newStateColor = '#00ffff';
        
        await updateQuestionnaireStyling(request, questionnaireId, { 
            textColor: newTextColor, 
            backgroundColor: newBackgroundColor, 
            primaryButtonColor: newPrimaryButtonColor, 
            secondaryButtonColor, // unused
            stateColor: newStateColor, 
            errorMessageColor: newErrorMessageColor
        }, token);

        await publish(request);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();
        await startPage.assertTextColor(newTextColor);
        await startPage.assertBackgroundColor(newBackgroundColor);
        await startPage.assertStartButtonTextAndColor(
            "Start now", 
            newPrimaryButtonColor,
            newStateColor);
        await startPage.clickStartNow();
        
        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.clickContinue();
        await nextPage.assertTextColor(newTextColor);
        await nextPage.assertContinueButtonTextAndColor(
            newContinueButtonText,
            newPrimaryButtonColor,
            newStateColor);
        await nextPage.assertBackgroundColor(newBackgroundColor);
        await nextPage.assertErrorComponentsColor(newErrorMessageColor);

    });

    test('Multi select question with multiple answers selected is prioritised correctly', async ({ request, page }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { question: question2, questionPostResponse: question2PostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(question2PostResponse, 201);

        await createSingleAnswer(
            request,
            {
                questionId: question2.id,
                questionnaireId,
                content: 'Single option 1',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: 'https://www.gov.uk/'
            },
            token
        );

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const payloads = [
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 1',
                priority: 0, // lowest priority
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: 'https://www.gov.uk/'
            },
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 2',
                priority: 1, // highest priority
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id
            },
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 3',
                priority: 2, // second-highest priority
                destinationType: AnswerDestinationType.Question,
                destinationQuestionId: question2.id
            },
        ];

        for (const payload of payloads) {
            await createSingleAnswer(
                request,
                payload,
                token
            );
        }
        
        await publish(request);
        
        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        
        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertMultiSelectQuestion();
        
        await nextPage.selectAllCheckboxOptions();
        
        await nextPage.clickContinue();
        
        await nextPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    })

    test('Multi select question with multiple answers selected (excluding highest priority) is prioritised correctly', async ({ page, request }) => {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { question: question2, questionPostResponse: question2PostResponse } = await createQuestion(request, questionnaireId, token, "Q2", QuestionType.SingleSelect, undefined);
        expect200HttpStatusCode(question2PostResponse, 201);
        
        await createSingleAnswer(
            request,
            {
                questionId: question2.id,
                questionnaireId,
                content: 'Single option 1',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: 'https://www.gov.uk/'
            },
            token
        );

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const payloads = [
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 1',
                priority: undefined, // lowest priority
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: 'https://www.gov.uk/'
            },
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 3',
                priority: 2, // second-highest priority
                destinationType: AnswerDestinationType.Question,
                destinationQuestionId: question2.id
            },
            {
                questionId: question.id,
                questionnaireId,
                content: 'Multi option 2',
                priority: 1, // highest priority
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id
            },
        ];

        for (const payload of payloads) {
            await createSingleAnswer(
                request,
                payload,
                token
            );
        }
        
        await publish(request);

        await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);

        nextPage = await QuestionnaireNextPage.create(page);
        await nextPage.assertMultiSelectQuestion();

        await nextPage.selectAllCheckboxOptions();
        
        await nextPage.unselectLastCheckboxOption();

        await nextPage.clickContinue();

        await nextPage.assertSingleSelectQuestion();
    })
    
    test('Unpublished questionnaires return 404 not found', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();
        
        const { response: unpublishResponse } = await unpublishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(unpublishResponse, 204);

        const response = await goToPublishedQuestionnairePageForResponse(page, questionnaireSlug);
        expect(response).not.toBeNull();
        expect(response?.status()).toEqual(404);
    });

    test('Unpublished questionnaires still return 404 not found, if edited', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();

        const { response: unpublishResponse } = await unpublishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(unpublishResponse, 204);

        const response = await goToPublishedQuestionnairePageForResponse(page, questionnaireSlug);
        expect(response).toBeTruthy();
        expect(response.status()).toEqual(404);
        
        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My edited questionnaire title",
            description: "Some edited description for questionnaire"
        }, token)
        
        const response2 = await goToPublishedQuestionnairePageForResponse(page, questionnaireSlug);
        expect(response2).not.toBeNull();
        expect(response2?.status()).toEqual(404);
    });
    
    test('Republished questionnaire is loaded', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();

        const { response: unpublishResponse } = await unpublishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(unpublishResponse, 204);

        const response = await goToPublishedQuestionnairePageForResponse(page, questionnaireSlug);
        expect(response).not.toBeNull();
        expect(response?.status()).toEqual(404);

        const { response: republishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(republishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();
    });

    test('Republished questionnaire is loaded, if edited', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();

        const { response: unpublishResponse } = await unpublishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(unpublishResponse, 204);

        const response = await goToPublishedQuestionnairePageForResponse(page, questionnaireSlug);
        expect(response).not.toBeNull();
        expect(response?.status()).toEqual(404);

        const { response: republishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(republishResponse, 204);

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My edited questionnaire title",
            description: "Some edited description for questionnaire"
        }, token)

        startPage = await goToPublishedQuestionnairePageByUrl(page, questionnaireSlug);
        await startPage.assertStructure();
    });

    test('Questionnaire in frame works properly', async ({ request, page, context, browser }) => {
        await context.grantPermissions(['clipboard-read', 'clipboard-write']);

        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);
        
        const { page2, frame } = await loadQuestionnaireIframe(page, browser, questionnaireId);

        startPage = await QuestionnaireStartPage.create(page2, frame);
        await startPage.assertStructure();
        await startPage.expectErrorSummaryIfPresent();
        await startPage.clickStartNow();

        nextPage = await QuestionnaireNextPage.create(page2, frame);
        const isQuestionMode = await nextPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    test('Questionnaire in frame works properly with decorative image', async ({ request, page, context, browser }) => {
        await context.grantPermissions(['clipboard-read', 'clipboard-write']);

        await populateQuestionnaire(request, questionnaireId, token);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: "My questionnaire title",
            description: "Some description for questionnaire"
        }, token)

        await signIntoAdmin(page, token);
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId, EnvConfig.ADMIN_URL);
        await stylingPage.uploadDecorativeImage('../../assets/sample-start-page-image.jpg');
        await stylingPage.acceptAccessibilityAgreement()
        await stylingPage.saveAndContinue();
        
        await publishQuestionnaireViaAdmin(page);

        const { page2, frame } = await loadQuestionnaireIframe(page, browser, questionnaireId);

        startPage = await QuestionnaireStartPage.create(page2, frame);
        await startPage.assertStructure();
        await expect(startPage.outerImage, '❌ Decorative image not shown on questionnaire start').toBeVisible();
        await startPage.expectErrorSummaryIfPresent();
        await startPage.clickStartNow();

        nextPage = await QuestionnaireNextPage.create(page2, frame);
        const isQuestionMode = await nextPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    test('Questionnaire in iframe uses current custom styling (primary colour and button label)', async ({ request, page, context, browser }) => {
        await context.grantPermissions(['clipboard-read', 'clipboard-write']);

        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to custom content',
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        await updateQuestionnaire(request, questionnaireId, {
            displayTitle: `displayTitle`,
            description: `description`,
        }, token);

        const questionnaireResponse = await getQuestionnaire(request, questionnaireId, token);
        const {
            textColor,
            backgroundColor,
            primaryButtonColor,
            secondaryButtonColor,
            stateColor,
            errorMessageColor
        } = questionnaireResponse.questionnaireGetBody;

        const newPrimaryButtonColor = '#005ea5';

        await signIntoAdmin(page, token);
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId, EnvConfig.ADMIN_URL);
        await stylingPage.setPrimaryButtonColor(newPrimaryButtonColor);
        await stylingPage.acceptAccessibilityAgreement()
        await stylingPage.saveAndContinue();

        const response = await updateQuestionnaireContinueButton(request, questionnaireId, { continueButtonText: `Continue questionnaire` }, token);
        expect200HttpStatusCode(response, 204);

        await publish(request);

        const { page2, frame } = await loadQuestionnaireIframe(page, browser, questionnaireId);

        startPage = await QuestionnaireStartPage.create(page2, frame);
        await startPage.assertStructure();
        
        // assert that the start page button text remains the same
        await startPage.assertStartButtonTextAndColor("Start now", newPrimaryButtonColor);
        await startPage.clickStartNow();

        const newContinueButtonText = "Continue questionnaire"

        nextPage = await QuestionnaireNextPage.create(page2, frame);

        // to trigger the error state
        await nextPage.clickContinue();
        
        await nextPage.waitForErrorStatePresent();

        await nextPage.assertContinueButtonTextAndColor(newContinueButtonText, newPrimaryButtonColor);
        await nextPage.assertTextColor(textColor);
        await nextPage.assertErrorComponentsColor(errorMessageColor);

        const newTextColor = '#ff0000';
        const newBackgroundColor = '#222222';
        const newErrorMessageColor = '#00ff00';
        const newStateColor = '#00ffff';

        await updateQuestionnaireStyling(request, questionnaireId, {
            textColor: newTextColor,
            backgroundColor: newBackgroundColor,
            primaryButtonColor: newPrimaryButtonColor,
            secondaryButtonColor, // unused
            stateColor: newStateColor,
            errorMessageColor: newErrorMessageColor
        }, token);
        
        const { questionnaireGetBody: questionnaire } = 
            await getQuestionnaire(request, questionnaireId, token);
        await expect(questionnaire.decorativeImage).toBeNull();

        await publish(request);

        const iframeElement = page2.locator('#gtaaFrame');
        await iframeElement.evaluate((node: HTMLIFrameElement) => {
            node.src = `${node.src}`;
        });

        startPage = await QuestionnaireStartPage.create(page2, frame);
        await startPage.assertStructure();
        await startPage.assertTextColor(newTextColor);
        await startPage.assertBackgroundColor(newBackgroundColor);
        await startPage.assertStartButtonTextAndColor(
            "Start now",
            newPrimaryButtonColor,
            newStateColor);
        await startPage.clickStartNow();

        nextPage = await QuestionnaireNextPage.create(page2, frame);
        await nextPage.clickContinue();
        
        await nextPage.waitForErrorStatePresent();
        
        await nextPage.assertTextColor(newTextColor);
        await nextPage.assertContinueButtonTextAndColor(
            newContinueButtonText,
            newPrimaryButtonColor,
            newStateColor);
        await nextPage.assertBackgroundColor(newBackgroundColor);
        await nextPage.assertErrorComponentsColor(newErrorMessageColor);
    });

    test('Custom content destination is rendered correctly in questionnaire iframe run', async ({ page, request, context, browser }) => {
        await context.grantPermissions(['clipboard-read', 'clipboard-write']);

        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom questionnaire content',
            content: 'Custom questionnaire content **markdown**',
            referenceName: 'custom-questionnaire-content',
        }, token)

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to custom content',
                destinationType: AnswerDestinationType.CustomContent,
                destinationContentId: content.id,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        await publish(request);

        const { page2, frame } = await loadQuestionnaireIframe(page, browser, questionnaireId);

        nextPage = await QuestionnaireNextPage.create(page2, frame);
        await nextPage.assertSingleSelectQuestion();

        await nextPage.selectFirstRadioOption();
        await nextPage.clickContinue();
        
        await nextPage.waitForResultsPageLoad();

        const isCustomContent = await nextPage.isResultsPageMode();
        expect(isCustomContent).toBeTruthy();

        await nextPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    });

    test('External-link destination in embedded questionnaire iframe – hidden field is rendered instead of redirect', async ({ page, request, context, browser }) => {
        await context.grantPermissions(['clipboard-read', 'clipboard-write']);

        await addContributor(request, questionnaireId, 'user-1', token)

        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const externalUrl = 'https://www.gov.uk/';
        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Go to GOV.UK',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        await publish(request);

        const { page2, frame } = await loadQuestionnaireIframe(page, browser, questionnaireId);

        nextPage = await QuestionnaireNextPage.create(page2, frame);
        await nextPage.assertSingleSelectQuestion(true);

        await nextPage.selectFirstRadioOption();
        await nextPage.clickContinue();
        
        // wait for page to redirect to 'externalUrl'
        await waitForRedirectTo(page2, externalUrl);
        
        const url = page2.url();

        expect(url).toBe(externalUrl);
    });
    
    async function populateQuestionnaire(request: any, questionnaireId: string, token: string) {
        await addContributor(request, questionnaireId, 'user-1', token)

        const { question } = await createQuestion(request, questionnaireId, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaireId, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
    }
    
    async function signIntoAdmin(page: Page, token: string) {
        await page.goto(`${EnvConfig.ADMIN_URL}/dev/login?jt=${token}`);

        const signInPage = new SignInPage(page);
        await signInPage.acceptCookiesIfVisible();
    }
    
    async function publish(request: APIRequestContext) {
        const { response: publishResponse } = await publishQuestionnaire(request, questionnaireId, token);
        expect200HttpStatusCode(publishResponse, 204);
    }
    
    async function loadQuestionnaireIframe(page: Page, browser: Browser, questionnaireId: string) {
        await signIntoAdmin(page, token);
        const integrationPage = await goToQuestionnaireIntegrationPage(page, questionnaireId, EnvConfig.ADMIN_URL);
        const iframeCode = await integrationPage.copyIframeCode();

        // 1. Define your custom HTML with an iframe
        // Note: You can point the src to a real URL or a local file
        const customHtml = `
          <!DOCTYPE html>
          <html lang="en">
            <head><title>Mock DfE Service</title></head>
            <body>
              <h1>Home Page</h1>
              ${iframeCode}
              <script src="${EnvConfig.FE_URL}/js/gtaa.external.js"
            </body>
          </html>
        `;

        // new browser context and page
        const context2 = await browser.newContext();
        const page2 = await context2.newPage();
        await page2.goto(EnvConfig.FE_URL!, {waitUntil: "networkidle"})
        // Our domain needs to be whitelisted in CSP otherwise the iframe will fail to load
        // 2. Set the content of the current page
        await page2.setContent(customHtml);

        // 3. Locate the iframe using a selector
        const frame = page2.frameLocator('#gtaaFrame');
        
        return { page2, frame };
    }
    
    async function publishQuestionnaireViaAdmin(page: Page) {
        await page.goto(`${EnvConfig.ADMIN_URL}/admin/questionnaires/${questionnaireId}/track`);

        const designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.publishQuestionnaire();

        const confirmPublishPage = new PublishQuestionnaireConfirmationPage(page);
        await confirmPublishPage.expectTwoRadiosPresent();
        await confirmPublishPage.chooseYes();
        await confirmPublishPage.clickContinue();
    }
});