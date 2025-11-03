import {test} from "@playwright/test";
import {NewQuestionnairePage} from "../../pages/admin/NewQuestionnairePage";
import {goToNewQuestionnairePage, doSignIn} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";

test.describe('Get to an answer create a new questionnaire', () => {
    let newQuestionnairePage: NewQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({page}) => {
        editQuestionnairePage = new EditQuestionnairePage(page);
        
        const username = 'test'; //to be created dynamically
        const password = 'test'; //to be created dynamically

        await doSignIn(page, username, password);
        newQuestionnairePage = await goToNewQuestionnairePage(page);
    });

    test('Validate presence of question and supportive text on new questionnaire page', async ({page}) => {
        await newQuestionnairePage.createNewQuestionnaire('Automation Questionnaire');
        await editQuestionnairePage.validateUrlContains('/questionnaire/');
    });
});