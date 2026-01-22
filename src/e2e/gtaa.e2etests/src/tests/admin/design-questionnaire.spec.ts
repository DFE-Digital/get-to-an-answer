import {test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {
    goToAddQuestionnairePage,
    goToAddQuestionPageByUrl,
    goToDesignQuestionnairePageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {AnswerDestinationType, PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {AddQuestionnaireStartPage} from "../../pages/admin/AddQuestionnaireStartPage";
import {EditContinueButtonTextPage} from "../../pages/admin/EditContinueButtonTextPage";
import {QuestionnaireStylingPage} from "../../pages/admin/QuestionnaireStylingPage";
import {AddResultsPagePage} from "../../pages/admin/AddResultsPagePage";
import {ViewResultsPagesPage} from "../../pages/admin/ViewResultsPagesPage";
import {DeleteQuestionConfirmationPage} from "../../pages/admin/DeleteQuestionConfirmationPage";

test.describe('Get to an answer create a new questionnaire', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    
    let designQuestionnairePage: DesignQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionPage: AddQuestionPage;
    let viewQuestionPage: ViewQuestionPage;
    let addAnswerPage: AddAnswerPage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;
    let addQuestionnaireStartPage: AddQuestionnaireStartPage;
    let editContinueButtonTextPage: EditContinueButtonTextPage;
    let questionnaireStylingPage: QuestionnaireStylingPage;
    let addResultsPagePage: AddResultsPagePage;
    let viewResultsPagesPage: ViewResultsPagesPage;
    
    test.beforeEach(async ({page, request}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
    });

    test('Validate presence of core elements on design questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.assertPageElements();
    });

    test('Validate presence of different sections on design questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.validateAllSectionsAndRelatedLinks();
    });

    test('Validate presence of optional tasks on design questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.validateOptionalTasks();
    });

    test('Validate presence of not started tasks on design questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.validateNotStartedTasks();
    });

    test('Click on back takes navigates to view questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.ClickBackToQuestionnaireLink();

        const viewQuestionnairePage = await ViewQuestionnairePage.create(page);
        await viewQuestionnairePage.expectQuestionnaireHeadingOnPage();
    });

    test('Validate questionnaire default status on questionnaire page', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        const questionnaireStatus = await designQuestionnairePage.getQuestionnaireStatus();
        await designQuestionnairePage.validateQuestionnaireDefaultStatus(questionnaireStatus);
    });

    test('Validate add or edit questions and answers link In Progress status', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.openAddEditQuestionsAnswers();
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.clickAddQuestion();

        addQuestionPage = await AddQuestionPage.create(page);
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        await addAnswerPage.clickSaveAndContinueButton();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        await viewQuestionPage.saveAndContinue();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('In progress', 'Add or edit questions and answers');
    });

    test('Validate add or edit questions and answers link status as Completed', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.openAddEditQuestionsAnswers();
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.clickAddQuestion();

        addQuestionPage = await AddQuestionPage.create(page);
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        await addAnswerPage.clickSaveAndContinueButton();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        await viewQuestionPage.markFinishedEditing(true)
        await viewQuestionPage.saveAndContinue();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.assertQuestionnaireUpdatedSuccessBanner()
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('Completed', 'Add or edit questions and answers');
    });

    test('Validate add or edit questionnaire Id link status as Completed', async ({page}) => {
        const inputSlug = `slug-${Date.now()}`;
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.createQuestionnaireId();
        
        updateQuestionnaireSlugPage = await UpdateQuestionnaireSlugPage.create(page);
        await updateQuestionnaireSlugPage.expectHeadingOnEditSlugPage(PageHeadings.EDIT_QUESTIONNAIRE_SLUG_PAGE_HEADING)
        await updateQuestionnaireSlugPage.enterSlug(inputSlug);

        await updateQuestionnaireSlugPage.submit()

        await designQuestionnairePage.taskStatusAddEditQuestionnaireId('Completed', 'Add or edit questionnaire ID');
    });

    test('Validate add and edit start page link status as Completed', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.taskStatusAddEditStartPage('Optional', 'Add and edit start page');

        await designQuestionnairePage.openStartPage();
        
        addQuestionnaireStartPage = await AddQuestionnaireStartPage.create(page);
        await addQuestionnaireStartPage.enterQuestionnaireDisplayTitleInput("Questionnaire Start Page")
        await addQuestionnaireStartPage.enterQuestionnaireDescriptionText("Questionnaire Description");
        await addQuestionnaireStartPage.clickSaveAndContinue();
        
        await designQuestionnairePage.taskStatusAddEditStartPage('Completed', 'Add and edit start page');
    });

    test('Validate edit button text page link status as Completed', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.taskStatusEditButtonTextPage('Optional', 'Edit button text');

        await designQuestionnairePage.openEditButtonText();

        editContinueButtonTextPage = await EditContinueButtonTextPage.create(page);
        await editContinueButtonTextPage.enterButtonText("Save me")
        await editContinueButtonTextPage.clickSaveAndContinue();

        await designQuestionnairePage.taskStatusEditButtonTextPage('Completed', 'Edit button text');
    });

    test('Validate customise styling page link status as Completed', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.taskStatusCustomiseStylingPage('Optional', 'Customise styling');

        await designQuestionnairePage.openCustomiseStyling();

        questionnaireStylingPage = await QuestionnaireStylingPage.create(page);
        
        await questionnaireStylingPage.setErrorMessageColor("#FF0000")
        await questionnaireStylingPage.acceptAccessibilityAgreement();
        await questionnaireStylingPage.saveAndContinue();

        await designQuestionnairePage.taskStatusCustomiseStylingPage('Completed', 'Customise styling');
    });

    test('Validate add edit results page link status as Completed', async ({page}) => {
        await signIn(page, token);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.expectEditQuestionnaireHeadingOnPage(PageHeadings.DESIGN_QUESTIONNAIRE_PAGE_HEADING);

        await designQuestionnairePage.taskStatusAddEditResultsPage('Optional', 'Add or edit results pages');

        await designQuestionnairePage.openAddEditResultsPage();

        viewResultsPagesPage = await ViewResultsPagesPage.create(page);
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
        await viewResultsPagesPage.clickAddResultsPage();
        
        addResultsPagePage = await AddResultsPagePage.create(page);
        
        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);
        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);
        await addResultsPagePage.clickSaveAndContinue();
        
        await viewResultsPagesPage.expectResultsPagesHeadingOnPage(PageHeadings.VIEW_RESULTS_PAGES_PAGE_HEADING);
        await viewResultsPagesPage.markFinishedEditing(true);
        await viewResultsPagesPage.saveAndContinue();
        
        await designQuestionnairePage.taskStatusAddEditResultsPage('Completed', 'Add or edit results page');
    });

    test('Add first question should set the questionnaire status to in progress', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        
        await designQuestionnairePage.openAddEditQuestionsAnswers();
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.clickAddQuestion();
        
        addQuestionPage = await AddQuestionPage.create(page);
        
        const questionContent = `Test Question - ${Date.now()}`;
        const hintText = `Test Hint - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(hintText);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('In progress', 'Add or edit questions and answers');
    });

    test('Delete last question should set the questionnaire status to not started', async ({page}) => {
        await signIn(page, token);
        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);

        await designQuestionnairePage.openAddEditQuestionsAnswers();
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.clickAddQuestion();

        addQuestionPage = await AddQuestionPage.create(page);

        const questionContent = `Test Question - ${Date.now()}`;
        const hintText = `Test Hint - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(hintText);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.openAddEditQuestionsAnswers();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.clickFirstEditQuestionLink();
        
        await addQuestionPage.clickDeleteQuestion();

        const deleteConfirmationPage = new DeleteQuestionConfirmationPage(page);
        await deleteConfirmationPage.validateOnPage();
        await deleteConfirmationPage.selectYes();
        await deleteConfirmationPage.clickContinue();

        viewQuestionPage = new ViewQuestionPage(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        designQuestionnairePage = await goToDesignQuestionnairePageByUrl(page, questionnaireId);
        await designQuestionnairePage.taskStatusAddEditQuestionsAnswers('Not started', 'Add or edit questions and answers');
    });
});