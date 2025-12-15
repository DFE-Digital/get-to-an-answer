import {test} from "@playwright/test";
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
    let question2Id: string;

    let addQuestionPage: AddQuestionPage;
    let addAnswerPage: AddAnswerPage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        
        const q1Resp = await createQuestion(request, questionnaireId, token,
            undefined, QuestionType.DropdownSelect);
        question1Id = q1Resp.question.id;
        question1Content = q1Resp.question.content;

        await signIn(page, token);
        addQuestionPage = await goToAddQuestionPageByUrl(page, questionnaire.id);
    });

    test("Submit an answer to a multiselect question with NextQuestion destination", async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.chooseDestination(0, 'NextQuestion');
        await addAnswerPage.setAnswerRank(0, 1);

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.chooseDestination(1, 'NextQuestion');
        await addAnswerPage.setAnswerRank(1, 2);
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test("Submit an answer to a single radio question with SpecificQuestion destination", async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setSpecificQuestion(0, question1Content);
        await addAnswerPage.setAnswerRank(0, 1);

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setSpecificQuestion(1, question1Content);

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test("Submit an answer to a single radio question with InternalResultsPage destination", async ({
                                                                                                        request,
                                                                                                        page
                                                                                                    }) => {
        const referenceName = 'Test Content';
        const apiContentResponse = await createContent(request, {
            questionnaireId,
            title: 'Test Content',
            content: 'This is a test content for the start page.',
            referenceName
        }, token)

        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setInternalLink(0, referenceName);
        await addAnswerPage.setAnswerRank(0, 1);

        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setInternalLink(1, referenceName);
        await addAnswerPage.setAnswerRank(1, 2);

        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });

    test("Submit an answer to a single radio question with ExternalResultsPage destination", async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.setOptionContent(0, 'First Answer Option');
        await addAnswerPage.setOptionHint(0, 'This is the first answer hint');
        await addAnswerPage.setExternalLink(0, 'https://www.example.com');
        await addAnswerPage.setAnswerRank(0, 1);
        
      
        await addAnswerPage.setOptionContent(1, 'Second Answer Option');
        await addAnswerPage.setOptionHint(1, 'This is the second answer hint');
        await addAnswerPage.setExternalLink(1, 'https://www.example.com');
        await addAnswerPage.setAnswerRank(1, 2);

        await addAnswerPage.clickAddAnotherOptionButton();
        await  addAnswerPage.waitForPageLoad();

        await addAnswerPage.setOptionContent(2, 'Third Answer Option');
        await addAnswerPage.setOptionHint(2, 'This is the third answer hint');
        await addAnswerPage.setExternalLink(2, 'https://www.example.com');
        await addAnswerPage.setAnswerRank(2, 3);
        
        await addAnswerPage.clickSaveAndContinueButton();

        const viewQuestionPage = await ViewQuestionPage.create(page);
        await viewQuestionPage.expectQuestionHeadingOnPage(PageHeadings.VIEW_QUESTION_PAGE_HEADING);
    });
    
    test("Error summary appears on invalid submit with all missing required fields", async ({page, browserName}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateMissingAllFieldsErrorMessageSummary(browserName);
    })

    test("Inline error and styling for missing option content", async ({page}) => {
        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.MultiSelect);
        await addQuestionPage.clickSaveAndContinue();
        
        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();

        await addAnswerPage.clickSaveAndContinueButton();
        await addAnswerPage.validateInlineQuestionContentError(0);
        await addAnswerPage.validateInlineQuestionContentError(1);
    })
});