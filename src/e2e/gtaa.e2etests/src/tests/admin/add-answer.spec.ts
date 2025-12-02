import {test} from "@playwright/test";
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

        await addAnswerPage.asserPageElementsUponLanding();
    });

    test("Header section - H1 and questionnaire status", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.expectQuestionnaireStatusOnPage('Draft');
    });

    test("Validate presence of elements on add another answer form", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();
        await addAnswerPage.asserPageElementsUponAddAnotherOptionClick(0);
    });

    test("Submit an answer to a single radio question with NextQuestion destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');
        await addAnswerPage.clickContinueButton();

        const editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
    });

    test("Submit an answer to a single radio question with SpecificQuestion destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question2Id);

        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setSpecificQuestion(0, question1Content);

        await addAnswerPage.clickContinueButton();

        const editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
        await editQuestionnairePage.validateHeadingAndStatus();
    });

    test("Submit an answer to a single radio question with InternalResultsPage destination", async ({request, page}) => {
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: 'Test Content',
            content: 'This is a test content for the start page.',
            referenceName: 'test-content'
        }, token)

        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(0, 'test-content');

        await addAnswerPage.clickContinueButton();

        const editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
        await editQuestionnairePage.validateHeadingAndStatus();
    });

    test("Submit an answer to a single radio question with ExternalResultsPage destination", async ({page}) => {
        await signIn(page, token);
        addAnswerPage = await goToAddAnswerPageByUrl(page, questionnaireId, question1Id);
        
        await addAnswerPage.expectAnswerHeadingOnPage();
        await addAnswerPage.clickAddAnotherOptionButton();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');
        
        await addAnswerPage.clickContinueButton();

        const editQuestionnairePage = await EditQuestionnairePage.create(page);
        await editQuestionnairePage.validateHeading(PageHeadings.EDIT_QUESTIONNAIRE_PAGE_HEADING);
        await editQuestionnairePage.validateHeadingAndStatus();
    });

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
    //
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