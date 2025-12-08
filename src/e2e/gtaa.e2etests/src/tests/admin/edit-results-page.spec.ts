import {expect, test} from "@playwright/test";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {
    signIn, goToEditQuestionnairePageByUrl, goToAddQuestionPageByUrl, goToAddQuestionnairePage,
    goToEditResultPagePageByUrl
} from '../../helpers/admin-test-helper';
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, getQuestionnaire, updateQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {PageHeadings} from "../../constants/test-data-constants";
import {AddResultsPagePage} from "../../pages/admin/AddResultsPagePage";
import {ViewResultsPagesPage} from "../../pages/admin/ViewResultsPagesPage";
import {createContent} from "../../test-data-seeder/content-data";
import {EditResultsPagePage} from "../../pages/admin/EditResultsPagePage";
import {RemoveStartPageConfirmationPage} from "../../pages/admin/RemoveStartPageConfirmationPage";
import {RemoveResultsPageConfirmationPage} from "../../pages/admin/RemoveResultsPageConfirmationPage";

test.describe('Get to an answer edit results-page to questionnaire', () => {
    let token: string;
    let editResultsPagePage: EditResultsPagePage;
    let viewResultsPagesPage: ViewResultsPagesPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        const { content } = await createContent(request, {
            questionnaireId: questionnaire.id,
            title: "Test title",
            content: "Test content",
            referenceName: "Test ref name"
        }, token);

        await signIn(page, token);
        editResultsPagePage = await goToEditResultPagePageByUrl(page, questionnaire.id, content.id);
        await editResultsPagePage.expectAddResultsPageHeadingOnPage(PageHeadings.EDIT_RESULTS_PAGE_PAGE_HEADING);
    });

    test('Validate presence of elements on edit results-page page', async ({page}) => {
        await editResultsPagePage.assertPageElements();
    });
    
    test('Back link takes to view results-page list from edit results-page page', async ({page}) => {
        await editResultsPagePage.verifyBackLinkPresent();
        await editResultsPagePage.clickBackLink();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
    });

    test('Error summary appears on invalid submit with all missing required fields', async ({browserName}) => {
        await editResultsPagePage.clearResultsPageTitleInput();
        await editResultsPagePage.clearResultsPageDetailsText();
        await editResultsPagePage.clearResultsPageRefNameInput();
        
        await editResultsPagePage.clickSaveAndContinue();
        await editResultsPagePage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing results-page title', async ({browserName}) => {
        await editResultsPagePage.clearResultsPageTitleInput();

        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);

        const resultsPageRefNameInput = `Test ref name - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageRefNameInput(resultsPageRefNameInput);
        
        await editResultsPagePage.clickSaveAndContinue();
        await editResultsPagePage.validateMissingTitleErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing results-page details text', async ({browserName}) => {
        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);

        await editResultsPagePage.clearResultsPageDetailsText();
        
        const resultsPageRefNameInput = `Test ref name - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageRefNameInput(resultsPageRefNameInput);
        
        await editResultsPagePage.clickSaveAndContinue();

        await editResultsPagePage.clickSaveAndContinue();
        await editResultsPagePage.validateMissingDetailsErrorMessageSummary(browserName);
    });

    test('Successful submit with missing optional results-page ref name input', async ({page}) => {
        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);

        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await editResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);
        
        await editResultsPagePage.clearResultsPageRefNameInput();

        await editResultsPagePage.clickSaveAndContinue();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
        await viewResultsPagesPage.expectSuccessBannerVisible();
        await viewResultsPagesPage.assertUpdatedResultsPageSuccessBanner();
    });

    test('Inline error and styling for missing results-page title', async ({page}) => {
        await editResultsPagePage.clearResultsPageTitleInput();
        await editResultsPagePage.clickSaveAndContinue();

        await editResultsPagePage.validateInlineTitleError();
    });

    test('Inline error and styling for missing results-page details', async ({page}) => {
        await editResultsPagePage.clearResultsPageDetailsText();
        await editResultsPagePage.clickSaveAndContinue();

        await editResultsPagePage.validateInlineDetailsError();
    });

    test('Successful remove results page', async ({request, page}) => {
        await editResultsPagePage.clickRemoveResultsPage();

        const removeResultsPageConfirmationPage = new RemoveResultsPageConfirmationPage(page);

        await removeResultsPageConfirmationPage.expectTwoRadiosPresent();
        await removeResultsPageConfirmationPage.chooseYes();
        await removeResultsPageConfirmationPage.clickContinue();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectSuccessBannerVisible();
        await viewResultsPagesPage.assertDeletedResultsPageSuccessBanner();
    });
});