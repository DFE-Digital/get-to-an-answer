import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {goToAddQuestionnairePage, signIn} from '../../helpers/admin-test-helper';
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {ErrorMessages} from "../../constants/test-data-constants";

test.describe('Get to an answer create a new questionnaire', () => {
    let token: string;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({page}) => {
        token = JwtHelper.NoRecordsToken();
        viewQuestionnairePage = await signIn(page, token);
    });
    
    test('Add a new questionnaire successfully and lands on Edit Questionnaire Page', async ({ page }) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.addQuestionnaire();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
         await designQuestionnairePage.validateHeadingAndStatus();
         await designQuestionnairePage.expectSuccessBannerVisible();
    });

    test('Validate presence of elements on add new questionnaire page', async ({page}) => {
        addQuestionnairePage = await goToAddQuestionnairePage(page);
        await addQuestionnairePage.assertPageElements();
    });

    test('click back to questionnaire link on add new questionnaire page', async ({page}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.ClickBackToQuestionnaireLink();

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();
    });
    
    test('Submit a new questionnaire with missing title', async ({page, browserName}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterTitle(''); 
        await addQuestionnairePage.clickSaveAndContinue();

        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await addQuestionnairePage.validateErrorLinkBehaviour(
            addQuestionnairePage.errorSummaryLink('#Title'),
            ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE,
            browserName
        );
        
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });
    
    test('Submit a new questionnaire with invalid title', async ({page, browserName}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterInvalidTitle();
        await addQuestionnairePage.clickSaveAndContinue();

        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });
    
    test('Submit a new questionnaire with invalid title to validate aria-describedby', async ({page}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterInvalidTitle();
        await addQuestionnairePage.clickSaveAndContinue();

        await addQuestionnairePage.validateTitleFieldAriaDescribedBy();
    });
});