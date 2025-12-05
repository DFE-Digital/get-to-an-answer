import {expect, test} from "@playwright/test";
import {AddQuestionPage} from "../../pages/admin/AddQuestionPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {
    goToAddAnswerPageByUrl,
    goToAddQuestionPageByUrl,
    goToViewQuestionsPageByUrl,
    signIn
} from "../../helpers/admin-test-helper";
import {ErrorMessages, PageHeadings} from "../../constants/test-data-constants";
import {createQuestion} from "../../test-data-seeder/question-data";
import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
import {createContent} from "../../test-data-seeder/content-data";

test.describe('Get to an answer add an answer to a question', () => {
    let token: string;
    let questionnaireId: string;
    let question1Id: string;
    let question1Content: string;
    let question2Id: string;

    let addAnswerPage: AddAnswerPage;

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

    test("Header section - H1 and questionnaire status", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.expectQuestionnaireStatusOnPage('Draft');
    });

    test('Clicking the back link takes to list questions page', async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.clickBackLInk();
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
        await addAnswerPage.removeOption(1);
        removeButtonsCount = await addAnswerPage.getRemoveButtonCount();
        expect(removeButtonsCount).toBe(1);
        
        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponLanding(0, 1);
    })


    test("Submit an answer to a single radio question with NextQuestion destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

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
        await addAnswerPage.asserPageElementsUponLanding(2, 3);
    });
    
    test("Submit an answer to a single radio question with SpecificQuestion destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question2Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setSpecificQuestion(0, question1Content);

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setSpecificQuestion(1, question1Content);

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    // TBC, CARE-1568
    test("Submit an answer to a single radio question with InternalResultsPage destination", async ({request, page}) => {
        const title = 'Test Content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title,
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)

        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(0, title);

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setInternalLink(1, title);
        
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });
    
    test("Submit an answer to a single radio question with ExternalResultsPage destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setExternalLink(1, 'https://www.example.com');

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test("Error summary appears on invalid submit with all missing required fields", async ({page, browserName}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    })

    test("Inline error and styling for missing option content", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateInlineQuestionContentError(0);
        await addAnswerPage.validateInlineQuestionContentError(1);
    })



    // test("Submit an answer to a single dropdown question with NextQuestion destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a single dropdown question with SpecificQuestion destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });

    // test("Submit an answer to a single dropdown question with InternalResultsPage destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a single dropdown question with ExternalResultsPage destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a multi-select question with NextQuestion destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a multi-select question with SpecificQuestion destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a multi-select question with InternalResultsPage destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
    //
    // test("Submit an answer to a multi-select question with ExternalResultsPage destination", async () => {
    //     await addAnswerPage.expectAnswerHeadingOnPage();
    //     await addAnswerPage.clickAddAnotherOptionButton();
    //
    // });
});