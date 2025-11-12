import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {goToAddQuestionnairePage, localSignIn} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('Get to an answer create a new questionnaire', () => {
    let token: string;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        token = JwtHelper.NoRecordsToken();
        viewQuestionnairePage = await localSignIn(page, token);
    });

    test('Add a new questionnaire successfully and lands on Edit Questionnaire Page', async ({ page }) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();

        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.addQuestionnaire();

         editQuestionnairePage = await EditQuestionnairePage.create(page);
         await editQuestionnairePage.validateHeadingAndStatus();
         await editQuestionnairePage.expectSuccessBannerVisible();
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

    //TBC, focus after clicking the error message
    // test('Submit a new questionnaire with missing title', async ({page}) => {
    //     await viewQuestionnairePage.clickCreateNewQuestionnaire();
    //
    //     addQuestionnairePage = await AddQuestionnairePage.create(page);
    //     await addQuestionnairePage.enterTitle(''); 
    //     await addQuestionnairePage.clickSaveAndContinue();
    //
    //     await addQuestionnairePage.validateMissingTitleMessageFlow();
    // });

    //TBC, we don't know if special characters are allowed in title
    // test('Submit a new questionnaire with invalid title', async ({page}) => {
    //     await viewQuestionnairePage.clickCreateNewQuestionnaire();
    //
    //     addQuestionnairePage = await AddQuestionnairePage.create(page);
    //     await addQuestionnairePage.enterTitle('@@@+++###~~~');
    //     await addQuestionnairePage.clickSaveAndContinue();
    //
    //     await addQuestionnairePage.validateInvalidTitleMessageFlow();
    // });

    //TBC
    // test('Submit a new questionnaire with invalid title to validate aria-describedby', async ({page}) => {
    //     await viewQuestionnairePage.clickCreateNewQuestionnaire();
    //
    //     addQuestionnairePage = await AddQuestionnairePage.create(page);
    //     await addQuestionnairePage.enterTitle('@@@+++###~~~');
    //     await addQuestionnairePage.clickSaveAndContinue();
    //
    //     await addQuestionnairePage.expectTitleAriaDescribedByIncludesHintAndError();
    // });
});