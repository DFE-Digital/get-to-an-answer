import {Page} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {EditQuestionnairePage} from "../pages/admin/EditQuestionnairePage";

export async function doSignIn(page: Page, username: string, password: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);
    await signInPage.navigateTo('/');
    await signInPage.waitForPageLoad();
    await signInPage.verifyOnSignInPage();
    await signInPage.signIn(username, password);

    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.expectUrlOnPage();
    
    return viewQuestionnairePage;
}

export async function goToAddQuestionnairePage(page: Page): Promise<AddQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.clickCreateNewQuestionnaire();

    const addQuestionnairePage = new AddQuestionnairePage(page);
    await addQuestionnairePage.waitForPageLoad();
    await addQuestionnairePage.expectUrlOnPage();

    return addQuestionnairePage;
}

export async function goToEditQuestionnairePage(page: Page): Promise<EditQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    await viewQuestionnairePage.waitForPageLoad();
    await viewQuestionnairePage.clickCreateNewQuestionnaire();

    const addQuestionnairePage = new AddQuestionnairePage(page);
    await addQuestionnairePage.waitForPageLoad();
    await addQuestionnairePage.expectUrlOnPage()

    const editQuestionnairePage = new EditQuestionnairePage(page);
    await editQuestionnairePage.waitForPageLoad();
    await editQuestionnairePage.expectUrlOnPage();

    return editQuestionnairePage;
}