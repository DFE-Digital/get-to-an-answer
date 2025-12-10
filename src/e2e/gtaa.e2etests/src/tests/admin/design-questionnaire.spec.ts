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

test.describe('Get to an answer create a new questionnaire', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let answer1: any;
    let answer2: any;

    let designQuestionnairePage: DesignQuestionnairePage;
    let viewQuestionnairePage: ViewQuestionnairePage;
    let addQuestionPage: AddQuestionPage;
    let viewQuestionPage: ViewQuestionPage;
    let addAnswerPage: AddAnswerPage;

    test.beforeEach(async ({page, request}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        // const q1Resp = await createQuestion(request, questionnaireId, token,
        //     undefined, QuestionType.SingleSelect);
        // question1Id = q1Resp.question.id;
        //
        // answer1 = await createSingleAnswer(request, {
        //     questionId: question1Id, questionnaireId, content: 'A1'
        // }, token)
        //
        // answer2 = await createSingleAnswer(request, {
        //     questionnaireId, questionId: question1Id, content: 'A2',
        //     destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        // }, token)
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
        await designQuestionnairePage.validateTaskStatusByTaskName('In progress', 'Add or edit questions and answers');
    });

    test('Validate add or edit questions and answers link Completed status', async ({page}) => {
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
        await designQuestionnairePage.validateTaskStatusByTaskName('Completed', 'Add or edit questions and answers');
    });


});