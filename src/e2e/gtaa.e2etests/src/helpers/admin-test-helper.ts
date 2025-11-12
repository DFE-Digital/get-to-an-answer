import {Page, expect} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {EditQuestionnairePage} from "../pages/admin/EditQuestionnairePage";
import {TermsOfUsePage} from "../pages/admin/TermsOfUsePage";
import {JwtHelper} from "./JwtHelper";

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