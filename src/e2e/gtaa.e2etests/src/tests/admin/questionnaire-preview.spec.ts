import {APIResponse, expect, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {
    goToDesignQuestionnairePageByUrl,
    goToQuestionnaireContributorsPageByUrl,
    goToQuestionnaireNextPreviewByUrl,
    goToQuestionnaireStartPreviewByUrl,
    goToQuestionnaireStylingPageByUrl,
    goToViewResultsPagesPageByUrl,
    signIn,
} from '../../helpers/admin-test-helper';
import {
    addContributor,
    createQuestionnaire,
    getQuestionnaire,
    updateQuestionnaire,
    updateQuestionnaireContinueButton
} from '../../test-data-seeder/questionnaire-data';
import {createQuestion} from '../../test-data-seeder/question-data';
import {
    createDropdownAnswerSet,
    createMultiSelectAnswerSet,
    createSingleAnswer
} from '../../test-data-seeder/answer-data';
import {expect200HttpStatusCode} from '../../helpers/api-assertions-helper';
import {QuestionnaireStartPreviewPage} from '../../pages/admin/QuestionnaireStartPreviewPage';
import {QuestionnaireNextPreviewPage} from '../../pages/admin/QuestionnaireNextPreviewPage';
import {DesignQuestionnairePage} from '../../pages/admin/DesignQuestionnairePage';
import {AddQuestionnaireStartPage} from '../../pages/admin/AddQuestionnaireStartPage';
import {QuestionnaireStylingPage} from '../../pages/admin/QuestionnaireStylingPage';
import {ViewResultsPagesPage} from '../../pages/admin/ViewResultsPagesPage';
import {ViewContributorPage} from '../../pages/admin/ViewContributorPage';
import {AddContributorPage} from '../../pages/admin/AddContributorPage';
import {AnswerDestinationType, QuestionType} from '../../constants/test-data-constants';
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Questionnaire preview run (start & next)', () => {
    let token: string;
    let questionnaireId: string;

    let designQuestionnairePage: DesignQuestionnairePage;
    let startPreviewPage: QuestionnaireStartPreviewPage;
    let nextPreviewPage: QuestionnaireNextPreviewPage;
    let stylingPage: QuestionnaireStylingPage;
    let resultsPagesPage: ViewResultsPagesPage;
    let contributorsPage: ViewContributorPage;
    let addContributorPage: AddContributorPage;

    test.beforeEach(async ({ page, request }) => {
        token = JwtHelper.NoRecordsToken();

        const { questionnaire, questionnairePostResponse } = await createQuestionnaire(request, token);
        expect200HttpStatusCode(questionnairePostResponse, 201);

        questionnaireId = questionnaire.id;

        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.waitForPageLoad();
    });

    test('Start preview without decorative image (non-embedded) shows header H1 and Start now button', async ({ request, page }) => {
        await populateQuestionnaire(request, questionnaireId, token);
        
        await designQuestionnairePage.openAddStartPage();
        const startPageEditor = await AddQuestionnaireStartPage.create(page);
        await startPageEditor.configureBasicStartPage('My preview title', 'Some description for preview');
        await startPageEditor.clickSaveAndContinue();

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        startPreviewPage = await QuestionnaireStartPreviewPage.create(newPage);
        await startPreviewPage.assertStructure();
        await startPreviewPage.expectErrorSummaryIfPresent();

        await startPreviewPage.clickStartNow();

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        const isQuestionMode = await nextPreviewPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    // TODO: add test to confirm preview start page renders the markdown correctly

    test('Start preview with no start page', async ({ request, page, context, browser }) => {
        await populateQuestionnaire(request, questionnaireId, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();
        
        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);

        const isQuestionMode = await nextPreviewPage.isQuestionMode();
        expect(isQuestionMode).toBeTruthy();
    });

    test('Start preview with decorative image shows hero image and inner fieldset heading', async ({ page }) => {
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId);

        await stylingPage.uploadDecorativeImage('../../assets/sample-start-page-image.jpg');
        await stylingPage.acceptAccessibilityAgreement();
        await stylingPage.saveAndContinue();

        await designQuestionnairePage.openAddStartPage();
        const startPageEditor = await AddQuestionnaireStartPage.create(page);
        await startPageEditor.configureBasicStartPage('Decorative start', 'Description with image');
        await startPageEditor.clickSaveAndContinue();

        startPreviewPage = await goToQuestionnaireStartPreviewByUrl(page, questionnaireId, false);

        await startPreviewPage.assertStructure();
        await expect(startPreviewPage.outerImage, '❌ Decorative image not shown on start preview').toBeVisible();
    });

    test('Embedded preview uses inner fieldset heading even without decorative image', async ({ page }) => {
        await designQuestionnairePage.openAddStartPage();
        const startPageEditor = await AddQuestionnaireStartPage.create(page);
        await startPageEditor.configureBasicStartPage('Embedded start', 'Embedded description');
        await startPageEditor.clickSaveAndContinue();

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        startPreviewPage = await QuestionnaireStartPreviewPage.create(newPage);

        await startPreviewPage.assertStructure();
        await expect(startPreviewPage.innerFieldset, '❌ Inner fieldset should be hidden').toBeHidden();
    });

    test('Question preview – single-select question with valid selection continues to next state', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);
        
        const { content, response } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
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

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        await nextPreviewPage.assertSingleSelectQuestion();

        await nextPreviewPage.selectFirstRadioOption();
        await nextPreviewPage.clickContinue();

        // Results page is rendered properly
        await nextPreviewPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    });

    test('Question preview – multi-select question allows multiple choices and continues', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
        }, token)

        await createMultiSelectAnswerSet(request, questionnaireId, question.id, content.id, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        await nextPreviewPage.assertMultiSelectQuestion();

        await nextPreviewPage.selectFirstCheckboxOption();
        await nextPreviewPage.clickContinue();
    });

    test('Question preview – dropdown question requires a non-empty selection', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.DropdownSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
        }, token)

        await createDropdownAnswerSet(request, questionnaireId, question.id, content.id, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        await nextPreviewPage.assertDropdownQuestion();

        await nextPreviewPage.clickContinue();
        await nextPreviewPage.expectErrorSummary();

        await nextPreviewPage.selectDropdownByIndex(1);
        await nextPreviewPage.clickContinue();
    });

    test('Custom content destination is rendered correctly in preview run', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);
        
        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
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

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        const newPage = await designQuestionnairePage.openPreview();

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        await nextPreviewPage.assertSingleSelectQuestion();

        await nextPreviewPage.selectFirstRadioOption();
        await nextPreviewPage.clickContinue();

        const isCustomContent = await nextPreviewPage.isResultsPageMode();
        expect(isCustomContent).toBeTruthy();

        await nextPreviewPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    });

    test('External-link destination in embedded preview – hidden field is rendered instead of redirect', async ({ page, request }) => {
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

        startPreviewPage = await goToQuestionnaireStartPreviewByUrl(page, questionnaireId, true);

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(page);
        await nextPreviewPage.assertSingleSelectQuestion(true);

        await nextPreviewPage.selectFirstRadioOption();
        await nextPreviewPage.clickContinue();

        expect(page.url()).toBe(externalUrl);
    });

    test('Preview uses current custom styling (primary colour and button label)', async ({ request, page }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
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

        /*
        TextColor = "#0b0c0c";
        BackgroundColor = "#ffffff";
        PrimaryButtonColor = "#00703c";
        SecondaryButtonColor = "#1d70b8";
        StateColor = "#ffdd00";
        ErrorMessageColor = "#c3432b";
         */
        
        const updatedPrimaryButtonColor = '#005ea5';
        
        stylingPage = await goToQuestionnaireStylingPageByUrl(page, questionnaireId);
        await stylingPage.setPrimaryButtonColor(updatedPrimaryButtonColor);
        await stylingPage.acceptAccessibilityAgreement()
        await stylingPage.saveAndContinue();
        
        const response = await updateQuestionnaireContinueButton(request, questionnaireId, { continueButtonText: `Continue preview` }, token);
        expect200HttpStatusCode(response, 204);
        
        startPreviewPage = await goToQuestionnaireStartPreviewByUrl(page, questionnaireId, false);
        await startPreviewPage.assertStructure();

        // assert that the start page button text remains the same
        await startPreviewPage.assertStartButtonTextAndColor("Start now", updatedPrimaryButtonColor);

        await startPreviewPage.clickStartNow();
        
        const newContinueButtonText = "Continue preview"
        
        nextPreviewPage = await QuestionnaireNextPreviewPage.create(page);
        
        // to trigger the error state
        await nextPreviewPage.clickContinue();

        await nextPreviewPage.assertContinueButtonTextAndColor(newContinueButtonText, updatedPrimaryButtonColor);

        await nextPreviewPage.assertTextColor(textColor);

        await nextPreviewPage.assertErrorComponentsColor(errorMessageColor);
    });

    test('Multi select question with multiple answers selected and prioritised correctly', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { question: question2, questionPostResponse: question2PostResponse } = await createQuestion(request, questionnaireId, token, "Q1", QuestionType.MultiSelect, undefined);
        expect200HttpStatusCode(question2PostResponse, 201);

        const { content } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
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
        
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        const newPage = await designQuestionnairePage.openPreview();
        
        nextPreviewPage = await QuestionnaireNextPreviewPage.create(newPage);
        await nextPreviewPage.assertMultiSelectQuestion();
        
        await nextPreviewPage.selectAllCheckboxOptions();
        
        await nextPreviewPage.clickContinue();
        
        await nextPreviewPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    })
    

    test.skip('Preview run reflects contributors and results pages configuration in related admin journeys', async ({ page }) => {
        contributorsPage = await goToQuestionnaireContributorsPageByUrl(page, questionnaireId);
        await contributorsPage.assertPageElements();

        await contributorsPage.clickAddPerson();
        addContributorPage = await AddContributorPage.create(page);

        const contributorEmail = `00000000-0000-0000-0000-000000000002`;
        await addContributorPage.fillEmail(contributorEmail);
        await addContributorPage.clickSaveAndContinue();

        contributorsPage = await ViewContributorPage.create(page);
        await contributorsPage.expectContributorListed('', contributorEmail);

        resultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);
        await resultsPagesPage.assertPageElements();

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectTaskStatusReflectsConfiguredContributorsAndResults();
    });

    test.skip('Preview next page is reachable directly and remains in sync with admin state', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Preview direct next',
                destinationType: AnswerDestinationType.CustomContent,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        nextPreviewPage = await goToQuestionnaireNextPreviewByUrl(page, questionnaireId, false);
        await nextPreviewPage.assertSingleSelectQuestion();
        await nextPreviewPage.selectFirstRadioOption();
        await nextPreviewPage.clickContinue();

        await nextPreviewPage.expectErrorSummary();
    });

    test.skip('Accessibility – heading hierarchy and error summary behavior in preview', async ({ page, request }) => {
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { answerPostResponse } = await createSingleAnswer(
            request,
            {
                questionId: question.id,
                questionnaireId,
                content: 'Option 1',
                destinationType: AnswerDestinationType.CustomContent,
            },
            token
        );
        expect200HttpStatusCode(answerPostResponse, 201);

        startPreviewPage = await goToQuestionnaireStartPreviewByUrl(page, questionnaireId, false);
        await startPreviewPage.assertStructure();

        await startPreviewPage.clickStartNow();
        nextPreviewPage = await QuestionnaireNextPreviewPage.create(page);

        await nextPreviewPage.assertSingleSelectQuestion();

        await nextPreviewPage.clickContinue();

        const visible = await nextPreviewPage.errorSummary.isVisible().catch(() => false);
        if (visible) {
            await nextPreviewPage.expectErrorSummary();

            const firstLink = nextPreviewPage.errorSummary.locator('a.govuk-error-summary__link').first();
            await firstLink.click();
        }
    });
    
    // add test to confirm other logged in users to access the preview
    //  the questionnaire should have all the potential answer destinations, 
    //  the multi select should have priority selection
    test('Other logged in users can access the preview, even when they are not contributors', async ({ request, page, browser }) => {
        await designQuestionnairePage.openAddStartPage();
        const startPageEditor = await AddQuestionnaireStartPage.create(page);
        await startPageEditor.configureBasicStartPage('My preview title', 'Some description for preview');
        await startPageEditor.clickSaveAndContinue();
        
        const { question, questionPostResponse } = await createQuestion(request, questionnaireId, token);
        expect200HttpStatusCode(questionPostResponse, 201);

        const { content, response } = await createContent(request, {
            questionnaireId,
            title: 'Custom preview content',
            content: 'Custom preview content **markdown**',
            referenceName: 'custom-preview-content',
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

        // Create second browser context
        const context2 = await browser.newContext();
        const page2 = await context2.newPage();
        
        // the token of another user
        const token2 = JwtHelper.NoRecordsToken();

        await signIn(page2, token2);

        // View another user's questionnaire preview
        startPreviewPage = await goToQuestionnaireStartPreviewByUrl(page2, questionnaireId, false);
        await startPreviewPage.assertStructure();
        await startPreviewPage.clickStartNow()

        nextPreviewPage = await QuestionnaireNextPreviewPage.create(page2);
        await nextPreviewPage.assertSingleSelectQuestion();

        await nextPreviewPage.selectFirstRadioOption();
        await nextPreviewPage.clickContinue();

        // Results page is rendered properly
        await nextPreviewPage.assertResultsPage(content.title, content.content.replace(/\*\*markdown\*\*/g, 'markdown'));
    })
    
    // TODO: add a test to confirm the desired error states, when:
    //  - the questionnaire has no questions
    //  - the question has no answers
    //  - the answer has no destination
    //  - the last question in the flow has invalid answer destination
    
    async function populateQuestionnaire(request: any, questionnaireId: string, token: string) {
        await updateQuestionnaire(request, questionnaireId, { slug: `questionnaire-slug-${Math.floor(Math.random() * 1000000)}` }, token);

        await addContributor(request, questionnaireId, 'user-1', token)

        const { question } = await createQuestion(request, questionnaireId, token, 'Custom test questionnaire title', QuestionType.MultiSelect, undefined);

        await createSingleAnswer(request, {
            questionnaireId: questionnaireId, questionId: question.id, content: 'A1',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
    }

});