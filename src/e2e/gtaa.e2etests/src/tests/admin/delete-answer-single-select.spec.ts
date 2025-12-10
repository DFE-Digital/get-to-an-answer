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
            undefined, QuestionType.SingleSelect);
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
    
    test('Delete an answer successfully - single select question', async ({page}) => {
        await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        await viewQuestionPage.expectQuestionHeadingOnPage();
        
        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.table.clickEditByQuestionContent(question1Content);

        addQuestionPage = await AddQuestionPage.create(page);
        rowData = await addQuestionPage.table.getAnswerRowData(answer1.content);
        expect(rowData).toBeDefined();
        
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        //delete answer and validate it's deleted in the table

    });

    test('Delete an answer successfully for question with 3 answers - single select', async ({page, request}) => {
        const {answer} = await createSingleAnswer(request, {
            questionnaireId, questionId: question1Id, content: 'A3',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
        
        await signIn(page, token);
        
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        rowData = await addQuestionPage.table.getAnswerRowData(answer.content);
        expect(rowData).toBeDefined();
        
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        //delete an answer and validate it's deleted in the table

    });
});