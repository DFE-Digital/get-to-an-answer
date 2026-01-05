import {FrameLocator, Page} from "@playwright/test";
import {DesignQuestionnairePage} from "../pages/admin/DesignQuestionnairePage";
import {EnvConfig} from "../config/environment-config";
import {LoadState} from "./admin-test-helper";
import {QuestionnaireStartPage} from "../pages/fe/QuestionnaireStartPage";
import {QuestionnaireNextPage} from "../pages/fe/QuestionnaireNextPage";


export async function goToPublishedQuestionnairePageByUrl(
    page: Page,
    questionnaireSlug: string,
    waitUntil: LoadState = 'networkidle'): Promise<QuestionnaireStartPage> {

    const feUrl = EnvConfig.FE_URL;
    const startPageUrl = `${feUrl}/questionnaires/${questionnaireSlug}/start?embed=false`;

    await page.goto(startPageUrl, {waitUntil});

    const questionnaireStartPage = new QuestionnaireStartPage(page)
    await questionnaireStartPage.waitForPageLoad();

    return questionnaireStartPage;
}

export async function goToPublishedQuestionnairePageForResponse(
    page: Page,
    questionnaireSlug: string): Promise<any> {

    const feUrl = EnvConfig.FE_URL;
    const startPageUrl = `${feUrl}/questionnaires/${questionnaireSlug}/start?embed=false`;

    const responsePromise = page.waitForResponse(response =>
        response.status() === 404
    );

    try {
        return await page.goto(startPageUrl);
    } catch (error) {
        console.error(`Failed to navigate to questionnaire page: ${error}`);
        return await responsePromise;
    }
}

export async function goToPublishedQuestionnaireNextPageByUrl(
    page: Page,
    questionnaireSlug: string,
    waitUntil: LoadState = 'networkidle'): Promise<QuestionnaireNextPage> {

    const feUrl = EnvConfig.FE_URL;
    const startPageUrl = `${feUrl}/questionnaires/${questionnaireSlug}/next?embed=false`;

    await page.goto(startPageUrl, {waitUntil});

    const questionnaireNextPage = new QuestionnaireNextPage(page)
    await questionnaireNextPage.waitForPageLoad();

    return questionnaireNextPage;
}

export async function waitForRedirectTo(page: Page, url: string) {
    return await page.waitForURL(currentUrl => 
        currentUrl.toString().includes(url));
}