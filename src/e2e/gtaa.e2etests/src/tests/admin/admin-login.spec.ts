import {test} from "@playwright/test";
import {SignInPage} from "../../pages/admin/SignInPage";

test.describe('Care Terms Explained Page Tests', () => {
    let signInPage: SignInPage;
    
    test.beforeEach(async ({page}) => {
        signInPage = new SignInPage(page);
        await signInPage.navigateTo('/');
        await signInPage.verifyOnSignInPage();
    });

    test('User can sign in with valid credentials', async ({page}) => {
        //await signInPage.signIn('testuser', 'P@ssword123'); //TBC whether it's dynamic or pool of users
        await signInPage.validateURLContains('/dashboard');
    });
});