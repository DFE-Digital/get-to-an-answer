import {Page, expect} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {ViewQuestionPage} from '../pages/admin/ViewQuestionPage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {AddQuestionPage} from '../pages/admin/AddQuestionPage';
import {AddAnswerPage} from '../pages/admin/AddAnswerPage';
import {EditQuestionnairePage} from "../pages/admin/EditQuestionnairePage";
import {TermsOfUsePage} from "../pages/admin/TermsOfUsePage";
import {EnvConfig} from '../config/environment-config';

type LoadState = 'domcontentloaded' | 'load' | 'networkidle';

export async function landing(page: Page, bearerToken?: string): Promise<SignInPage> {
    const signInPage = new SignInPage(page);
    await signInPage.openSignInPage(bearerToken);

    return signInPage;
}

export async function signIn(page: Page, bearerToken?: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);

    await signInPage.openSignInPage(bearerToken);
    await signInPage.clickSignIn();

    const termsOfUsePage = new TermsOfUsePage(page);
    await termsOfUsePage.agreeToTermsOfUse();

    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();

    return viewQuestionnairePage;
}

export async function goToAddQuestionnairePage(page: Page): Promise<AddQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.clickCreateNewQuestionnaire();

    const addQuestionnairePage = new AddQuestionnairePage(page);
    await addQuestionnairePage.waitForPageLoad();

    return addQuestionnairePage;
}

export async function goToEditQuestionnairePage(page: Page): Promise<EditQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.clickCreateNewQuestionnaire();

    const addQuestionnairePage = new AddQuestionnairePage(page);
    await addQuestionnairePage.waitForPageLoad();

    const editQuestionnairePage = new EditQuestionnairePage(page);
    await editQuestionnairePage.waitForPageLoad();

    return editQuestionnairePage;
}

// =====  URL based page navigation =====
export async function goToUpdateQuestionnairePageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddQuestionnairePage> {
    
    const adminUrl = EnvConfig.ADMIN_URL;
    const editTitleUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/edit-name`;

    await page.goto(editTitleUrl, {waitUntil});

    const addQuestionnairePage = new AddQuestionnairePage(page, 'update')
    await addQuestionnairePage.waitForPageLoad();

    return addQuestionnairePage;
}

export async function goToEditQuestionnairePageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<EditQuestionnairePage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const editUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/track`;

    await page.goto(editUrl, {waitUntil});

    const editQuestionnairePage = new EditQuestionnairePage(page)
    await editQuestionnairePage.waitForPageLoad();

    return editQuestionnairePage;
}

export async function goToViewQuestionsPageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<ViewQuestionPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const viewQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/questions`;

    await page.goto(viewQuestionUrl, {waitUntil});

    const viewQuestionPage = new ViewQuestionPage(page)
    await viewQuestionPage.waitForPageLoad();

    return viewQuestionPage;
}

export async function goToAddQuestionPageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddQuestionPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/questions/add`;

    await page.goto(addQuestionUrl, {waitUntil});

    const addQuestionPage = new AddQuestionPage(page)
    await addQuestionPage.waitForPageLoad();

    return addQuestionPage;
}

export async function goToAddAnswerPageByUrl(
    page: Page,
    questionnaireId: string,
    questionId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddAnswerPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addAnswerUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/questions/${questionId}/answers/add`;

    await page.goto(addAnswerUrl, {waitUntil});

    const addAnswerPage = new AddAnswerPage(page)
    await addAnswerPage.waitForPageLoad();

    return addAnswerPage;
}