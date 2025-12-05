import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {UpdateQuestionnaireSlugPage} from "../../pages/admin/UpdateQuestionnaireSlugPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {goToUpdateQuestionPageByUrl, signIn} from "../../helpers/admin-test-helper";
import {PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {createQuestion, getQuestion} from "../../test-data-seeder/question-data";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {DeleteQuestionConfirmationPage} from "../../pages/admin/DeleteQuestionConfirmationPage";

test.describe('Get to an answer update question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;

    let viewQuestionnairePage: ViewQuestionnairePage;
    let viewQuestionPage: ViewQuestionPage;
    let addQuestionPage: AddQuestionPage;
    let editQuestionnairePage: EditQuestionnairePage;
    let updateQuestionnaireSlugPage: UpdateQuestionnaireSlugPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token);
        question1Id = q1Resp.question.id;
    });

    test('Edit question page displays all required elements', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        await addQuestionPage.assertPageElements();
    });

    test('Back link takes to view question list from Edit question page', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        
        await addQuestionPage.verifyBackLinkPresent();
        await addQuestionPage.clickBackLink();

        viewQuestionPage = new ViewQuestionPage(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test('Save question updates question data successfully', async ({request, page}) => {
        const {questionGetBody: originalQuestion} = await getQuestion(request, question1Id, token);

        // Verify original data exists
        expect(originalQuestion.content).toBeDefined();
        expect(originalQuestion.type).toBeDefined();

        // Store original values
        const originalContent = originalQuestion.content;
        const originalType = originalQuestion.type;
        const originalDescription = originalQuestion.description;

        // Navigate to edit page
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        // Update question data
        const updatedContent = `Updated Question - ${Date.now()}`;
        const updatedHintText = `Updated hint text - ${Date.now()}`;

        await addQuestionPage.enterQuestionContent(updatedContent);
        await addQuestionPage.enterQuestionHintText(updatedHintText);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectLong);
        await addQuestionPage.clickSaveQuestion();

        // Verify the update via API
        const {questionGetBody: updatedQuestion} = await getQuestion(request, question1Id, token);

        // Assert original data was different from updated
        expect(updatedQuestion.content).not.toBe(originalContent);
        expect(updatedQuestion.description).not.toBe(originalDescription);
        expect(updatedQuestion.type).not.toBe(originalType);

        // Assert updated data matches new values
        expect(updatedQuestion.content).toBe(updatedContent);
        expect(updatedQuestion.description).toBe(updatedHintText);
        expect(updatedQuestion.type).toBe(QuestionType.DropdownSelect);

        await addQuestionPage.validateSuccessBanner();
    });
    
    test('Error summary on invalid submit with missing required fields when updating question', async ({page, browserName}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        await addQuestionPage.clearQuestionContent();
        await addQuestionPage.clickSaveQuestion();
        await addQuestionPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });

    test('Successful submit with missing optional question hint text when updating question', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.clearQuestionHintText();
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectShort);
        await addQuestionPage.clickSaveQuestion();

        await addQuestionPage.validateSuccessBanner();
    });

    test('Successful submit updated question with success banner', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectShort);
        await addQuestionPage.clickSaveQuestion();

        await addQuestionPage.validateSuccessBanner();

        await addQuestionPage.clickAddQuestionInSuccessBanner();
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.ADD_QUESTION_PAGE_HEADING);

        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        await addQuestionPage.clickBackToYourQuestionsInSuccessBanner();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test('Inline error and styling for missing question content', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        await addQuestionPage.clearQuestionContent();
        await addQuestionPage.clickSaveQuestion();

        await addQuestionPage.validateInlineQuestionContentError();
    });
    
    test('Update an existing question with invalid content to validate aria-describedby', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        await addQuestionPage.enterInvalidContent();

        await addQuestionPage.clickSaveQuestion();
        await addQuestionPage.validateQuestionContentFieldAriaDescribedBy();
    });

    test('Delete a question successfully', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        await addQuestionPage.clickDeleteQuestion();

        const deleteConfirmationPage = new DeleteQuestionConfirmationPage(page);
        await deleteConfirmationPage.validateOnPage();
        await deleteConfirmationPage.selectYes();
        await deleteConfirmationPage.clickContinue();

        viewQuestionPage = new ViewQuestionPage(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test('Delete a question with cancellation returns to edit page', async ({page}) => {
        await signIn(page, token);
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        await addQuestionPage.clickDeleteQuestion();

        const deleteConfirmationPage = new DeleteQuestionConfirmationPage(page);
        await deleteConfirmationPage.validateOnPage();
        await deleteConfirmationPage.selectNo();
        await deleteConfirmationPage.clickContinue();

        addQuestionPage = new AddQuestionPage(page, 'update');
        await addQuestionPage.expectAddQuestionHeadingOnPage();
    });
});