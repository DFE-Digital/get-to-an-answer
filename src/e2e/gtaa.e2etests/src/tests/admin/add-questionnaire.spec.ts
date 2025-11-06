import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {goToAddQuestionnairePage, doSignIn} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";

test.describe('Get to an answer create a new questionnaire', () => {
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        const username = 'test'; //to be created dynamically
        const password = 'test'; //to be created dynamically

        viewQuestionnairePage = await doSignIn(page, username, password);
        await viewQuestionnairePage.clickCreateNewQuestionnaire();
    });

    test('Add a new questionnaire successfully and lands on Edit Questionnaire Page', async ({ page }) => {
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.addQuestionnaire();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.expectUrlOnPage();
        await editQuestionnairePage.expectSuccessBannerVisible();
    });

    test('Validate presence of elements on add new questionnaire page', async ({page}) => {
        addQuestionnairePage = await goToAddQuestionnairePage(page);
        await addQuestionnairePage.assertPageElements();
    });

    test('click back to questionnaire link on add new questionnaire page', async ({page}) => {
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.ClickBackToQuestionnaireLink();

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectUrlOnPage();
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();
    });

    test('Submit a new questionnaire with missing title', async ({page}) => {
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterTitle(''); 
        await addQuestionnairePage.clickSaveAndContinue();
        
        await addQuestionnairePage.validateMissingTitleMessageFlow();
    });

    test('Submit a new questionnaire with invalid title', async ({page}) => {
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterTitle('@@@+++###~~~');
        await addQuestionnairePage.clickSaveAndContinue();

        await addQuestionnairePage.validateInvalidTitleMessageFlow();
    });

    test('Submit a new questionnaire with invalid title to validate aria-describedby', async ({page}) => {
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.enterTitle('@@@+++###~~~');
        await addQuestionnairePage.clickSaveAndContinue();

        await addQuestionnairePage.expectTitleAriaDescribedByIncludesHintAndError();
    });

    test('should prevent double submission for a questionnaire', async ({ page }) => {
        addQuestionnairePage = new AddQuestionnairePage(page);
        await addQuestionnairePage.enterTitle('Double click prevention test');
        
        await addQuestionnairePage.submitFormAndCheckNoDoubleSubmit();
    });
});