import {expect, test} from '@playwright/test';
import {JwtHelper} from '../../helpers/JwtHelper';
import {signIn} from '../../helpers/admin-test-helper';
import {createQuestionnaire} from '../../test-data-seeder/questionnaire-data';
import {CloneQuestionnairePage} from '../../pages/admin/CloneQuestionnairePage';
import {ViewQuestionPage} from '../../pages/admin/ViewQuestionPage';

test.describe('Clone questionnaire page', () => {
    let token: string;
    let questionnaireId: string;
    let questionnaireTitle: string;
    let clonePage: CloneQuestionnairePage;

    test.beforeEach(async ({request, page}) => {
        token = JwtHelper.NoRecordsToken();
        const {questionnaire} = await createQuestionnaire(request, token);
        questionnaireId = questionnaire.id;
        questionnaireTitle = questionnaire.title;

        await signIn(page, token);
        await page.goto(`/admin/questionnaires/${questionnaireId}/clone`);
        clonePage = new CloneQuestionnairePage(page);
        await clonePage.waitForPageLoad();
    });

    test('renders helpers and caption with original title', async () => {
        await clonePage.expectOnPage();
        await clonePage.expectPrefilledTitle(questionnaireTitle);
    });

    test('shows validation errors when the title is blank', async ({browserName}) => {
        await clonePage.clearTitle();
        await clonePage.clickMakeCopy();
        await clonePage.expectValidationErrors(browserName);
    });

    test('submitting with a new title redirects to the questions list for the clone', async ({page}) => {
        const newTitle = `Cloned from ${questionnaireTitle} - ${Date.now()}`;
        await clonePage.enterTitle(newTitle);
        await clonePage.clickMakeCopy();

        await page.waitForURL(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
        const questionsPage = await ViewQuestionPage.create(page);
        await questionsPage.expectQuestionHeadingOnPage();
        expect(await page.url()).toMatch(/\/admin\/questionnaires\/[0-9a-f-]+\/questions/);
    });
});