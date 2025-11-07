import {test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {doSignIn, goToAddQuestionnairePage} from "../../helpers/admin-test-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";

test.describe('Get to an answer views questionnaire', () => {
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        const username = 'test'; //to be created dynamically
        const password = 'test'; //to be created dynamically

        // add a new questionnaire by API
        viewQuestionnairePage = await doSignIn(page, username, password);
    });

    test('Validate presence of elements on view questionnaire page', async ({page}) => {
        await viewQuestionnairePage.assertPageElements();
    });

    test("Header section - H1 and paragraph presence", async () => {
        await viewQuestionnairePage.verifyHelpUserHeadingVisible();
        await viewQuestionnairePage.verifyHelpUserDescriptionVisible();
    });

    test("Create questionnaire CTA navigates to Add page", async ({page}) => {
        await viewQuestionnairePage.clickCreateNewQuestionnaire();
        
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.expectUrlOnPage();
        await addQuestionnairePage.verifyLabelAndHintPresent();
    });

    test("Questionnaires table - columns, title link navigates", async ({page}) => {
        await viewQuestionnairePage.table.verifyHeaders();
        await viewQuestionnairePage.table.verifyFirstTitleIsLink();
        
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.expectUrlOnPage();
        await editQuestionnairePage.validateHeadingAndStatus();
    });

    test("Questionnaires table - status as draft", async ({page}) => {
        await viewQuestionnairePage.table.expectRowPresentByName('pass the unique questionnaire title from api');
        
        // based on unique title this will return the status as draft 
        await viewQuestionnairePage.table.getStatus('pass the unique questionnaire title from api');
        
    });
});