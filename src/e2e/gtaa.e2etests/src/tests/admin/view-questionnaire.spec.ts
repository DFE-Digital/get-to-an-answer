import {test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {doSignIn} from "../../helpers/admin-test-helper";

test.describe('Get to an answer views questionnaire', () => {
    let viewQuestionnairePage: ViewQuestionnairePage;

    test.beforeEach(async ({page}) => {
        const username = 'test'; //to be created dynamically
        const password = 'test'; //to be created dynamically

        viewQuestionnairePage = await doSignIn(page, username, password);
    });

    test('Verify questionnaires table on view page', async ({page}) => {
        
    });
});