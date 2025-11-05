import {Page} from '@playwright/test';
import {SignInPage} from '../pages/admin/SignInPage';
import {ViewQuestionnairePage} from '../pages/admin/ViewQuestionnairePage';
import {AddQuestionnairePage} from '../pages/admin/AddQuestionnairePage';
import {EditQuestionnairePage} from "../pages/admin/EditQuestionnairePage";

export async function doSignIn(page: Page, username: string, password: string): Promise<ViewQuestionnairePage> {
    const signInPage = new SignInPage(page);
    const viewQuestionnairePage = new ViewQuestionnairePage(page);

    await signInPage.navigateTo('/');
    await signInPage.verifyOnSignInPage();
    await signInPage.signIn(username, password);

    await viewQuestionnairePage.validatePartialUrlMatches('/admin/questionnaires');
    return viewQuestionnairePage;
}

export async function goToAddQuestionnairePage(page: Page): Promise<AddQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    const addQuestionnairePage = new AddQuestionnairePage(page);

    await viewQuestionnairePage.clickCreateNewQuestionnaire();
    await addQuestionnairePage.verifyOnAddQuestionnairePage();

    return addQuestionnairePage;
}

export async function goToEditQuestionnairePage(page: Page): Promise<EditQuestionnairePage> {
    const viewQuestionnairePage = new ViewQuestionnairePage(page);
    const newQuestionnairePage = new AddQuestionnairePage(page);
    const editQuestionnairePage = new EditQuestionnairePage(page);
    
    await viewQuestionnairePage.clickCreateNewQuestionnaire();
    await newQuestionnairePage.verifyOnAddQuestionnairePage();
    await newQuestionnairePage.createNewQuestionnaire('Automation Questionnaire');
    await editQuestionnairePage.validatePartialUrlMatches('/questionnaire/');

    return editQuestionnairePage;
}