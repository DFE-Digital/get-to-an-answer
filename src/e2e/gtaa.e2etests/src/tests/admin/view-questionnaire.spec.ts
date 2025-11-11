import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {localSignIn, goToAddQuestionnairePage} from "../../helpers/admin-test-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {JwtHelper} from "../../helpers/JwtHelper";

test.describe('Get to an answer views questionnaire', () => {
    let token: string;
    let questionnaire: any;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;

    test.beforeEach(async () => {
        token = JwtHelper.NoRecordsToken();
    });

    test('Validate presence of elements on view questionnaire page', async ({page}) => {

        viewQuestionnairePage = await localSignIn(page, token);
        await viewQuestionnairePage.assertPageElements();
    });

    test("Header section - H1 and paragraph presence", async ({page}) => {
        viewQuestionnairePage = await localSignIn(page, token);

        await viewQuestionnairePage.verifyHelpUserHeadingVisible();
        await viewQuestionnairePage.verifyHelpUserDescriptionVisible();
    });

    test("Create questionnaire CTA navigates to Add page", async ({page}) => {
        viewQuestionnairePage = await localSignIn(page, token);

        await viewQuestionnairePage.clickCreateNewQuestionnaire();
        addQuestionnairePage = await AddQuestionnairePage.create(page);
        await addQuestionnairePage.verifyLabelAndHintPresent();
    });

    test("Questionnaires table - columns, title link navigates", async ({page, request}) => {
        const res = await createQuestionnaire(request, token);
        questionnaire = res.questionnaire;

        viewQuestionnairePage = await localSignIn(page, token);

        await viewQuestionnairePage.table.verifyHeaders();
        await viewQuestionnairePage.table.verifyFirstTitleIsLink();

        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeadingAndStatus();
    });
    
    test("Questionnaires table - status as draft", async ({page}) => {
        const status = await viewQuestionnairePage.table.getStatus(questionnaire.title);
        expect(status).toBe('Draft');
    });

    // test("Questionnaires table - validate data", async ({request}) => {
    //     const res = await createQuestionnaire(request, token);
    //     questionnaire = res.questionnaire;
    //
    //     //need get questionnaire api call
    //     const getResponse = await getQuestionnaire(
    //         request,
    //         questionnaire.id,
    //         token
    //     );
    //
    //     const expectedRows = [
    //         {
    //             title: getResponse.questionnaireGetBody.title,
    //             createdBy: getResponse.questionnaireGetBody.createdBy,
    //             status: getResponse.questionnaireGetBody.status,
    //         }
    //     ];
    //
    //     await viewQuestionnairePage.table.verifyTableData(expectedRows);
    // });
});