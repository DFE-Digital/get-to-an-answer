import {expect, test} from "@playwright/test";
import {AddQuestionPage, QuestionType} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswersPage} from "../../pages/admin/AddAnswersPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {
    signIn, goToEditQuestionnairePageByUrl, goToAddQuestionPageByUrl, goToAddQuestionnairePage
} from '../../helpers/admin-test-helper';
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {PageHeadings} from "../../constants/test-data-constants";

test.describe('Get to an answer add question to questionnaire', () => {
    let token: string;
    let questionnaire: any;
    let addQuestionPage: AddQuestionPage;
    let viewQuestionPage: ViewQuestionPage;
    let addAnswersPage: AddAnswersPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);

        await signIn(page, token);
        addQuestionPage = await goToAddQuestionPageByUrl(page, questionnaire.id);
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.ADD_QUESTION_PAGE_HEADING);
    });

    test('Validate presence of elements on add new question page', async ({page}) => {
        await addQuestionPage.assertPageElements();
    });

    // TBC, expected page should be view questions (1548 bug raised)
    // test('Back link takes to view question list from Add question page', async ({page}) => {
    //     await addQuestionPage.verifyBackLinkPresent();
    //     await addQuestionPage.clickBackLink();
    //
    //     viewQuestionPage = new ViewQuestionPage(page);
    //     await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    // });

    test('Error summary appears on invalid submit with all missing required fields', async ({browserName}) => {
        await addQuestionPage.clickSaveAndContinue();
        await addQuestionPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing question content', async ({browserName}) => {
        const questionHintText = `Test hint text - ${Date.now()}`;
        await addQuestionPage.enterQuestionHintText(questionHintText);
        await addQuestionPage.chooseQuestionType(QuestionType.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();
        await addQuestionPage.validateMissingQuestionContentErrorMessageSummary(browserName);
    });

    test('Error summary appears on submit with missing question type', async ({browserName}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        const questionHintText = `Test hint text - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(questionHintText);
        await addQuestionPage.clickSaveAndContinue();
        await addQuestionPage.validateMissingQuestionTypeErrorMessageSummary(browserName);
    });

    test('Successful submit with missing optional question hint text', async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionType.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();

        addAnswersPage = await AddAnswersPage.create(page);
        await addAnswersPage.expectAnswerHeadingOnPage();
    });

    test('Inline error and styling for missing question content', async ({page}) => {
        await addQuestionPage.clearQuestionContent();
        await addQuestionPage.chooseQuestionType(QuestionType.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();

        await addQuestionPage.validateInlineQuestionContentError();
    });

    test('Inline error and styling for missing question type', async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        const questionHintText = `Test hint text - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(questionHintText);
        await addQuestionPage.clickSaveAndContinue();
        await addQuestionPage.validateInlineQuestionTypeError();
    });

    test('Create question with dropdown type', async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        const hintText = `Test Hint - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(hintText);
        await addQuestionPage.chooseQuestionType(QuestionType.SingleSelectLong);
        await addQuestionPage.clickSaveAndContinue();

        addAnswersPage = await AddAnswersPage.create(page);
        await addAnswersPage.expectAnswerHeadingOnPage();
    });

    test('Create question with multi-select type', async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        const hintText = `Test Hint - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.enterQuestionHintText(hintText);
        await addQuestionPage.chooseQuestionType(QuestionType.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswersPage = await AddAnswersPage.create(page);
        await addAnswersPage.expectAnswerHeadingOnPage();
    });

    // TBC, bug raised CARE-1549
    // test('Create a new question with invalid content to validate aria-describedby', async ({page}) => {
    //     await addQuestionPage.enterInvalidContent();
    //     await addQuestionPage.chooseQuestionType(QuestionType.SingleSelectShort);
    //
    //     await addQuestionPage.clickSaveAndContinue();
    //     await addQuestionPage.validateQuestionContentFieldAriaDescribedBy();
    // });

    // TBC, CARE-1549 bug raised
    // test('Create a new question with missing question type to validate aria-describedby', async ({page}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //
    //     await addQuestionPage.clickSaveAndContinue();
    //     await addQuestionPage.validateQuestionTypeErrorAriaDescribedBy();
    // });
});