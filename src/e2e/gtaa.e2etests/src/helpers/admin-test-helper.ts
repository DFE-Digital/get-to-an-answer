import {Page} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {NewQuestionnairePage} from '../pages/admin/NewQuestionnairePage';

export async function doSignIn(page: Page, username: string, password: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);
    const viewQuestionnairePage = new ViewQuestionnairePage(page);

    await signInPage.navigateTo('/');
    await signInPage.verifyOnSignInPage();
    await signInPage.signIn(username, password);

    await viewQuestionnairePage.validateUrlContains('/admin/questionnaires');
    return viewQuestionnairePage;
}

export async function goToNewQuestionnairePage(page: Page): Promise<NewQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    const newQuestionnairePage = new NewQuestionnairePage(page);

    await viewQuestionnairePage.ClickCreateNewQuestionnaire();
    await newQuestionnairePage.verifyOnNewQuestionnairePage();

    return newQuestionnairePage;
}