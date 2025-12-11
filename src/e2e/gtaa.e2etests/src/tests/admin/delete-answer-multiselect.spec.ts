import {expect, test} from "@playwright/test";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AnswerDestinationType, QuestionType} from "../../constants/test-data-constants";
import {
    goToUpdateAnswerPageByUrl, goToUpdateQuestionPageByUrl,
    goToViewQuestionsPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";

test.describe('Get to an answer update questionnaire', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question1Content: string;
    let rowData: any;
    let answer1: any;
    let answer2: any;

    let addQuestionPage: AddQuestionPage;
    let addAnswerPage: AddAnswerPage;
    let viewQuestionPage: ViewQuestionPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.MultiSelect);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        answer1 = await createSingleAnswer(request, {
            questionId: question1Id, questionnaireId, content: 'A1'
        }, token)

        answer2 = await createSingleAnswer(request, {
            questionnaireId, questionId: question1Id, content: 'A2',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
    });

    test('Delete an answer successfully - multiselect question', async ({page}) => {
        await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        await viewQuestionPage.expectQuestionHeadingOnPage();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.table.clickEditByQuestionContent(question1Content);

        addQuestionPage = await AddQuestionPage.create(page);

        // Get all answer contents before deletion
        const initialAnswers = await addQuestionPage.table.getAllAnswerContents();
        expect(initialAnswers.length).toBeGreaterThan(1);

        // Choose an answer to delete (second answer)
        const answerToDelete = initialAnswers[1];

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Remove the chosen answer
        await addAnswerPage.removeOption(1);
        await addAnswerPage.clickSaveAnswersButton();

        // Reload page and verify
        await page.reload();
        addQuestionPage = await AddQuestionPage.create(page);

        // Get remaining answers
        const remainingAnswers = await addQuestionPage.table.getAllAnswerContents();
        
        expect(remainingAnswers.length).toBe(initialAnswers.length - 1);
        expect(remainingAnswers).not.toContain(answerToDelete);
        expect(remainingAnswers).toContain(initialAnswers[0]);
    });

    test('Delete an answer successfully for question with 3 answers - multiselect', async ({page, request}) => {
        // Create a third answer
        const {answer} = await createSingleAnswer(request, {
            questionnaireId, questionId: question1Id, content: 'A3',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)

        await signIn(page, token);

        // Navigate to update a question page
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);

        // Get all answer contents before deletion
        const initialAnswers = await addQuestionPage.table.getAllAnswerContents();
        expect(initialAnswers.length).toBe(3);

        // Verify that a specific answer exists
        expect(initialAnswers).toContain(answer.content);

        // Navigate to update an answer page
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Remove the third answer
        await addAnswerPage.removeOption(2);
        await addAnswerPage.clickSaveAnswersButton();

        // Reload page and verify
        await page.reload();
        addQuestionPage = await AddQuestionPage.create(page);

        // Get remaining answers
        const remainingAnswers = await addQuestionPage.table.getAllAnswerContents();
        
        expect(remainingAnswers.length).toBe(2);
        expect(remainingAnswers).not.toContain(answer.content);
    });
});