import {expect, test} from "@playwright/test";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage, AnswerFieldName} from "../../pages/admin/AddAnswerPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {goToAddAnswerPageByUrl, goToAddResultPagePageByUrl, signIn} from "../../helpers/admin-test-helper";
import {PageHeadings} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AddResultsPagePage} from "../../pages/admin/AddResultsPagePage";

test.describe('Get to an answer add an answer to a question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question1Content: string;
    let question2Id: string;

    let addAnswerPage: AddAnswerPage;
    let addResultsPagePage: AddResultsPagePage;
    
    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;

        const q1Resp = await createQuestion(request, questionnaireId, token);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        const q2Resp = await createQuestion(request, questionnaireId, token);
        question2Id = q2Resp.question.id;
    });

    test('Validate presence of elements on add answer page upon landing', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        await addAnswerPage.asserPageElementsUponLanding(0, 2);
        await addAnswerPage.asserPageElementsUponLanding(1, 2);
    });

    test("Header section - H1 heading", async ({page}) => {
        await signIn(page, token);
        
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        await addAnswerPage.expectAnswerHeadingOnPage();
    });

    test('Clicking the back link takes to list questions page', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.clickBackLink();
        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test('Remove button should remove answers option', async ({page}) => {
        let removeButtonsCount: number;
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(2);
        
        await addAnswerPage.removeOption(0);
        
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        
        expect(removeButtonsCount).toBe(1);
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();

        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();

        await addAnswerPage.asserPageElementsUponLanding(0, 1);
    })

    test("Validate presence of elements on add another answer form", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');

        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        
        await addAnswerPage.asserPageElementsUponLanding(2, 3);
    });

    test('Validate existing answer options are displayed upon saving and triggering validation', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        //Fill out initial options
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');
        
        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        
        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponLanding(2, 3);
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();

        await addAnswerPage.setOptionContent(2, 'Third Answer Option');
        await addAnswerPage.setOptionHint(2, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(2, 'InternalResultsPage');
        
        await addAnswerPage.clickSaveAndContinueButton();
        
        await addAnswerPage.errorSummaryLink("#Options-2-destination-internal").isVisible();
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        
        await addAnswerPage.asserPageElementsUponLanding(2, 3);
    })

    test('Validate reference name is selected when selecting answer destination as internal results page', async ({page}) => {
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
        
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        //Fill out initial options
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(0, resultsPageRefNameInput);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(1, resultsPageRefNameInput);

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    })

    test('Validate results page title is selected when selecting answer destination as internal results page', async ({page}) => {
        await signIn(page, token);

        // Add a new results page
        addResultsPagePage = await goToAddResultPagePageByUrl(page, questionnaireId);
        await addResultsPagePage.expectAddResultsPageHeadingOnPage(PageHeadings.ADD_RESULTS_PAGE_PAGE_HEADING);

        const resultsPageTitleInput = `Test title - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageTitleInput(resultsPageTitleInput);
        const resultsPageDetailsText = `Test details text - ${Date.now()}`;
        await addResultsPagePage.enterResultsPageDetailsText(resultsPageDetailsText);
        await addResultsPagePage.clickSaveAndContinue();

        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        //Fill out initial options
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(0, resultsPageTitleInput);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(1, resultsPageTitleInput);

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    })
    
    test('Accessible ids and aria-describedby for multiple options with hint', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.validateUniqueIdsForMultipleOptions(2);

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionContent(1, 'Second Answer Option');

        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.validateUniqueIdsForMultipleOptions(3);

        await addAnswerPage.setOptionContent(2, 'Third Answer Option');
        await addAnswerPage.setOptionHint(0, 'First answer hint');
        await addAnswerPage.setOptionHint(1, 'Second answer hint');
        await addAnswerPage.setOptionHint(2, 'Third answer hint');

        // Verify aria-describedby includes hint ids (no errors present)
        await addAnswerPage.validateAriaDescribedByForHintOnly(0);
        await addAnswerPage.validateAriaDescribedByForHintOnly(1);
        await addAnswerPage.validateAriaDescribedByForHintOnly(2);
    })

    test('Accessible Error Ids and aria-describedby for multiple Option content, specific question, internal link', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        //Select radios, but no values in drop-down selects
        await addAnswerPage.setSpecificQuestion(0, "");
        await addAnswerPage.setInternalLink(1, "")
        
        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateUniqueIdsForMultipleOptions(2);

        // Verify aria-describedby includes both hint id and error id when error is present
        await addAnswerPage.validateAriaDescribedByWithError(0, AnswerFieldName.Content);
        await addAnswerPage.validateAriaDescribedByWithError(1, AnswerFieldName.Content);
        
        await addAnswerPage.validateAriaDescribedByWithError(0, AnswerFieldName.SpecificQuestionSelect);
        await addAnswerPage.validateAriaDescribedByWithError(1, AnswerFieldName.ResultsPageSelect);
    })
    
    test('Accessible Error Ids and aria-describedby for multiple Option content, external link', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setExternalLink(0, "");
        await addAnswerPage.setExternalLink(1, "");
        
        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateUniqueIdsForMultipleOptions(2);

        await addAnswerPage.validateAriaDescribedByWithError(0, AnswerFieldName.Content);
        await addAnswerPage.validateAriaDescribedByWithError(1, AnswerFieldName.Content);
        await addAnswerPage.validateAriaDescribedByWithError(0, AnswerFieldName.ExternalLinkInput);
        await addAnswerPage.validateAriaDescribedByWithError(1, AnswerFieldName.ExternalLinkInput);
    })
});