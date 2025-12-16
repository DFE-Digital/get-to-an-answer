import {expect, test} from "@playwright/test";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {goToAddAnswerPageByUrl, goToAddQuestionPageByUrl, signIn} from "../../helpers/admin-test-helper";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Get to an answer add an answer to a question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question1Content: string;
    let question2Content: string;
    let question2Id: string;
    let rowData: any;

    let addQuestionPage: AddQuestionPage;
    let addAnswerPage: AddAnswerPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
    });

    test("Validate answers data in answers table for radio type question", async ({page, request}) => {
        const q1Resp = await createQuestion(request, questionnaireId, token);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        const q2Resp = await createQuestion(request, questionnaireId, token);
        question2Id = q2Resp.question.id;
        question2Content = q2Resp.question.content;

        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question2Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        const answerOption1 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(0, answerOption1);
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        const answerOption2 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(1, answerOption2);
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'SpecificQuestion');
        await addAnswerPage.setSpecificQuestion(1, question1Content);
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        await viewQuestionPage.table.clickEditByQuestionContent(question2Content);

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage();

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption1);
        expect(rowData.answer, `❌ First answer option content mismatch: expected "${answerOption1}"`).toBe(answerOption1);
        expect(rowData.destination, `❌ First answer destination mismatch: expected "Next question"`).toBe('Next question');

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption2);
        expect(rowData.answer, `❌ Second answer option content mismatch: expected "${answerOption2}"`).toBe(answerOption2);
        expect(rowData.destination, `❌ Second answer destination mismatch: expected "Specific question"`).toBe('Specific question');
    });

    test("Validate answers data in answers table for dropdown type question", async ({page, request}) => {
        const q1Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.DropdownSelect);
        question1Id = q1Resp.question.id;
        question2Content = q1Resp.question.content;

        const referenceName = 'test-content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: 'Test Content',
            content: 'This is a test content for the start page.',
            referenceName
        }, token)

        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        const answerOption1 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(0, answerOption1);
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'ExternalResultsPage');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');

        const answerOption2 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(1, answerOption2);
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'InternalResultsPage');
        await addAnswerPage.setInternalLink(1, referenceName);
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        await viewQuestionPage.table.clickEditByQuestionContent(question2Content);

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage();

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption1);
        expect(rowData.answer, `❌ First answer option content mismatch: expected "${answerOption1}"`).toBe(answerOption1);
        expect(rowData.destination, `❌ First answer destination mismatch: expected "Link"`).toBe('Link');

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption2);
        expect(rowData.answer, `❌ Second answer option content mismatch: expected "${answerOption2}"`).toBe(answerOption2);
        expect(rowData.destination, `❌ Second answer destination mismatch: expected "Results page"`).toBe('Results page');
    });

    test("Validate answers data in answers table for a multiselect question", async ({page, request}) => {
        const title = 'Test Content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)
        
        await signIn(page, token);
        addQuestionPage = await goToAddQuestionPageByUrl(page, questionnaireId);
        
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        const answerOption1 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(0, answerOption1);
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'ExternalResultsPage');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');
        await addAnswerPage.setAnswerRank(0, 1);

        const answerOption2 = `Answer Option - ${Date.now()}`;
        await addAnswerPage.setOptionContent(1, answerOption2);
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);

        await viewQuestionPage.table.clickEditByQuestionContent(questionContent);

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage();

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption1, true);
        expect(rowData.answer, `❌ First answer option content mismatch: expected "${answerOption1}"`).toBe(answerOption1);
        expect(rowData.priority, `❌ First answer priority mismatch: expected "1"`).toBe('1');
        expect(rowData.destination, `❌ First answer destination mismatch: expected "Link"`).toBe('Link');

        rowData = await addQuestionPage.table.getAnswerRowData(answerOption2, true );
        expect(rowData.answer, `❌ Second answer option content mismatch: expected "${answerOption2}"`).toBe(answerOption2);
        expect(rowData.priority, `❌ Second answer priority mismatch: expected "0"`).toBe('0');
        expect(rowData.destination, `❌ Second answer destination mismatch: expected "Next question"`).toBe('Next question');
    });
});