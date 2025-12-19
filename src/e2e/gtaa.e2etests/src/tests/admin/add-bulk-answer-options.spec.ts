import {test, expect} from "@playwright/test";
import {goToViewQuestionsPageByUrl, signIn} from '../../helpers/admin-test-helper';
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {JwtHelper} from "../../helpers/JwtHelper";
import {CloneQuestionnairePage} from "../../pages/admin/CloneQuestionnairePage";
import {createQuestionnaire} from "../../test-data-seeder/questionnaire-data";
import {createQuestion} from "../../test-data-seeder/question-data";
import {AddBulkAnswerOptionsPage} from "../../pages/admin/AddBulkAnswerOptionsPage";
import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
import {AddQuestionPage, QuestionRadioLabel} from "../../pages/admin/AddQuestionPage";
import {AddAnswerPage} from "../../pages/admin/AddAnswerPage";


test.describe('Get to an answer Add bulk answers options to question', () => {
    let token: string;
    let questionnaireTitle: string;
    let bulkAddAnswersPage : AddBulkAnswerOptionsPage;
    let addQuestionPage : AddQuestionPage;
    let viewQuestionsPage : ViewQuestionPage;
    let addAnswerPage : AddAnswerPage;
    
    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireTitle = questionnaire.title;
        
        await signIn(page, token);

        viewQuestionsPage = await goToViewQuestionsPageByUrl(page, questionnaire.id);
    });

    
    test('Add bulk answers options from Add answers to question successfully lands on Add Questionnaire Page with entered answer options', async ({ page }) => {
        
        await viewQuestionsPage.clickAddQuestion();
        
        addQuestionPage = await AddQuestionPage.create(page);

        const questionContent = `Test Question - ${Date.now()}`;
        await addQuestionPage.enterQuestionContent(questionContent);
        await addQuestionPage.chooseQuestionType(QuestionRadioLabel.SingleSelectShort);
        await addQuestionPage.clickSaveAndContinue();

        addAnswerPage = await AddAnswerPage.create(page);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.clickEnterAllOptionsButton();
        
        bulkAddAnswersPage = await AddBulkAnswerOptionsPage.create(page);
        await bulkAddAnswersPage.expectOnPage();
        
        await bulkAddAnswersPage.enterNumberOfValidBulkOptions(5);
        
        await bulkAddAnswersPage.clickContinue();
        
        await expect(page).toHaveURL(/\/answers\/add$/);
        await addAnswerPage.expectAnswerHeadingOnPage();
        
        await addAnswerPage.assertAllOptionNumberLabelsInOrder();
        
        await addAnswerPage.validateAllOptionContents(bulkAddAnswersPage.getEnteredBulkOptions);
        
        await addAnswerPage.clickSaveAndContinueButton();
        
        await viewQuestionsPage.expectQuestionHeadingOnPage();
        
        await viewQuestionsPage.clickFirstEditQuestionLink();
        
        await addQuestionPage
    })
})