// import {test} from "@playwright/test";
// import {DeleteQuestionnaireConfirmationPage} from "../../pages/admin/DeleteQuestionnairePage";
// import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
// import {signIn, goToEditQuestionnairePageByUrl} from '../../helpers/admin-test-helper';
// import {JwtHelper} from "../../helpers/JwtHelper";
// import {createQuestionnaire, getQuestionnaire, deleteQuestionnaire} from "../../test-data-seeder/questionnaire-data";
//
// test.describe('Get to an answer delete questionnaire', () => {
//     let token: string;
//     let questionnaire: any;
//     let deleteQuestionnairePage: DeleteQuestionnaireConfirmationPage;
//     let editQuestionnairePage: EditQuestionnairePage;
//
//     test.beforeEach(async ({request, page}) => {
//         token = JwtHelper.NoRecordsToken();
//
//         // Create a questionnaire via API
//         const createRes = await createQuestionnaire(request, token);
//         questionnaire = createRes.questionnaire;
//
//         // Get questionnaire details
//         const getRes = await getQuestionnaire(request, questionnaire.id, token);
//         const questionnaireId = getRes.questionnaireGetBody.id;
//
//         // Sign in
//         await signIn(page, token);
//
//         // Navigate to Edit Questionnaire page
//         editQuestionnairePage = await goToEditQuestionnairePageByUrl(page, questionnaireId);
//         await editQuestionnairePage.waitForPageLoad();
//
//         // Navigate to Delete Confirmation page
//         await editQuestionnairePage.delete();
//         deleteQuestionnairePage = new DeleteQuestionnaireConfirmationPage(page);
//         await page.waitForLoadState('networkidle');
//     });
//
//     test('Back link to questionnaire track from Delete Confirmation page', async ({page}) => {
//         await deleteQuestionnairePage.expectBackLinkContainsTitle(questionnaire.title);
//        
//         await deleteQuestionnairePage.clickBackLink();
//
//         editQuestionnairePage = new EditQuestionnairePage(page);
//         await editQuestionnairePage.validateHeading();
//     });
//
//     test('Required choice validation when submitting without selecting Yes or No', async ({page}) => {
//         await deleteQuestionnairePage.expectRequiredChoiceValidation();
//     });
//
//     test('Choose Yes and submit deletes the questionnaire', async ({request, page}) => {
//         const getRes = await getQuestionnaire(request, questionnaire.id, token);
//         const questionnaireId = getRes.questionnaireGetBody.id;
//        
//         await deleteQuestionnairePage.chooseYesAndSubmitWaitForDelete('/delete');
//     });
//
//     test('Choose No and return to questionnaire track page', async ({page}) => {
//         const getRes = await getQuestionnaire(page.context().request, questionnaire.id, token);
//        
//         await deleteQuestionnairePage.chooseNoAndSubmitReturnToTrack('/track');
//        
//         editQuestionnairePage = new EditQuestionnairePage(page);
//         await editQuestionnairePage.validateHeading();
//     });
//
//     test('Continue button submits the selected choice', async ({page}) => {
//         await deleteQuestionnairePage.expectTwoRadiosPresent();
//        
//         await deleteQuestionnairePage.chooseYes();
//
//         // Click continue and verify it submits
//         const selectedIndex = await deleteQuestionnairePage.submitAndReturnSelectedIndex();
//
//         // Verify that the first radio (Yes) was selected and submitted
//         test.expect(selectedIndex).toBe(0);
//     });
//
//     test('Validate all required UI elements are present on Delete Confirmation page', async ({page}) => {
//         await deleteQuestionnairePage.expectTwoRadiosPresent();
//         await deleteQuestionnairePage.expectBackLinkContainsTitle(questionnaire.title);
//     });
//
//     test('Back link contains questionnaire title', async ({page}) => {
//         const titleText = questionnaire.title;
//         await deleteQuestionnairePage.expectBackLinkContainsTitle(titleText);
//     });
// });