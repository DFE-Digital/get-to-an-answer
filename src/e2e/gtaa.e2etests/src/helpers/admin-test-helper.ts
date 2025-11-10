import {Page, expect} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {EditQuestionnairePage} from "../pages/admin/EditQuestionnairePage";
import {JwtHelper} from "./JwtHelper";

export async function localSignIn(page: Page): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);
    
    page.once('pageerror', e => {
        if (page.url().includes('/dev/login')) {
            console.warn('Ignored /dev/login error:', e.message);
        }
    });
    
    await signInPage
        .navigateTo(`/dev/login?jt=${JwtHelper.ValidToken}`, 'domcontentloaded')
        .catch(() => { /* ignore if it redirects instantly */ });
    
    await page.waitForLoadState('domcontentloaded').catch(() => {});
    await page.waitForTimeout(500); // short grace period (useful for mobile Safari)
    
    await signInPage
        .navigateTo(`/admin/questionnaires`, 'domcontentloaded')
        .catch(() => {});
    
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    
    return viewQuestionnairePage;
}

export async function SignIn(page: Page, username: string, password: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);
    await signInPage.navigateTo('/');
    await signInPage.waitForPageLoad();
    await signInPage.verifyOnSignInPage();
    await signInPage.enterCredentialsToSignIn(username, password);

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