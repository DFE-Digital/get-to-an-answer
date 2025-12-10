import {Page, expect} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {ViewQuestionPage} from '../pages/admin/ViewQuestionPage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {AddQuestionPage} from '../pages/admin/AddQuestionPage';
import {AddAnswerPage} from '../pages/admin/AddAnswerPage';
import {DesignQuestionnairePage} from "../pages/admin/DesignQuestionnairePage";
import {TermsOfUsePage} from "../pages/admin/TermsOfUsePage";
import {EnvConfig} from '../config/environment-config';
import {AddQuestionnaireStartPage} from "../pages/admin/AddQuestionnaireStartPage";
import {ViewResultsPagesPage} from "../pages/admin/ViewResultsPagesPage";
import {AddResultsPagePage} from "../pages/admin/AddResultsPagePage";
import {EditResultsPagePage} from "../pages/admin/EditResultsPagePage";
import {ViewContributorPage} from "../pages/admin/ViewContributorPage";

type LoadState = 'domcontentloaded' | 'load' | 'networkidle';

export async function landing(page: Page, bearerToken?: string): Promise<SignInPage> {
    const signInPage = new SignInPage(page);
    await signInPage.openSignInPage(bearerToken);

    return signInPage;
}

export async function signIn(page: Page, bearerToken?: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);

    await signInPage.openSignInPage(bearerToken);
    await signInPage.acceptCookiesIfVisible();

    const termsOfUsePage = new TermsOfUsePage(page);
    await termsOfUsePage.acceptCookiesIfVisible();
    await termsOfUsePage.agreeToTermsOfUse();

    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await signInPage.acceptCookiesIfVisible();

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

export async function goToEditQuestionnairePage(page: Page): Promise<DesignQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.clickCreateNewQuestionnaire();

    const addQuestionnairePage = new AddQuestionnairePage(page);
    await addQuestionnairePage.waitForPageLoad();

    const designQuestionnairePage = new DesignQuestionnairePage(page);
    await designQuestionnairePage.waitForPageLoad();

    return designQuestionnairePage;
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

export async function goToUpdateQuestionPageByUrl(
    page: Page,
    questionnaireId: string,
    questionId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddQuestionPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const editTitleUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/questions/${questionId}/edit`;

    await page.goto(editTitleUrl, {waitUntil});

    const addQuestionPage = new AddQuestionPage(page, 'update')
    await addQuestionPage.waitForPageLoad();

    return addQuestionPage;
}

export async function goToDesignQuestionnairePageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<DesignQuestionnairePage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const editUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/track`;

    await page.goto(editUrl, {waitUntil});

    const designQuestionnairePage = new DesignQuestionnairePage(page)
    await designQuestionnairePage.waitForPageLoad();

    return designQuestionnairePage;
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

export async function goToViewResultsPagesPageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<ViewResultsPagesPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const viewQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/contents`;

    await page.goto(viewQuestionUrl, {waitUntil});

    const viewResultsPagesPage = new ViewResultsPagesPage(page)
    await viewResultsPagesPage.waitForPageLoad();

    return viewResultsPagesPage;
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

export async function goToAddQuestionnaireStartPageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddQuestionnaireStartPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/start-page/edit`;

    await page.goto(addQuestionUrl, {waitUntil});

    const addQuestionnaireStartPage = new AddQuestionnaireStartPage(page)
    await addQuestionnaireStartPage.waitForPageLoad();

    return addQuestionnaireStartPage;
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

export async function goToUpdateAnswerPageByUrl(
    page: Page,
    questionnaireId: string,
    questionId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddAnswerPage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addAnswerUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/questions/${questionId}/answers/edit`;

    await page.goto(addAnswerUrl, {waitUntil});

    const addAnswerPage = new AddAnswerPage(page, 'update')
    await addAnswerPage.waitForPageLoad();

    return addAnswerPage;
}

export async function goToAddResultPagePageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<AddResultsPagePage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/contents/add`;

    await page.goto(addQuestionUrl, {waitUntil});

    const addResultsPagePage = new AddResultsPagePage(page)
    await addResultsPagePage.waitForPageLoad();

    return addResultsPagePage;
}

export async function goToEditResultPagePageByUrl(
    page: Page,
    questionnaireId: string,
    resultPageId: string,
    waitUntil: LoadState = 'networkidle'): Promise<EditResultsPagePage> {

    const adminUrl = EnvConfig.ADMIN_URL;
    const addQuestionUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/contents/${resultPageId}/edit`;

    await page.goto(addQuestionUrl, {waitUntil});

    const addResultsPagePage = new EditResultsPagePage(page)
    await addResultsPagePage.waitForPageLoad();

    return addResultsPagePage;
}

export async function goToQuestionnaireContributorsPageByUrl(
    page: Page,
    questionnaireId: string,
    waitUntil: LoadState = 'networkidle'): Promise<ViewContributorPage> {
    
    const adminUrl = EnvConfig.ADMIN_URL;
    
    // to cache persistence questionnaire information
    const trackQuestionnaireUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/track`;
    await page.goto(trackQuestionnaireUrl, {waitUntil});
    
    const viewContributorsUrl = `${adminUrl}/admin/questionnaires/${questionnaireId}/contributors`;

    await page.goto(viewContributorsUrl, {waitUntil});

    const viewContributorPage = new ViewContributorPage(page)
    await viewContributorPage.waitForPageLoad();

    return viewContributorPage;
}
