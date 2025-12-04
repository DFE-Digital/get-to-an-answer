import {expect, test} from "@playwright/test";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {
    goToEditQuestionnairePageByUrl,
    goToUpdateQuestionnairePageByUrl,
    goToViewQuestionsPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire, publishQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion, deleteQuestion} from "../../test-data-seeder/question-data";
import {expect200HttpStatusCode} from "../../helpers/api-assertions-helper";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {AnswerDestinationType, ErrorMessages, PageHeadings} from "../../constants/test-data-constants";

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

        const {answerPostResponse: a1Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp1.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse: a2Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp2.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse: a3Res} = await createSingleAnswer(
            request,
            {
                questionId: qResp3.question.id,
                questionnaireId,
                content: 'Answer leading to external link',
                destinationType: AnswerDestinationType.ExternalLink,
                destinationUrl: externalUrl,
            }, token
        );

        const {answerPostResponse: a4Res} = await createSingleAnswer(
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
        await viewQuestionPage.expectQuestionnaireStatusOnPage('Draft');
    });
    
    test("Validate questionnaire status as published on view questions page", async ({request, page}) => {
        await publishQuestionnaire(request, questionnaireId, token);

        viewQuestionnairePage = await signIn(page, token);

        editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireId);
        await editQuestionnairePage.openAddEditQuestionsAnswers();

        viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage();
        await viewQuestionPage.expectQuestionnaireStatusOnPage('Published');
    });

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
        expect(editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING));
    });

    test('Save and continue with No I will come back later radio navigates to Edit questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.markFinishedEditing(false);
        await viewQuestionPage.expectComeBackLaterRadioIsSelected();

        await viewQuestionPage.saveAndContinue();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('Save and continue with Yes radio navigates to Edit questionnaire page', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.expectYesRadioIsNotSelected();
        await viewQuestionPage.markFinishedEditing(true);

        await viewQuestionPage.saveAndContinue();

        editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test('List existing questions', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.expectQuestionHeadingOnPage();

        await viewQuestionPage.table.verifyListExistsWithOrderNumbersContentAndActions();
    });

    test('No "Move up" on first item', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.expectQuestionHeadingOnPage();

        // Then I do not see a "Move up" link for it
        await viewQuestionPage.table.verifyMoveUpLinkNotVisibleForFirstRow();
    })

    test('No "Move down" on last item', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.expectQuestionHeadingOnPage();

        // Then I do not see a "Move down" link for it
        await viewQuestionPage.table.verifyMoveDownLinkNotVisibleForLastRow();
    });

    test('Show re-order controls for middle rows', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        await viewQuestionPage.table.expectReorderControlsOnRowByIndex(2);
    });

    test('Move middle question up', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        const allTextsBefore = await viewQuestionPage.table.allQuestionContent();
        const secondQuestionBefore = allTextsBefore[1]; // Index 1 = second question

        await viewQuestionPage.table.moveUpByIndex(2);
        await viewQuestionPage.waitForPageLoad();

        const allTextsAfter = await viewQuestionPage.table.allQuestionContent();
        const firstQuestionAfter = allTextsAfter[0]; // Index 0 = first question

        // Extract just the question content without the numbering prefix for comparison
        const secondQuestionContent = secondQuestionBefore.replace(/^\d+\.\s/, '');
        const firstQuestionContent = firstQuestionAfter.replace(/^\d+\.\s/, '');

        expect(firstQuestionContent).toBe(secondQuestionContent);
    });

    test('Move middle question down', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        const allTextsBefore = await viewQuestionPage.table.allQuestionContent();
        const secondQuestionBefore = allTextsBefore[1]; // Index 1 = second question

        await viewQuestionPage.table.moveDownByIndex(2);

        await viewQuestionPage.waitForPageLoad();

        const allTextsAfter = await viewQuestionPage.table.allQuestionContent();
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
        let allTexts = await viewQuestionPage.table.allQuestionContent();
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
        allTexts = await viewQuestionPage.table.allQuestionContent();
        const finalOrder = allTexts.map(text => text.replace(/^\d+\.\s/, ''));

        // Verify the third question is deleted and fourth has moved up to position 3
        expect(finalOrder).toEqual([initialOrder[0], initialOrder[1], initialOrder[3]]);
        expect(finalOrder).toHaveLength(3);
    });

    // TBC, bug raised CARE-1565, CARE-1575
    test('Performing concurrent move up question ordering should throw an error', async ({browser, request}) => {
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

            // Tab 1: Move question 2 up
            await viewQuestionPage1.table.moveUpByIndex(2);
            await viewQuestionPage1.waitForPageLoad();

            // Tab 2: Try to move question 2 up again (concurrent/stale update)
            await viewQuestionPage2.table.moveUpByIndex(2);
            await viewQuestionPage2.waitForPageLoad();

            // Validate error message is displayed on viewQuestionPage2
            await viewQuestionPage2.expectErrorSummaryVisible();

            // Verify the error message indicates a conflict or stale update
            await viewQuestionPage2.validateMoveUpErrorMessageContains();
            const expectedMessage = await viewQuestionPage2.getMatchingErrorMessages(ErrorMessages.ERROR_MESSAGE_TOP_QUESTION_UP)
            expect(expectedMessage).toHaveLength(1);

        } finally {
            await context1.close();
            await context2.close();
        }
    });

    // TBC, bug raised CARE-1565, CARE-1575
    test('Performing concurrent move down question ordering should throw an error', async ({browser, request}) => {
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

            // Tab 1: Move question 2 down
            await viewQuestionPage1.table.moveDownByIndex(3);
            await viewQuestionPage1.waitForPageLoad();

            // Tab 2: Try to move question 2 down again (concurrent/stale update)
            await viewQuestionPage2.table.moveDownByIndex(3);
            await viewQuestionPage2.waitForPageLoad();

            // Validate error message is displayed on viewQuestionPage2
            await viewQuestionPage2.expectErrorSummaryVisible();

            // Verify the error message indicates a conflict or stale update
            await viewQuestionPage2.validateMoveDownErrorMessageContains();
            const expectedMessage = await viewQuestionPage2.getMatchingErrorMessages(ErrorMessages.ERROR_MESSAGE_BOTTOM_QUESTION_DOWN)
            expect(expectedMessage).toHaveLength(1);

        } finally {
            await context1.close();
            await context2.close();
        }
    });

    test('Reorder behavior', async ({page}) => {
        viewQuestionnairePage = await signIn(page, token);
        viewQuestionPage = await goToViewQuestionsPageByUrl(page, questionnaireId);

        // Capture the initial order - get the actual question content
        let allContent = await viewQuestionPage.table.allQuestionContent();
        const initialFirstQuestion = allContent[0];
        const initialSecondQuestion = allContent[1];

        // When I click "Move up" on the second question
        await viewQuestionPage.table.moveUpByIndex(2);
        await page.waitForLoadState('networkidle');
        await viewQuestionPage.waitForPageLoad();

        // Then the question's order is updated
        let allContentAfterMoveUp = await viewQuestionPage.table.allQuestionContent();
        const firstQuestionAfterMoveUp = allContentAfterMoveUp[0];
        const secondQuestionAfterMoveUp = allContentAfterMoveUp[1];

        expect(firstQuestionAfterMoveUp).toBe(initialSecondQuestion);
        expect(secondQuestionAfterMoveUp).toBe(initialFirstQuestion);

        // And the list reflects the new order when the page reloads
        await page.reload();
        await page.waitForLoadState('networkidle');
        await viewQuestionPage.waitForPageLoad();

        let allContentAfterReload = await viewQuestionPage.table.allQuestionContent();
        const firstQuestionAfterReload = allContentAfterReload[0];
        const secondQuestionAfterReload = allContentAfterReload[1];

        expect(firstQuestionAfterReload).toBe(initialSecondQuestion);
        expect(secondQuestionAfterReload).toBe(initialFirstQuestion);
    });
});