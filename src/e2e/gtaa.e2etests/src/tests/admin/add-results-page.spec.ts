import {expect, test} from "@playwright/test";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {
    signIn, goToEditQuestionnairePageByUrl, goToAddQuestionPageByUrl, goToAddQuestionnairePage,
    goToAddResultPagePageByUrl
} from '../../helpers/admin-test-helper';
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {PageHeadings} from "../../constants/test-data-constants";
import {AddResultsPagePage} from "../../pages/admin/AddResultsPagePage";
import {ViewResultsPagesPage} from "../../pages/admin/ViewResultsPagesPage";

test.describe('Get to an answer add results-page to questionnaire', () => {
    let token: string;
    let addResultsPagePage: AddResultsPagePage;
    let viewResultsPagesPage: ViewResultsPagesPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);

        await signIn(page, token);
        addResultsPagePage = await goToAddResultPagePageByUrl(page, questionnaire.id);
        await addResultsPagePage.expectAddResultsPageHeadingOnPage(PageHeadings.ADD_RESULTS_PAGE_PAGE_HEADING);
    });

    test('Validate presence of elements on add new results-page page', async ({page}) => {
        await addResultsPagePage.assertPageElements();
    });
    
    test('Back link takes to view results-page list from Add results-page page', async ({page}) => {
        await addResultsPagePage.verifyBackLinkPresent();
        await addResultsPagePage.clickBackLink();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
    });

    test('Error summary appears on invalid submit with all missing required fields', async ({browserName}) => {
        await addResultsPagePage.clickSaveAndContinue();
        await addResultsPagePage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing results-page title', async ({browserName}) => {
        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);

        const resultsPageRefNameInput = `Test ref name - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageRefNameInput(resultsPageRefNameInput);
        
        await addResultsPagePage.clickSaveAndContinue();
        await addResultsPagePage.validateMissingTitleErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing results-page details text', async ({browserName}) => {
        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);

        const resultsPageRefNameInput = `Test ref name - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageRefNameInput(resultsPageRefNameInput);
        
        await addResultsPagePage.clickSaveAndContinue();

        await addResultsPagePage.clickSaveAndContinue();
        await addResultsPagePage.validateMissingDetailsErrorMessageSummary(browserName);
    });

    test('Successful submit with missing optional results-page ref name input', async ({page}) => {
        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);

        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);

        await addResultsPagePage.clickSaveAndContinue();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
        await viewResultsPagesPage.expectSuccessBannerVisible();
        await viewResultsPagesPage.assertCreatedResultsPageSuccessBanner();
    });

    test('Inline error and styling for missing results-page title', async ({page}) => {
        await addResultsPagePage.clearResultsPageTitleInput();
        await addResultsPagePage.clickSaveAndContinue();

        await addResultsPagePage.validateInlineTitleError();
        await addResultsPagePage.validateInlineDetailsError();
    });

    test('Inline error and styling for missing results-page details', async ({page}) => {
        await addResultsPagePage.clearResultsPageDetailsText();
        await addResultsPagePage.clickSaveAndContinue();

        await addResultsPagePage.validateInlineTitleError();
        await addResultsPagePage.validateInlineDetailsError();
    });
});