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
    });

    test('Add a mew questionnaire successfully and lands on Edit Questionnaire Page', async ({ page }) => {
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
});