import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {
    goToEditQuestionnairePageByUrl,
    goToUpdateQuestionnairePageByUrl,
    goToViewResultsPagesPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, publishQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, deleteQuestion} from "../../test-data-seeder/question-data";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {AnswerDestinationType, ErrorMessages, PageHeadings} from "../../constants/test-data-constants";
import {ViewResultsPagesPage} from "../../pages/admin/ViewResultsPagesPage";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Get to an answer view questions', () => {
    let token: string;
    let questionnaireId: string;
    let qResp3: any;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;
    let viewResultsPagesPage: ViewResultsPagesPage;
    let addQuestionPage: AddQuestionPage;

    test.beforeEach(async ({page, request}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const qResp1 = await createContent(request, {
            questionnaireId,
            title: 'Content 1',
            content: 'Content description 1',
        }, token);
        const qResp2 = await createContent(request, {
            questionnaireId,
            title: 'Content 2',
            content: 'Content description 2',
        }, token);
        qResp3 = await createContent(request, {
            questionnaireId,
            title: 'Content 3',
            content: 'Content description 3',
        }, token);
        const qResp4 = await createContent(request, {
            questionnaireId,
            title: 'Content 4',
            content: 'Content description 4',
        }, token);

        expect200HttpStatusCode(qResp1.response, 201);
        expect200HttpStatusCode(qResp2.response, 201);
        expect200HttpStatusCode(qResp3.response, 201);
        expect200HttpStatusCode(qResp4.response, 201);
    });


    test('Validate presence of elements on view results-page page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.assertPageElements();
    });

    test("Header section - H1 and results-page status", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.expectResultsPagesHeadingOnPage();
    });

    test("Add results-page CTA navigates to Add results-page", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.clickAddResultsPage();

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage();
    });

    test('Back to edit questionnaire link navigation from view questions page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.ClickBackToEditQuestionnaireLink();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        expect(editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING));
    });

    test('Save and continue with No I will come back later radio navigates to Edit questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.markFinishedEditing(false);
        await viewResultsPagesPage.expectComeBackLaterRadioIsSelected();

        await viewResultsPagesPage.saveAndContinue();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('Save and continue with Yes radio navigates to Edit questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.expectYesRadioIsNotSelected();
        await viewResultsPagesPage.markFinishedEditing(true);

        await viewResultsPagesPage.saveAndContinue();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('List existing results pages', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewResultsPagesPage = await goToViewResultsPagesPageByUrl(page, questionnaireId);

        await viewResultsPagesPage.expectResultsPagesHeadingOnPage();

        await viewResultsPagesPage.table.verifyListExistsWithTitleAndActions();
    });
});