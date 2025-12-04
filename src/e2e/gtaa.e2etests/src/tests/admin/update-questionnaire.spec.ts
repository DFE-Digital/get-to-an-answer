import {test, expect} from "@playwright/test";
import {
    createQuestionnaire,
    getQuestionnaire,
    listQuestionnaires,
    updateQuestionnaire
} from "../../test-data-seeder/questionnaire-data";

import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {
    signIn,
    goToUpdateQuestionnairePageByUrl,
    goToEditQuestionnairePageByUrl, goToUpdateQuestionPageByUrl
} from '../../helpers/admin-test-helper';
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {EntityStatus, ErrorMessages, PageHeadings} from "../../constants/test-data-constants";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {DeleteQuestionnaireConfirmationPage} from "../../pages/admin/DeleteQuestionnaireConfirmationPage";

test.describe('Get to an answer update questionnaire', () => {
    let token: string;
    let questionnaireGetResponse:any;
    
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

    test('Edit Title page displays all required elements', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);
        await addQuestionnairePage.assertPageElements();
    });

    test('Back link to questionnaire from Edit Title page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);
        await addQuestionnairePage.ClickBackToQuestionnaireLink();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        expect(editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING));
    });

    test('Error summary appears on submit with missing title', async ({page, browserName}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);
        
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);  
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });

    test('Inline error message and styling when title field has error', async ({page, browserName}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);
        
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await addQuestionnairePage.validateInlineTitleError();
        await addQuestionnairePage.validateTitleFormGroup();
    });

    // TBC, aria-described by id's need correction (CARE-1546)
    test('Accessible aria-describedby includes hint id and error message id', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);
        
        await addQuestionnairePage.enterTitle('');
        await addQuestionnairePage.clickSaveAndContinue();
        await addQuestionnairePage.validateTitleFieldAriaDescribedBy();
    });

    test('Successful submit updates title and validation', async ({request, page}) => {
        viewQuestionnairePage = await signIn(page, token);
        addQuestionnairePage = await goToUpdateQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        // Capture the original title before updating
        const originalTitle = questionnaireGetResponse.questionnaireGetBody.title;
        expect(originalTitle).toBeDefined();

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

        const lisQuestionnaireResponse = await listQuestionnaires(request, token);
        const list: any[] = lisQuestionnaireResponse.questionnaireGetBody
        expect(list.length).toBeGreaterThan(0);
        const firstQuestionnaire = list[0];

        questionnaireGetResponse = await getQuestionnaire(request, questionnaireGetResponse.questionnaireGetBody.id, token);

        // Verify the title was actually updated from the original
        expect(questionnaireGetResponse.questionnaireGetBody.title).not.toBe(originalTitle);
        expect(questionnaireGetResponse.questionnaireGetBody.title).toBe(newTitle);

        const expectedRows = [
            {
                title: questionnaireGetResponse.questionnaireGetBody.title,
                createdBy: firstQuestionnaire.createdBy,
                updatedAt: questionnaireGetResponse.questionnaireGetBody.updatedAt,
                status: statusName
            }
        ];
        await viewQuestionnairePage.table.verifyTableData(expectedRows);
    });

    test('Duplicate slug from another questionnaire is not allowed ', async ({request, page}) => {
        const initialSlug = `questionnaire-slug-${Math.floor(Math.random() * 1000000000)}`;
        
        const {
            updatedQuestionnairePostResponse
        } = await updateQuestionnaire(
            request,
            questionnaireGetResponse.questionnaireGetBody.id,
            {
                slug: initialSlug
            }, token
        );
        
        expect200HttpStatusCode(updatedQuestionnairePostResponse, 204);
        
        const {questionnaire} = await createQuestionnaire(request, token); // second questionnaire
        
        viewQuestionnairePage = await signIn(page, token);

        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaire.id);
        await editQuestionnairePage.openEditSlug();

        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);

        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING)
        await updateQuestionnaireSlugPage.enterSlug(initialSlug);

        await updateQuestionnaireSlugPage.submit()
        
        await updateQuestionnaireSlugPage.validateDuplicateSlugMessageSummary('webkit')
        await updateQuestionnaireSlugPage.validateInlineSlugError();
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

    //TBC, CARE-1573 bug raised
    test('Delete questionnaire back link returns to edit page', async ({page}) => {
        await signIn(page, token);

        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireGetResponse.questionnaireGetBody.id);

        await editQuestionnairePage.deleteQuestionnaire();

        const deleteConfirmationPage = new DeleteQuestionnaireConfirmationPage(page);
        await deleteConfirmationPage.clickBackLink();

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