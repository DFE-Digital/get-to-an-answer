import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {goToUpdateQuestionnairePageByUrl, goToViewQuestionsPageByUrl, signIn} from "../../helpers/admin-test-helper";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, publishQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, deleteQuestion} from "../../test-data-seeder/question-data";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {AnswerDestinationType} from "../../constants/test-data-constants";

test.describe('Get to an answer view questions', () => {
    let token: string;
    let questionnaireId: string;
    let qResp3: any;
    
    let viewQuestionnairePage: ViewQuestionnairePage;
    let editQuestionnairePage: EditQuestionnairePage;
    let viewQuestionPage: ViewQuestionPage;
    let addQuestionPage: AddQuestionPage;

    test.beforeEach(async ({page, request}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const qResp1 = await createQuestion(request, questionnaireId, token);
        const qResp2 = await createQuestion(request, questionnaireId, token);
        qResp3 = await createQuestion(request, questionnaireId, token);
        const qResp4 = await createQuestion(request, questionnaireId, token);

        const externalUrl = 'https://www.gov.uk/government/organisations/department-for-education';

        const {answerPostResponse:a1Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp1.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse:a2Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp2.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse:a3Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp3.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse:a4Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp4.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        expect200HttpStatusCode(qResp1.questionPostResponse, 201);
        expect200HttpStatusCode(qResp2.questionPostResponse, 201);
        expect200HttpStatusCode(qResp3.questionPostResponse, 201);
        expect200HttpStatusCode(qResp4.questionPostResponse, 201);
        expect200HttpStatusCode(a1Res, 201);
        expect200HttpStatusCode(a2Res, 201);
        expect200HttpStatusCode(a3Res, 201);
        expect200HttpStatusCode(a4Res, 201);
    });


    test('Validate presence of elements on view question page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        await viewQuestionPage.assertPageElements();
    });

    test("Header section - H1 and question status", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        await viewQuestionPage.expectQuestionHeadingOnPage();
        await viewQuestionPage.expectQuestionStatusOnPage('Draft');
    });

    // TBS, CARE-1557 bug raised
    // test("Validate questionnaire status as published on view questions page", async ({request, page}) => {
    //     await publishQuestionnaire(request, questionnaireId, token);
    //    
    //     viewQuestionnairePage = await signIn(page, token);
    //     viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
    //    
    //     await viewQuestionPage.expectQuestionHeadingOnPage();
    //     await viewQuestionPage.expectQuestionStatusOnPage('Published');
    // });

    test("Add question CTA navigates to Add question", async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        await viewQuestionPage.clickAddQuestion();

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage();
    });

    test('Back to edit questionnaire link navigation from view questions page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        await viewQuestionPage.ClickBackToEditQuestionnaireLink();
        editQuestionnairePage = await EditQuestionnairePage.create(page);
        expect(editQuestionnairePage.validateHeading());
    });

    // TBC, bug raised CARE-1552
    // test('Show re-order controls for first and last row', async ({page}) => {
    //     viewQuestionnairePage = await signIn(page, token);
    //     viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
    //
    //     // Need to add remaining test script after bug is resolved
    // });

    test('Show re-order controls for middle rows', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        await viewQuestionPage.table.expectReorderControlsOnRowByIndex(2);
    });

    test('Move middle question up', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        const allTextsBefore = await viewQuestionPage.table.allText();
        const secondQuestionBefore = allTextsBefore[1]; // Index 1 = second question
        
        await viewQuestionPage.table.moveUpByIndex(2);
        await viewQuestionPage.waitForPageLoad();
        
        const allTextsAfter = await viewQuestionPage.table.allText();
        const firstQuestionAfter = allTextsAfter[0]; // Index 0 = first question

        // Extract just the question content without the numbering prefix for comparison
        const secondQuestionContent = secondQuestionBefore.replace(/^\d+\.\s/, '');
        const firstQuestionContent = firstQuestionAfter.replace(/^\d+\.\s/, '');

        expect(firstQuestionContent).toBe(secondQuestionContent);
    });

    test('Move middle question down', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);
        
        const allTextsBefore = await viewQuestionPage.table.allText();
        const secondQuestionBefore = allTextsBefore[1]; // Index 1 = second question
        
        await viewQuestionPage.table.moveDownByIndex(2);

        await viewQuestionPage.waitForPageLoad();
        
        const allTextsAfter = await viewQuestionPage.table.allText();
        const thirdQuestionAfter = allTextsAfter[2]; // Index 2 = third question
        
        // Extract just the question content without the numbering prefix for comparison
        const secondQuestionContent = secondQuestionBefore.replace(/^\d+\.\s/, '');
        const thirdQuestionContent = thirdQuestionAfter.replace(/^\d+\.\s/, '');

        expect(thirdQuestionContent).toBe(secondQuestionContent);
    });

    test('Validate correct order after deleting question', async ({page, request}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        // Capture the initial order from questions
        let allTexts = await viewQuestionPage.table.allText();
        const initialOrder = allTexts.map(text => text.replace(/^\d+\.\s/, ''));

        // Delete the third question
        const {deleteQuestionResponse, deleteQuestionBody} = await deleteQuestion(
            request,
            qResp3.question.id,
            token
        );
        
        expect([200, 204]).toContain(deleteQuestionResponse.status());
        
        await page.reload();
        await viewQuestionPage.waitForPageLoad();

        // Capture the order after deletion
        allTexts = await viewQuestionPage.table.allText();
        const finalOrder = allTexts.map(text => text.replace(/^\d+\.\s/, ''));

        // Verify the third question is deleted and fourth has moved up to position 3
        expect(finalOrder).toEqual([initialOrder[0], initialOrder[1], initialOrder[3]]);
        expect(finalOrder).toHaveLength(3);
    });

    test('Performing concurrent question ordering should throw an error', async ({browser, request}) => {
        // Create first browser context
        const context1 = await browser.newContext();
        const page1 = await context1.newPage();

        // Create second browser context
        const context2 = await browser.newContext();
        const page2 = await context2.newPage();

        try {
            await signIn(page1, token);
            await signIn(page2, token);

            // Navigate to view questions page on both pages
            const viewQuestionPage1 = await goToViewQuestionsPageByUrl(page1, questionnaireId);
            const viewQuestionPage2 = await goToViewQuestionsPageByUrl(page2, questionnaireId);

            // Tab 1: Move question 3 up
            await viewQuestionPage1.table.moveUpByIndex(2);
            await viewQuestionPage1.waitForPageLoad();

            // Tab 2: Try to move question 3 up again (concurrent/stale update)
            await viewQuestionPage2.table.moveUpByIndex(2);
            await viewQuestionPage2.waitForPageLoad();

            // Validate error message is displayed on viewQuestionPage2
            await viewQuestionPage2.expectErrorSummaryVisible();

            // Get the error message
            const errorMessage = await viewQuestionPage2.getErrorMessage();

            // Verify the error message indicates a conflict or stale update
            expect(errorMessage).toMatch(/conflict|stale|already|changed|cannot|failed|optimistic/i);

            console.log('✅ Error message validated:', errorMessage);
        } finally {
            // Clean up contexts
            await context1.close();
            await context2.close();
        }
    });
});