// import {test} from "@playwright/test";
// import {AddQuestionPage, QuestionType} from "../../pages/admin/AddQuestionPage";
// import {ViewQuestionPage} from "../../pages/admin/ViewQuestionPage";
// import {AddAnswersPage} from "../../pages/admin/AddAnswersPage";
// import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
// import {
//     signIn,
//     goToEditQuestionnairePageByUrl,
//     goToEditQuestionnaireTitlePageByUrl, goToAddQuestionPageByUrl, goToAddQuestionnairePage
// } from '../../helpers/admin-test-helper';
// import {JwtHelper} from "../../helpers/JwtHelper";
// import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
// import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
//
// test.describe('Get to an answer add question to questionnaire', () => {
//     let token: string;
//     let questionnaire: any;
//     let addQuestionPage: AddQuestionPage;
//     let viewQuestionPage: ViewQuestionPage;
//     let addAnswersPage: AddAnswersPage;
//
//     test.beforeEach(async ({request, page}) => {
//         token = JwtHelper.NoRecordsToken();
//        
//         const {questionnaire} = await createQuestionnaire(request, token);
//        
//         const {questionnaireGetBody} = await getQuestionnaire(request, questionnaire.id, token);
//         const questionnaireId = questionnaireGetBody.id;
//        
//         await signIn(page, token);
//         addQuestionPage = await goToAddQuestionPageByUrl(page, questionnaireId);
//     });
//
//     test('Validate presence of elements on add new question page', async ({page}) => {
//         await addQuestionPage.assertPageElements();
//     });
//
//     test('Back link to questions list from Add Question page', async ({page}) => {
//         await addQuestionPage.verifyBackLinkPresent();
//         await addQuestionPage.clickBackLink();
//
//         viewQuestionPage = new ViewQuestionPage(page);
//         await viewQuestionPage.expectQuestionnaireHeadingOnPage();
//     });
//
//     test('Error summary appears on invalid submit with missing required fields', async ({page}) => {
//         await addQuestionPage.save();
//         await addQuestionPage.validateMissingfAllErrorMessageSummary();
//     });
//
//     test('Inline error and styling for missing question text', async ({page}) => {
//         await addQuestionPage.clearQuestion();
//         await addQuestionPage.chooseType(QuestionType.SingleSelectShort);
//         await addQuestionPage.save();
//
//         await addQuestionPage.validateInlineQuestionTextError();
//     });
//    
//     test('Inline error and styling for missing question type radio', async ({page}) => {
//         const questionText = `Test Question - ${Date.now()}`;
//         const hintText = `Test Hint - ${Date.now()}`;
//
//         await addQuestionPage.enterQuestion(questionText);
//         await addQuestionPage.enterHint(hintText);
//         await addQuestionPage.save();
//
//         await addQuestionPage.validateRadiosError();
//     });
//
//     test('Successful submit creates question', async ({page}) => {
//         const questionText = `Test Question - ${Date.now()}`;
//         const hintText = `Test Hint - ${Date.now()}`;
//
//         await addQuestionPage.enterQuestion(questionText);
//         await addQuestionPage.enterHint(hintText);
//         await addQuestionPage.chooseType(QuestionType.SingleSelectShort);
//         await addQuestionPage.save();
//
//         viewQuestionPage = await ViewQuestionPage.create(page);
//         await viewQuestionPage.expectQuestionnaireHeadingOnPage();
//     });
//    
//     test('Create question with multi-select type', async ({page}) => {
//         const questionText = `Test Question - ${Date.now()}`;
//         const hintText = `Test Hint - ${Date.now()}`;
//
//         await addQuestionPage.enterQuestion(questionText);
//         await addQuestionPage.enterHint(hintText);
//         await addQuestionPage.chooseType(QuestionType.MultiSelect);
//         await addQuestionPage.save();
//
//         viewQuestionPage = await ViewQuestionPage.create(page);
//         await viewQuestionPage.expectQuestionnaireHeadingOnPage();
//     });
// });