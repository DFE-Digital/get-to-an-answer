import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {goToAddQuestionnairePage, doSignIn} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";

test.describe('Get to an answer create a new questionnaire', () => {
    let newQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        const username = 'test'; //to be created dynamically
        const password = 'test'; //to be created dynamically

        await doSignIn(page, username, password);
    });

    test('Scenario: Successful submit posts to create endpoint - enter valid title and submit lands on EditQuestionnairePage', async ({ page }) => {
        
        const addQuestionnairePage = await goToAddQuestionnairePage(page);
        await addQuestionnairePage.createNewQuestionnaire('Automation Questionnaire');

        editQuestionnairePage = new EditQuestionnairePage(page);
        await editQuestionnairePage.validatePartialUrlMatches('admin/questionnaire/');
        
    });

    test('Validate presence of question and supportive text on new questionnaire page', async ({page}) => {
        newQuestionnairePage = await goToAddQuestionnairePage(page);
        await newQuestionnairePage.createNewQuestionnaire('Automation Questionnaire');
        
        editQuestionnairePage = new EditQuestionnairePage(page);
        await editQuestionnairePage.validatePartialUrlMatches('/questionnaire/');
    });
});