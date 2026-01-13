import {test, expect} from "@playwright/test";
import {
    createQuestionnaire,
    getQuestionnaire,
    listQuestionnaires
} from "../../test-data-seeder/questionnaire-data";

import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {signIn, goToDesignQuestionnairePageByUrl} from '../../helpers/admin-test-helper';
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {DeleteQuestionnaireConfirmationPage} from "../../pages/admin/DeleteQuestionnaireConfirmationPage";
import {ErrorMessages, SuccessBannerMessages} from "../../constants/test-data-constants";

test.describe('Get to an answer delete questionnaire', () => {
    let token: string;
    let questionnaireGetResponse: any;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;
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

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        await designQuestionnairePage.clickDeleteQuestionnaireButton();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.expectTwoRadiosPresent();
        await deleteConfirmationPage.chooseYes();
        await deleteConfirmationPage.clickContinue();

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();

        await viewQuestionnairePage.assertQuestionnaireDeletionSuccessBanner(
            SuccessBannerMessages.DELETED_QUESTIONNAIRE_SUCCESS_MESSAGE
                .replace('**name**', questionnaireGetResponse.questionnaireGetBody.title)
        )
    });

    test('Delete questionnaire with cancellation returns to edit page', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        await designQuestionnairePage.clickDeleteQuestionnaireButton();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.expectTwoRadiosPresent();
        await deleteConfirmationPage.chooseNo();
        await deleteConfirmationPage.clickContinue();

        designQuestionnairePage = new DesignQuestionnairePage(page);
        await designQuestionnairePage.validateHeadingAndStatus();
    });

    test('Delete questionnaire removes questionnaire from questionnaire list', async ({request, page}) => {
        await signIn(page, token);

        const questionnaireIdToDelete = questionnaireGetResponse.questionnaireGetBody.id;
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireIdToDelete);

        await designQuestionnairePage.clickDeleteQuestionnaireButton();

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

        viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.assertQuestionnaireDeletionSuccessBanner(
            SuccessBannerMessages.DELETED_QUESTIONNAIRE_SUCCESS_MESSAGE
                .replace('**name**', questionnaireGetResponse.questionnaireGetBody.title)
        )
    });
});