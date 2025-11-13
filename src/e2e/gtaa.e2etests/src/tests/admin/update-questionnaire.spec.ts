// import {test, expect} from "@playwright/test";
// import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
// import {signIn, goToEditQuestionnaireTitlePageByUrl} from '../../helpers/admin-test-helper';
// import {EditQuestionnairePage} from "../../pages/admin/EditQuestionnairePage";
// import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
// import {JwtHelper} from "../../helpers/JwtHelper";
// import {ErrorMessages} from "../../constants/test-data-constants";
// import {createQuestionnaire, getQuestionnaire} from "../../test-data-seeder/questionnaire-data";
//
// test.describe('Get to an answer update questionnaire', () => {
//     let token: string;
//     let questionnaire: any;
//     let getQuestionnaireResponse:any;
//     let viewQuestionnairePage: ViewQuestionnairePage;
//     let addQuestionnairePage: AddQuestionnairePage;
//     let editQuestionnairePage: EditQuestionnairePage;
//
//     test.beforeEach(async ({request, page}) => {
//         token = JwtHelper.NoRecordsToken();
//         const res = await createQuestionnaire(request, token);
//         questionnaire = res.questionnaire;
//
//         const getQuestionnaireResponse = await getQuestionnaire(
//             request,
//             questionnaire.id,
//             token
//         );
//
//         viewQuestionnairePage = await signIn(page, token);
//         addQuestionnairePage = await goToEditQuestionnaireTitlePageByUrl(page, getQuestionnaireResponse.questionnaireGetBody.id);
//     });
//
//     test('Edit Title page displays all required elements', async ({page}) => {
//         await addQuestionnairePage.assertPageElements();
//     });
//
//     test('Back link to questionnaire from Edit Title page', async ({page}) => {
//         await addQuestionnairePage.ClickBackToQuestionnaireLink();
//         editQuestionnairePage = await EditQuestionnairePage.create(page);
//         expect(editQuestionnairePage.validateHeading());
//     });
//
//     test('Error summary appears on invalid submit with missing title', async ({page}) => {
//         await addQuestionnairePage.enterTitle('');
//         await addQuestionnairePage.clickSaveAndContinue();
//
//         await addQuestionnairePage.validateMissingTitleMessageFlow();
//     });
//
//     test('Inline error message and styling when title field has error', async ({page}) => {
//         await addQuestionnairePage.enterTitle('');
//         await addQuestionnairePage.clickSaveAndContinue();
//
//         await addQuestionnairePage.validateInvalidTitleMessageFlow();
//     });
//
//     test('Accessible aria-describedby includes hint id and error message id', async ({page}) => {
//         await addQuestionnairePage.enterTitle('');
//         await addQuestionnairePage.clickSaveAndContinue();
//
//         await addQuestionnairePage.expectTitleAriaDescribedByIncludesHintAndError();
//     });
//
//     // TBC - redirect not working yet upon update title
//     // test('Successful submit updates title and validation', async ({page}) => {
//     //     const newTitle = `Updated questionnaire title - ${Date.now()}`;
//     //     await addQuestionnairePage.enterTitle(newTitle);
//     //     await addQuestionnairePage.clickSaveAndContinue(); //redirects to edit questionnaire page
//     //
//     //     editQuestionnairePage = await EditQuestionnairePage.create(page);
//     //     //await editQuestionnairePage.expectSuccessBannerVisible(); //TBC
//     //     await editQuestionnairePage.validateHeadingAndStatus();
//     //
//     //     await editQuestionnairePage.ClickBackToQuestionnaireLink()
//     //     viewQuestionnairePage = await ViewQuestionnairePage.create(page);
//     //
//     //     const expectedRows = [
//     //         {
//     //             title: getQuestionnaireResponse.questionnaireGetBody.title,
//     //             createdBy: getQuestionnaireResponse.questionnaireGetBody.createdBy,
//     //             status: getQuestionnaireResponse
//     //         }
//     //     ];
//     //    
//     //     await viewQuestionnairePage.table.verifyTableData(expectedRows);
//     // });
// });
//
