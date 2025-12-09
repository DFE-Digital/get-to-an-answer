import {expect, test} from "@playwright/test";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    goToUpdateAnswerPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {AnswerDestinationType, QuestionType} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Update answer for multiselect question with priority', () => {
    let token: string;
    let questionnaireId: string;
    let questionId: string;
    let question2Id: string;
    let question2Content: string;
    let contentId: string;
    let contentTitle: string;
    let answerWithNextQuestion: any;
    let answerWithExternalLink: any;

    let addAnswerPage: AddAnswerPage;
    let addQuestionPage: AddQuestionPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const qResp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.MultiSelect);
        questionId = qResp.question.id;

        const q2Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.MultiSelect);
        question2Id = q2Resp.question.id;
        question2Content = q2Resp.question.content;

        // Create answers with different priorities and destinations
        answerWithNextQuestion = await createSingleAnswer(request, {
            questionId, questionnaireId, content: 'Answer with NextQuestion', priority: 1
        }, token);

        answerWithExternalLink = await createSingleAnswer(request, {
            questionId, questionnaireId, content: 'Answer with ExternalLink',
            priority: 2, destinationType: AnswerDestinationType.ExternalLink,
            destinationUrl: 'https://example.com'
        }, token);

        // Create content for internal results page destination
        contentTitle = `test-content-${Date.now()}`;
        const contentResp = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is test content for results page.',
            referenceName: `test-content-${Date.now()}`
        }, token);
        contentId = contentResp.content.id;
    });
    
    test('Update answer priority from 1 to 3', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update first answer priority
        await addAnswerPage.clearAnswerRank(0);
        await addAnswerPage.setAnswerRank(0, 3);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer with NextQuestion', true);
        expect(rowData.priority, '❌ Priority should be updated to "3"').toBe('3');
    });

    test('Update answer priority and maintain destination', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update second answer priority (has external link destination)
        await addAnswerPage.clearAnswerRank(1);
        await addAnswerPage.setAnswerRank(1, 5);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify priority and destination are both maintained
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer with ExternalLink', true);
        expect(rowData.priority, '❌ Priority should be "5"').toBe('5');
        expect(rowData.destination, '❌ Destination should remain "Link"').toBe('Link');
    });

    // ===== Destination Update Tests with Priority =====
    test('Update answer destination from NextQuestion to SpecificQuestion while keeping priority', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update destination to specific question
        await addAnswerPage.setSpecificQuestion(0, question2Content);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify destination updated and priority preserved
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer with NextQuestion', true);
        expect(rowData.destination, '❌ Destination should be "Specific question"').toBe('Specific question');
        expect(rowData.priority, '❌ Priority should remain "1"').toBe('1');
    });

    test('Update answer destination from ExternalLink to InternalResultsPage', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.setInternalLink(1, contentTitle);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify destination updated
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer with ExternalLink', true);
        expect(rowData.destination, '❌ Destination should be "Results page"').toBe('Results page');
        expect(rowData.priority, '❌ Priority should remain "2"').toBe('2');
    });

    // ===== Error Summary Tests =====
    test('Error summary displays when clearing required answer content on update', async ({browserName, page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Clear first answer content
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.clearOptionContent(1);
        await addAnswerPage.clickSaveAnswersButton();

        await addAnswerPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });
    
    // ===== Inline Error Tests =====
    test('Inline error displays for invalid answer content on update', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Clear first answer content
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify inline error is visible
        await addAnswerPage.validateInlineQuestionContentError(0);
    });
    
    test('Multiple inline errors display for multiple empty answer contents', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, questionId);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Clear all answer contents
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.clearOptionContent(1);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify inline errors for both
        await addAnswerPage.validateInlineQuestionContentError(0);
        await addAnswerPage.validateInlineQuestionContentError(1);
    });
});