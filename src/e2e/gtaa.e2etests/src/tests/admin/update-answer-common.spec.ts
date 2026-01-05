import {expect, test} from "@playwright/test";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    goToAddAnswerPageByUrl,
    goToAddResultPagePageByUrl,
    goToUpdateAnswerPageByUrl,
    goToUpdateQuestionPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {AnswerDestinationType, PageHeadings, QuestionType} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {createSingleAnswer} from "../../test-data-seeder/answer-data";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddResultsPagePage} from "../../pages/admin/AddResultsPagePage";

test.describe('Update answers to a question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let answer1: any;
    let answer2: any;

    let addQuestionPage: AddQuestionPage;
    let addAnswerPage: AddAnswerPage;
    let addResultsPagePage: AddResultsPagePage;
    
    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.SingleSelect);
        question1Id = q1Resp.question.id;

        answer1 = await createSingleAnswer(request, {
            questionId: question1Id, questionnaireId, content: 'A1'
        }, token)

        answer2 = await createSingleAnswer(request, {
            questionnaireId, questionId: question1Id, content: 'A2',
            destinationType: AnswerDestinationType.ExternalLink, destinationUrl: 'https://example.com'
        }, token)
    });

    test('Click edit takes to the edit answer page', async ({page}) => {
        await signIn(page, token);
        
        addQuestionPage = await goToUpdateQuestionPageByUrl(page, questionnaireId, question1Id);
        await addQuestionPage.expectAddQuestionHeadingOnPage();

        await addQuestionPage.table.openEdit();
        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();
    });
    
    test('Validate presence of elements on update answer page upon landing', async ({page}) => {
        await signIn(page, token);
        
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.asserPageElementsUponLanding(0, 2);
        await addAnswerPage.asserPageElementsUponLanding(1, 2);
    });

    test("Header section - H1 heading on answer update", async ({page}) => {
        await signIn(page, token);
        
        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
    });

    test('Clicking the back link takes to question page', async ({page}) => {
        await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.clickBackLInk();
        const addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.EDIT_QUESTION_PAGE_HEADING);
    });

    // TBC, CARE-1587 bug raised.
    test('Remove button should remove answers option on update page', async ({ page }) => {
        let removeButtonsCount: number;
        await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        // Verify initial state has 2 remove buttons
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(2);

        // Remove one answer option
        await addAnswerPage.removeOption(1);
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(1);
    });

    test("Validate presence of elements on add another answer form", async ({page}) => {
        await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        // Add another option
        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponLanding(0, 3);
    });

    test('Validate reference name is selected when answer destination is internal results page - edit answer', async ({page}) => {
        await signIn(page, token);

        // Add a new results page
        addResultsPagePage = await goToAddResultPagePageByUrl(page, questionnaireId);
        await addResultsPagePage.expectAddResultsPageHeadingOnPage(PageHeadings.ADD_RESULTS_PAGE_PAGE_HEADING);

        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);
        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);
        const resultsPageRefNameInput = `Test ref name - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageRefNameInput(resultsPageRefNameInput);
        await addResultsPagePage.clickSaveAndContinue();

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        // Update answers destination
        await addAnswerPage.setInternalLink(0, resultsPageRefNameInput);
        await addAnswerPage.setInternalLink(1, resultsPageRefNameInput);
        await addAnswerPage.clickSaveAnswersButton();
        
        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.EDIT_QUESTION_PAGE_HEADING);
    })

    test('Validate results page title is selected when answer destination is internal results page - edit answer', async ({page}) => {
        await signIn(page, token);

        // Add a new results page
        addResultsPagePage = await goToAddResultPagePageByUrl(page, questionnaireId);
        await addResultsPagePage.expectAddResultsPageHeadingOnPage(PageHeadings.ADD_RESULTS_PAGE_PAGE_HEADING);

        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);
        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);
        await addResultsPagePage.clickSaveAndContinue();

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);

        // Update answers destination
        await addAnswerPage.setInternalLink(0, resultsPageTitleInput);
        await addAnswerPage.setInternalLink(1, resultsPageTitleInput);
        await addAnswerPage.clickSaveAnswersButton();

        addQuestionPage = await AddQuestionPage.create(page);
        await addQuestionPage.expectAddQuestionHeadingOnPage(PageHeadings.EDIT_QUESTION_PAGE_HEADING);
    })
    
    test('Accessible ids and aria-describedby for multiple options with hint', async ({page}) => {
        await signIn(page, token);

        addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.validateUniqueIdsForMultipleOptions(2);
        
        // Verify aria-describedby includes hint ids (no errors present)
        await addAnswerPage.validateAriaDescribedByForHintOnly(0);
        await addAnswerPage.validateAriaDescribedByForHintOnly(1);
    })

    // TBC, CARE-1579 bug raised
    // test('Accessible ids and aria-describedby for multiple options with error', async ({page}) => {
    //     await signIn(page, token);
    //
    //     addAnswerPage = await goToUpdateAnswerPageByUrl(page, questionnaireId, question1Id);
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //
    //     await addAnswerPage.clickSaveAnswersButton();
    //     await addAnswerPage.validateUniqueIdsForMultipleOptions(2);
    //
    //     // Verify aria-describedby includes both hint id and error id when error is present
    //     await addAnswerPage.validateAriaDescribedByWithError(0);
    //     await addAnswerPage.validateAriaDescribedByWithError(1);
    // })
});