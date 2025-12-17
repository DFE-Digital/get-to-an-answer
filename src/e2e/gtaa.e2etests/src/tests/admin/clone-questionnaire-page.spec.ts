import {expect, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {signIn} from '../../helpers/admin-test-helper';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {CloneQuestionnairePage} from '../../pages/admin/CloneQuestionnairePage';
import {ErrorMessages} from "../../constants/test-data-constants";
import {ViewQuestionnairePage} from "../../pages/admin/ViewQuestionnairePage";
import {AddQuestionnairePage} from "../../pages/admin/AddQuestionnairePage";
import {DesignQuestionnairePage} from "../../pages/admin/DesignQuestionnairePage";

test.describe('Clone questionnaire page', () => {
    let token: string;
    let questionnaireId: string;
    let questionnaireTitle: string;
    let cloneQuestionnairePage: CloneQuestionnairePage;
    let viewQuestionnairePage : ViewQuestionnairePage;
    let addQuestionnairePage: AddQuestionnairePage;
    let designQuestionnairePage: DesignQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();

        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireTitle = questionnaire.title;
        
        viewQuestionnairePage = await signIn(page, token);
        
    });

    test('Make a copy/Clone questionnaire page render with original title', async ({page}) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();
        
        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();
        
        await designQuestionnairePage.openMakeACopyPage();
        
        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);
        
        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);
    });
    

    test('shows validation errors when the title is blank', async ({page, browserName}) => {
        await viewQuestionnairePage.table.clickFirstQuestionnaireTitle();

        designQuestionnairePage = await DesignQuestionnairePage.create(page);
        await designQuestionnairePage.validateHeadingAndStatus();

        await designQuestionnairePage.openMakeACopyPage();

        cloneQuestionnairePage = await CloneQuestionnairePage.create(page);

        await cloneQuestionnairePage.expectPrefilledTitle(questionnaireTitle);
        
        await cloneQuestionnairePage.clearTitle();
        
        await cloneQuestionnairePage.clickMakeCopy();
        
        await cloneQuestionnairePage.validateMissingTitleMessageSummary(browserName);
        await cloneQuestionnairePage.validateErrorLinkBehaviour(
            cloneQuestionnairePage.errorSummaryLink('#Title'),
            ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE,
            browserName
        )
    });
    //
    // test('submitting with a new title redirects to the questions list for the clone', async ({page}) => {
    //     const newTitle = `Cloned from ${questionnaireTitle} - ${Date.now()}`;
    //     await clonePage.enterTitle(newTitle);
    //     await clonePage.clickMakeCopy();
    //
    //     await page.waitForURL(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    //     const questionsPage = await ViewQuestionPage.create(page);
    //     await questionsPage.expectQuestionHeadingOnPage();
    //     expect(page.url()).toMatch(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    // });
    //
    // test('shows validation errors when the title is blank', async ({browserName}) => {
    //     await clonePage.clearTitle();
    //     await clonePage.clickMakeCopy();
    //     await clonePage.expectValidationErrors(browserName);
    // });
    //
    // test('error summary link focuses the input when the title is missing', async ({browserName}) => {
    //     await clonePage.clearTitle();
    //     await clonePage.clickMakeCopy();
    //
    //     const errorLink = await clonePage.errorSummaryLink('#Title');
    //     await clonePage.validateErrorLinkBehaviour(errorLink, ErrorMessages.ERROR_MESSAGE_MISSING_QUESTIONNAIRE_TITLE, browserName);
    // });
    //
    // test('submitting with a new title redirects to the questions list for the clone', async ({page}) => {
    //     const newTitle = `Cloned from ${questionnaireTitle} - ${Date.now()}`;
    //     await clonePage.enterTitle(newTitle);
    //     await clonePage.clickMakeCopy();
    //
    //     await page.waitForURL(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    //     const questionsPage = await ViewQuestionPage.create(page);
    //     await questionsPage.expectQuestionHeadingOnPage();
    //     expect(page.url()).toMatch(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    // });
    //
    // test('saving the clone brings me back to the questionnaire design page', async ({page}) => {
    //     const newTitle = `Clone saved ${Date.now()}`;
    //     await clonePage.enterTitle(newTitle);
    //     await clonePage.clickMakeCopy();
    //
    //     await page.waitForURL(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    //     const questionsPage = await ViewQuestionPage.create(page);
    //     await questionsPage.expectQuestionHeadingOnPage();
    // });
});