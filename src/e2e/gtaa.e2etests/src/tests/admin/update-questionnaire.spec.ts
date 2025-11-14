import {test, expect} from "@playwright/test";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {signIn, goToUpdateQuestionnairePageByUrl} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {EntityStatus, ErrorMessages} from "../../constants/test-data-constants";
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";

test.describe('Get to an answer update questionnaire', () => {
    let token: string;
    let questionnaireGetResponse:any;
    
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);

        questionnaireGetResponse = await getQuestionnaire(
            request,
            questionnaire.id,
            token
        );

        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaire.id);
    });

    test('Edit Title page displays all required elements', async ({page}) => {
        await addQuestionnairePage.assertPageElements();
    });

    test('Back link to questionnaire from Edit Title page', async ({page}) => {
        await addQuestionnairePage.ClickBackToQuestionnaireLink();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        expect(editQuestionnairePage.validateHeading());
    });

    test('Error summary appears on submit with missing title', async ({page, browserName}) => {
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);  
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });

    test('Inline error message and styling when title field has error', async ({page, browserName}) => {
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });

    test('Accessible aria-describedby includes hint id and error message id', async ({page}) => {
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateTitleFieldAriaDescribedBy();
    });

    // TBC - CreateddBy still empty
    test('Successful submit updates title and validation', async ({request, page}) => {
        const newTitle = `Updated questionnaire title - ${Date.now()}`;
        await addQuestionnairePage.enterTitle(newTitle);
        await addQuestionnairePage.clickSaveAndContinue();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.expectSuccessBannerVisible();
        await editQuestionnairePage.validateHeadingAndStatus();

        await editQuestionnairePage.ClickBackToQuestionnaireLink()
        viewQuestionnairePage = await ViewQuestionnairePage.create(page);

        const statusValue = questionnaireGetResponse.questionnaireGetBody.status;
        const statusName = EntityStatus[statusValue];

        questionnaireGetResponse = await getQuestionnaire(request, questionnaireGetResponse.questionnaireGetBody.id, token);
        
        const expectedRows = [
            {
                title: questionnaireGetResponse.questionnaireGetBody.title,
                createdBy: questionnaireGetResponse.questionnaireGetBody.createdBy,
                status: statusName
            }
        ];

        await viewQuestionnairePage.table.verifyTableData(expectedRows);
    });
});

