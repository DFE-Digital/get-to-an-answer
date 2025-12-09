import {expect, test} from "@playwright/test";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    goToUpdateAnswerPageByUrl,
    goToUpdateQuestionPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {AnswerDestinationType, QuestionType} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Update answer for single select question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question2Id: string;
    let question1Content: string;
    let question2Content: string;
    let answerWithNextQuestion: any;
    let answerWithExternalLink: any;
    let contentTitle: string;

    let addQuestionPage: AddQuestionPage;
    let addAnswerPage: AddAnswerPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.SingleSelect);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        const q2Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.SingleSelect);
        question2Id = q2Resp.question.id;
        question2Content = q2Resp.question.content;

        answerWithNextQuestion = await createSingleAnswer(request, {
            questionId: question1Id, questionnaireId, content: 'Answer A'
        }, token);

        answerWithExternalLink = await createSingleAnswer(request, {
            questionnaireId, questionId: question1Id, content: 'Answer B',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token);

        contentTitle = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: contentTitle,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
    });

    test('Update answer destination from NextQuestion to SpecificQuestion', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update first answer destination to SpecificQuestion
        await addAnswerPage.setSpecificQuestion(0, question2Content);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer A');
        expect(rowData.destination, '❌ Destination should be "Specific question"').toBe('Specific question');
    });

    test('Update answer destination from ExternalLink to SpecificQuestion', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update second answer from external-link to specific question
        await addAnswerPage.setSpecificQuestion(1, question2Content);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer B');
        expect(rowData.destination, '❌ Destination should be "Specific question"').toBe('Specific question');
    });

    test('Update answer destination from NextQuestion to InternalResultsPage', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Update first answer destination to internal results page
        await addAnswerPage.setInternalLink(0, contentTitle);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer A');
        expect(rowData.destination, '❌ Destination should be "Results page"').toBe('Results page');
    });

    test('Update answer destination from SpecificQuestion to ExternalLink', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // First update to SpecificQuestion
        await addAnswerPage.setSpecificQuestion(0, question2Content);
        await addAnswerPage.clickSaveAnswersButton();

        // Re-open an update page
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        // Update to ExternalLink
        const externalUrl = 'https://updated-example.com';
        await addAnswerPage.setExternalLink(0, externalUrl);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData('Answer A');
        expect(rowData.destination, '❌ Destination should be "Link"').toBe('Link');
    });

    // ===== Answer Content Update Tests =====
    test('Update answer content and hint while preserving destination', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        const updatedContent = `Updated Answer - ${Date.now()}`;
        const updatedHint = 'Updated hint text';

        // Update content and hint
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.setOptionContent(0, updatedContent);
        await addAnswerPage.clearOptionHint(0);
        await addAnswerPage.setOptionHint(0, updatedHint);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify update in question table
        addQuestionPage = await AddQuestionPage.create(page);
        const rowData = await addQuestionPage.table.getAnswerRowData(updatedContent);
        expect(rowData.answer, '❌ Answer content should be updated').toBe(updatedContent);
        expect(rowData.destination, '❌ Destination should remain "Next question"').toBe('Next question');
    });

    // ===== Error Summary Tests =====
    test('Error summary displays when clearing required answer content on update', async ({browserName, page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Clear answer content
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.clearOptionContent(1);
        await addAnswerPage.clickSaveAnswersButton();

        await addAnswerPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    });

    // ===== Inline Error Tests =====
    test('Inline error displays for invalid answer content on update', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Clear first answer content
        await addAnswerPage.clearOptionContent(0);
        await addAnswerPage.clearOptionContent(1);
        await addAnswerPage.clickSaveAnswersButton();

        // Verify inline error is visible
        await addAnswerPage.validateInlineQuestionContentError(0);
    });
});