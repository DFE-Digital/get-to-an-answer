import {test, expect} from "@playwright/test";
import {
    createQuestionnaire,
    getQuestionnaire,
    listQuestionnaires
} from "../../test-data-seeder/questionnaire-data";

import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {signIn, goToEditQuestionnairePageByUrl} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {DeleteQuestionnaireConfirmationPage} from "../../pages/admin/DeleteQuestionnaireConfirmationPage";

test.describe('Get to an answer update questionnaire', () => {
    let token: string;
    let questionnaireGetResponse: any;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);

        questionnaireGetResponse = await getQuestionnaire(
            request,
            questionnaire.id,
            token
        );
    });
    test('Delete a questionnaire successfully', async ({page}) => {
        await signIn(page, token);

        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        await editQuestionnairePage.deleteQuestionnaire();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.expectTwoRadiosPresent();
        await deleteConfirmationPage.chooseYes();
        await deleteConfirmationPage.clickContinue();

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();
    });

    test('Delete questionnaire with cancellation returns to edit page', async ({page}) => {
        await signIn(page, token);

        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        await editQuestionnairePage.deleteQuestionnaire();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.expectTwoRadiosPresent();
        await deleteConfirmationPage.chooseNo();
        await deleteConfirmationPage.clickContinue();

        editQuestionnairePage = new EditQuestionnairePage(page);
        await editQuestionnairePage.validateHeadingAndStatus();
    });
    
    test('Delete questionnaire removes questionnaire from questionnaire list', async ({request, page}) => {
        await signIn(page, token);

        const questionnaireIdToDelete = questionnaireGetResponse.questionnaireGetBody.id;
        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireIdToDelete);

        await editQuestionnairePage.deleteQuestionnaire();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.expectTwoRadiosPresent();
        await deleteConfirmationPage.chooseYes();
        await deleteConfirmationPage.clickContinue();

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();

        const listQuestionnaireResponse = await listQuestionnaires(request, token);
        const list: any[] = listQuestionnaireResponse.questionnaireGetBody;
        const deletedQuestionnaire = list.find((q: any) => q.id === questionnaireIdToDelete);

        expect(deletedQuestionnaire).toBeUndefined();
    });
});