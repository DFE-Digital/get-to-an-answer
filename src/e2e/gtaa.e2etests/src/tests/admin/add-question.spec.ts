import {expect, test} from "@playwright/test";
import {AddQuestionPage, QuestionType} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswersPage} from "../../pages/admin/AddAnswersPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {signIn,goToEditQuestionnairePageByUrl,goToAddQuestionPageByUrl, goToAddQuestionnairePage
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
        await addQuestionPage.expectQuestionHeadingOnPage(PageHeadings.ADD_QUESTION_PAGE_HEADING);
    });

    // test('Validate presence of elements on add new question page', async ({page}) => {
    //     await addQuestionPage.assertPageElements();
    // });

    // TBC, expected page should be view questions
    // test('Back link takes to view question list from Add question page', async ({page}) => {
    //     await addQuestionPage.verifyBackLinkPresent();
    //     await addQuestionPage.clickBackLink();
    //
    //     viewQuestionPage = new ViewQuestionPage(page);
    //     await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    // });

    // TBC, it should not validate question text hint
    // test('Error summary appears on invalid submit with all missing required fields', async ({browserName}) => {
    //     await addQuestionPage.save();
    //     await addQuestionPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    // });

    // test('Error summary appears on submit with missing question content', async ({browserName}) => {
    //     const questionHintText = `Test hint text - ${Date.now()}`;
    //     await addQuestionPage.enterQuestionHintText(questionHintText);
    //     await addQuestionPage.chooseType(QuestionType.SingleSelectShort);
    //     await addQuestionPage.save();
    //     await addQuestionPage.validateMissingQuestionContentErrorMessageSummary(browserName);
    // });
    //
    // test('Error summary appears on submit with missing question type', async ({browserName}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     const questionHintText = `Test hint text - ${Date.now()}`;
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //     await addQuestionPage.enterQuestionHintText(questionHintText);
    //     await addQuestionPage.save();
    //     await addQuestionPage.validateMissingQuestionTypeErrorMessageSummary(browserName);
    // });

    // TBC, do we need a success banner and redirection to view question upon success
    // test('Successful submit with missing optional question hint text', async ({page}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //     await addQuestionPage.chooseType(QuestionType.SingleSelectShort);
    //     await addQuestionPage.save();
    //
    //     // TBC, do we need a success banner here?
    //
    //     addAnswersPage = await AddAnswersPage.create(page);
    //     await addAnswersPage.expectAnswerHeadingOnPage();
    // });

    // test('Inline error and styling for missing question content', async ({page}) => {
    //     await addQuestionPage.clearQuestionContent();
    //     await addQuestionPage.chooseType(QuestionType.SingleSelectShort);
    //     await addQuestionPage.save();
    //
    //     await addQuestionPage.validateInlineQuestionContentError();
    // });

    // test('Inline error and styling for missing question type', async ({page}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     const questionHintText = `Test hint text - ${Date.now()}`;
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //     await addQuestionPage.enterQuestionHintText(questionHintText);
    //     await addQuestionPage.save();
    //     await addQuestionPage.validateInlineQuestionTypeError();
    // });

    // test('Create question with dropdown type', async ({page}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     const hintText = `Test Hint - ${Date.now()}`;
    //
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //     await addQuestionPage.enterQuestionHintText(hintText);
    //     await addQuestionPage.chooseType(QuestionType.SingleSelectLong);
    //     await addQuestionPage.save();
    //
    //     addAnswersPage = await AddAnswersPage.create(page);
    //     await addAnswersPage.expectAnswerHeadingOnPage();
    // });

    // test('Create question with multi-select type', async ({page}) => {
    //     const questionContent = `Test Question - ${Date.now()}`;
    //     const hintText = `Test Hint - ${Date.now()}`;
    //
    //     await addQuestionPage.enterQuestionContent(questionContent);
    //     await addQuestionPage.enterQuestionHintText(hintText);
    //     await addQuestionPage.chooseType(QuestionType.MultiSelect);
    //     await addQuestionPage.save();
    //
    //     addAnswersPage = await AddAnswersPage.create(page);
    //     await addAnswersPage.expectAnswerHeadingOnPage();
    // });
});